using CsvHelper;
using CsvHelper.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static AISLab2.Commands;

namespace AISLab2
{
    public class Server
    {
        static async Task Main(string[] args)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            const string ip = "127.0.0.1";
            const int port = 8081; 

            var udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port); // точка подключения
            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(udpEndPoint); // переводим сокет в режим ожидания, связывает соект и эндпоинт
            IPAddress clientIP = IPAddress.Parse("127.0.0.1");
            const int clientPort = 8082; 
            EndPoint senderEndPoint = new IPEndPoint(clientIP, clientPort); // Эндпоинт клиента(отправителя сообщений) сюда будет сохранен адресс 

            string fileName = ReceiveData(udpSocket, senderEndPoint); // Принимаем название файла
            Console.WriteLine($"Получено название файла от клиента: {fileName}");

            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);
            List<Student> Students = null;
            if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture); // Используется для настройки библиотеки CsvHelper
                config.ShouldUseConstructorParameters = type => false; // Передача библиотеке разрешение на присваивание значений полям класса без наличия конструктора
                try
                {
                    using (var reader = new StreamReader(filePath)) // Открытие файла в режиме чтения
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) // Чтение данных из csv файла с учетом указанных настроек CsvConfiguration
                    {
                        IEnumerable<Student> Records = csv.GetRecords<Student>(); // Базовый интерфейс для всех неуниверсальных коллекций
                        Students = Records.ToList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошла ошибка при чтении файла: {ex.Message}");
                }
            }
            else
            {
                SendData(udpSocket, senderEndPoint, "Файл не существует или не является файлом формата CSV.");
                Console.WriteLine("Файл не существует или не является файлом формата CSV.");
                return;
            }
            await ProcessClientCommand(udpSocket, senderEndPoint, Students, filePath, logger);
        }

        private static async Task ProcessClientCommand(Socket udpSocket, EndPoint senderEndPoint, List<Student> Students, string filePath, Logger logger)
        {
            string serverAnswer; // Переменная ответа сервера клиенту
            logger.Info("Server started at" + DateTime.Now);
            Console.WriteLine("Сервер успешно запущен!");

            while (true)
            {
                string data = ReceiveData(udpSocket, senderEndPoint); //сообщения от клиента

                if (Check.UintCheck(data))
                {
                    Console.WriteLine("Выбор пользователя: " + data);
                    logger.Info("Client choice:" + data);
                    switch (Convert.ToInt32(data))
                    {
                        case 1: // Вывод всех записей на экран 
                            serverAnswer = await FullOutputAsync(Students);
                            break;
                        case 2: // Вывод записи по номеру 
                            serverAnswer = await OutputByIdAsync(udpSocket, senderEndPoint, Students);
                            break;
                        case 3: // Запись данных в файл 
                            serverAnswer = await SaveRecordsAsync(Students, filePath);
                            break;
                        case 4: // Удалить запись по номеру
                            serverAnswer = await DropRecordAsync(udpSocket, senderEndPoint, Students);
                            break;
                        case 5: // Добавление новой записи
                            serverAnswer = await AddRecordAsync(udpSocket, senderEndPoint, Students);
                            break;
                        default:
                            serverAnswer = "\nОшибка ввода. Выберите и введите цифру от 1  до 5";
                            break;
                    }
                    await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                }
                else
                {
                    serverAnswer = "\nОшибка ввода. Попробуйте заново.";
                    await SendDataAsync(udpSocket, senderEndPoint, serverAnswer);
                    logger.Error("Incorrect user input");
                    Console.WriteLine("Получены недопустимые данные");

                }
            }
        }
    }
}

//if ()
//{
//    udpSocket.Close();
//    Console.WriteLine("Сервер завершил работу.");
//    break;
//}
