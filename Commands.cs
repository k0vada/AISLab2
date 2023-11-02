using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static AISLab2.DataInteraction;

namespace AISLab2
{
    class Commands
    {
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
        public static async Task<string> FullOutputAsync(List<Student> Students)
        {
            Console.WriteLine("Вызван метод FullOutputAsync");
            return await Task.Run(() => FullOutput(Students));
        }

        public static async Task<string> OutputByIdAsync(Socket udpSocket, EndPoint senderEndPoint, List<Student> Students)
        {
            Console.WriteLine("Вызван метод OutputByIdAsync");
            string serverAnswer;
            string data;
            serverAnswer = "\nВведите ID: ";
            await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
            data = ReceiveDataAsync(udpSocket, senderEndPoint).Result;
            if (Check.UintCheck(data))
            {
                return await Task.Run(() => OutputById(Students, Convert.ToUInt32(data)).ToString());
            }
            return "\nОшибка.Студента с данным ID не существует";
        }
        public static async Task<string> SaveRecordsAsync(List<Student> Students, string filePath) 
        {
            Console.WriteLine("Вызван метод SaveRecordsAsync");
            return await Task.Run(() => SaveRecords(Students, filePath));
        }

        public static async Task<string> DropRecordAsync(Socket udpSocket, EndPoint senderEndPoint, List<Student> Students) 
        {
            Console.WriteLine("Вызван метод DropRecordAsync");
            string serverAnswer;
            string data;
            serverAnswer = "\nВведите ID: ";
            await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
            data = ReceiveDataAsync(udpSocket, senderEndPoint).Result;
            if (Check.UintCheck(data))
            {
                return await Task.Run(() => DropRecord(Students, Convert.ToUInt32(data)).ToString());
            }
            return "\nОшибка.Студента с данным ID не существует";
        }

        public static async Task<string> AddRecordAsync(Socket udpSocket, EndPoint senderEndPoint, List<Student> Students)
        {
            Console.WriteLine("Вызван метод AddRecordAsync");
            string serverAnswer;
            string data;

            do
            {
                serverAnswer = "\nВведите ID: ";
                await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                data = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

                if (Check.UintCheck(data))
                {
                    serverAnswer = "\nВведите имя: ";
                    await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                    string varfirstname = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

                    serverAnswer = "\nВведите фамилию: ";
                    await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                    string varlastname = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

                    serverAnswer = "\nВведите Отчество: ";
                    await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                    string varmiddlename = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

                    serverAnswer = "\nВведите пол (0 - Женский, 1 - Мужской): ";
                    await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                    string genderInput = ReceiveDataAsync(udpSocket, senderEndPoint).Result;
                    bool vargender = Check.BoolCheck(genderInput);

                    serverAnswer = "\nВведите дату рождения (ГГГГ-ММ-ДД): ";
                    await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                    string varbirthday = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

                    if (Check.IsAllLetters(varfirstname) && Check.IsAllLetters(varlastname) && Check.IsAllLetters(varmiddlename) && Check.IsDateValid(varbirthday))
                    {
                        Student varstudent = new Student(Convert.ToUInt32(data), varfirstname, varlastname, varmiddlename, vargender, varbirthday);
                        Students.Add(varstudent);
                        return "\nЗапись успешно добавлена.";
                    }
                    else
                    {
                        return "\nОшибка. Введите корректные данные и попробуйте снова.";
                    }
                }
                else
                {
                    return "\nОшибка. Введите корректный ID и попробуйте снова.";
                }
            }
            while (true);
        }
    }
}
