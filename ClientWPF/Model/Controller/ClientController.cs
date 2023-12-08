using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClientWPF.Model.Controller
{
    public class ClientController
    {
        public static ObservableCollection<Student> GetStudents(Socket udpSocket, EndPoint serverEndPoint)
        {
            ObservableCollection<Student> studentList;
            SendData(udpSocket, serverEndPoint, "students"); // Отправляем запрос "students"
            string answer = ReceiveData(udpSocket, serverEndPoint);
            studentList = new ObservableCollection<Student>(JsonSerializer.Deserialize<List<Student>>(answer));
            return studentList;
        }
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке данных: {ex.Message}");
            }

        }
        public static async Task SendDataAsync(Socket udpSocket, EndPoint senderEndPoint, string serverAnswer)
        {
            await Task.Run(() => SendData(udpSocket, senderEndPoint, serverAnswer));
        }

    }
}
