using AISLab2.DataBase;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static AISLab2.Commands;

namespace AISLab2
{
    public class Server
    {
        const string ip = "127.0.0.1";
        const int port = 8081;
        static async Task Main(string[] args)
        {
            Logger logger = LogManager.GetCurrentClassLogger();

            #region database init
            using (var db = new StudentContext())
            {

                Console.Write("Students:\n");
                foreach (Student student in db.Students)
                {
                    Console.WriteLine($"student id: {student.Id}, student's name: {student.FirstName}, student's surname: {student.LastName}");
                }
                try
                {

                    var udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                    var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    udpSocket.Bind(udpEndPoint);
                    await ProcessClientCommand(udpSocket, logger, db);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    logger.Error($"Socket/EndPoint error: {e.Message}");
                }
            }
            #endregion
        }

        private static async Task ProcessClientCommand(Socket udpSocket, Logger logger, StudentContext db)
        {
            string serverAnswer; // Переменная ответа сервера клиенту
            string data; // Cообщения от клиента
            IPAddress clientIP = IPAddress.Parse("127.0.0.1");
            const int clientPort = 8082;
            EndPoint senderEndPoint = new IPEndPoint(clientIP, clientPort); // Эндпоинт клиента(отправителя сообщений) сюда будет сохранен адресс 

            logger.Info("Server started at" + DateTime.Now);
            Console.WriteLine("Сервер успешно запущен!");

            while (true)
            {
                data = ReceiveData(udpSocket, senderEndPoint);
                if (Check.UintCheck(data))
                {
                    Console.WriteLine("Выбор пользователя: " + data);
                    logger.Info("Client choice:" + data);
                    switch (Convert.ToInt32(data))
                    {
                        case 1: // Вывод всех записей на экран 
                            serverAnswer = await FullOutputAsync(udpSocket, senderEndPoint, db);
                            break;
                        case 2: // Вывод записи по номеру 
                            serverAnswer = await OutputByIdAsync(udpSocket, senderEndPoint, db); 
                            break;
                        case 3: // Запись данных
                            serverAnswer = await SaveRecordsAsync(udpSocket, senderEndPoint, db);
                            break;
                        case 4: // Удалить запись по номеру
                            serverAnswer = await DeleteRecordByIdAsync(udpSocket, senderEndPoint, db);
                            break;
                        case 5: // Добавление новой записи
                            serverAnswer = await AddRecordAsync(udpSocket, senderEndPoint, db);
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