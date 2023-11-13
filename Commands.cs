using AISLab2.DataBase;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static AISLab2.DataInteraction;
using static AISLab2.DataController;
using System.Data.Entity;

namespace AISLab2
{
    class Commands
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        public static string ReceiveData(Socket udpSocket, EndPoint senderEndPoint)
        {
            try
            {
                byte[] buffer = new byte[4096]; //буфер, сюда принимаем значения (хранилище данных)
                int size; //кол-во реальных полученных байтов 
                string data; //сообщения от клиента
                size = udpSocket.ReceiveFrom(buffer, ref senderEndPoint);  //принимаем данные 
                data = Encoding.UTF8.GetString(buffer, 0, size); //записываем полученные данные в виде строки 
                return data;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при приеме данных: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> ReceiveDataAsync(Socket udpSocket, EndPoint senderEndPoint)
        {
            return await Task.Run(() => ReceiveData(udpSocket, senderEndPoint));
        }
        public static void SendData(Socket udpSocket, EndPoint senderEndPoint, string serverAnswer)
        {
            try
            {
                byte[] responseBuffer;
                responseBuffer = Encoding.UTF8.GetBytes(serverAnswer);
                udpSocket.SendTo(responseBuffer, SocketFlags.None, senderEndPoint);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке данных: {ex.Message}");
            }
            
        }
        public static async Task SendDataAsync(Socket udpSocket, EndPoint senderEndPoint, string serverAnswer)
        {
            await Task.Run(() => SendData(udpSocket, senderEndPoint, serverAnswer));
        }


        public static async void SendListOfStudents(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            try
            {
                List<Student> students = db.Students.ToList();
                await SendDataAsync(udpSocket, senderEndPoint, JsonSerializer.Serialize(students));
                logger.Info("Cписок студентов отправлен клиенту.");
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка при отправке списка студентов: {ex.Message}");
            }
        }

        public static async Task ReceiveDataForWriteAsync(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            string receivedStudent = await ReceiveDataAsync(udpSocket, senderEndPoint);
            try
            {
                AddStudent(JsonSerializer.Deserialize<Student>(receivedStudent), db);
                logger.Info("Студент успешно добавлен.");
            }
            catch(Exception ex)
            {
                logger.Error($"Ошибка при добавлении студента: {ex.Message}");
            }
        }

        public static async Task ReceiveDataForDelete(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            string receivedIndex = await ReceiveDataAsync(udpSocket, senderEndPoint);
            try
            {
                if(int.TryParse(receivedIndex, out int index))
                {
                    DeleteStudent(index, db);
                    logger.Info($"Студент c Id {index} успешно удален.");
                }
                else
                    logger.Error($"Ошибка при удалении студента.");
            }
            catch(Exception ex)
            {
                logger.Error($"Ошибка при удалении студента: {ex.Message}");
            }
        }

    }
}
