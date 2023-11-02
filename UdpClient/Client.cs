using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpClient
{
    class Client
    {
        static void Main(string[] args)
        {
            const string ip = "127.0.0.1";
            const int port = 8082;

            EndPoint udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port); // точка подключения
            EndPoint udpServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081);
            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(udpEndPoint); // переводим сокет в режим ожидания, связывает соект и эндпоинт
            Console.WriteLine("Клиент успешно запущен!");
            Console.WriteLine("Введите название файла");
            string fileName = Console.ReadLine();
            udpSocket.SendTo(Encoding.UTF8.GetBytes(fileName), udpServerEndPoint);
            Console.WriteLine("\nМЕНЮ ДЕЙСТВИЙ");
            Console.WriteLine("Выберете действие и нажмите соответствующую клавишу");
            Console.WriteLine("     1. Вывод всех записей на экран");
            Console.WriteLine("     2. Вывод записи по ID");
            Console.WriteLine("     3. Сохранение данных в файл");
            Console.WriteLine("     4. Удаление записи по ID");
            Console.WriteLine("     5. Добавление записи в файл");
            Console.WriteLine("     Esc. Закрытие приложения");

            while (true)
            {
                byte[] buffer = new byte[2048]; //буфер, сюда принимаем значения (хранилище данных)
                int size; //кол-во реальных полученных байтов 
                var data = new StringBuilder(); //сообщения от клиента
                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    udpSocket.Close();
                    Console.WriteLine("Клиент завершил работу.");
                    Environment.Exit(0);
                }
                string message = Console.ReadLine();
                udpSocket.SendTo(Encoding.UTF8.GetBytes(message), udpServerEndPoint);
                do
                {
                    size = udpSocket.ReceiveFrom(buffer, ref udpServerEndPoint); //прием данных
                    data.Append(Encoding.UTF8.GetString(buffer)); //записываем полученные данные в виде строки 
                }
                while (udpSocket.Available > 0);
               Console.WriteLine(data);
            }
        }
    }
}
