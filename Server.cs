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
        static IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        const int serverPort = 8081;

        static IPAddress clientIP = IPAddress.Parse("127.0.0.1");
        const int clientPort = 8082;
       // const string ip = "127.0.0.1";
       // const int port = 8081;
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

                    var udpEndPoint = new IPEndPoint(serverIP, serverPort);
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
           // string serverAnswer; // Переменная ответа сервера клиенту
            string data; // Cообщения от клиента
           // IPAddress clientIP = IPAddress.Parse("127.0.0.1");
            //const int clientPort = 8082;
            EndPoint senderEndPoint = new IPEndPoint(clientIP, clientPort); // Эндпоинт клиента(отправителя сообщений) сюда будет сохранен адресс 

            logger.Info("Сервер успешно запущен" + DateTime.Now);
            Console.WriteLine("Сервер успешно запущен!");

            while (true)
            {
                data = ReceiveData(udpSocket, senderEndPoint);
                switch (data)
                {
                    case "Add":
                        await ReceiveDataForWriteAsync(udpSocket, senderEndPoint, db);
                        break;
                    case "Delete":
                        await ReceiveDataForDeleteAsync(udpSocket, senderEndPoint, db);
                        break;
                    case "ListOfStudents":
                        SendListOfStudents(udpSocket, senderEndPoint, db);
                        break;
                    default:
                        logger.Error("Ошибка при приеме данных от клиента");
                        break;
                }
            }
        }
    }
}