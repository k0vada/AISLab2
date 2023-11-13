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



        public static async Task<string> FullOutputAsync(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            List<Student> students = await GetStudents(db);
            if (students.Count == 0)
                return "Нет доступных записей в базе данных.";

            string response = "Список студентов:\n";
            foreach (Student student in students)
            {
                string gender = student.Gender ? "Мужской" : "Женский";
                response += $"\nId: {student.Id}\nФИО: {student.LastName} {student.FirstName} {student.MiddleName}\nПол: {gender}\nДата рождения: {student.Birthday:yyyy-MM-dd}\n";
            }
            return response;
        }

        public static async Task<string> OutputByIdAsync(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            string response = "Введите Id студента: ";
            await SendDataAsync(udpSocket, senderEndPoint, response);
            string studentId = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

            if (!int.TryParse(studentId, out int id))
                return "Некорректный ввод Id.";

            var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student != null)
            {
                string gender = student.Gender ? "Мужской" : "Женский";
                response = $"Id: {student.Id}\nФИО: {student.LastName} {student.FirstName} {student.MiddleName}\nПол: {gender}\nДата рождения: {student.Birthday:yyyy-MM-dd}\n";
            }
            else 
                response = "Студент с указанным Id не найден.";

            return response;
        }

        public static async Task<string> SaveRecordsAsync(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            try
            {
                await db.SaveChangesAsync();
                return "Изменения сохранены в базе данных.";
            }
            catch (Exception ex)
            {
                return $"Ошибка при сохранении данных: {ex.Message}";
            }
        }

        public static async Task<string> DeleteRecordByIdAsync(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            await SendDataAsync(udpSocket, senderEndPoint, "Введите Id студента для удаления: ");
            var studentId = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

            if (!int.TryParse(studentId, out int id))
                return "Некорректный ввод Id.";

            var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student != null)
            {
                DeleteStudent(id, db);
                await db.SaveChangesAsync();
                return $"Студент с Id {id} удален из базы данных.";
            }
            else
            {
                return "Студент с указанным Id не найден.";
            }
        }

        public static async Task<string> AddRecordAsync(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        {
            await SendDataAsync(udpSocket, senderEndPoint, "Введите Id студента: ");
            var studentId = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

            if (!int.TryParse(studentId, out int id))
                return "Некорректный ввод Id.";

            await SendDataAsync(udpSocket, senderEndPoint, "Введите имя студента: ");
            var firstName = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

            await SendDataAsync(udpSocket, senderEndPoint, "Введите фамилию студента: ");
            var lastName = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

            await SendDataAsync(udpSocket, senderEndPoint, "Введите Отчество студента: ");
            var middleName = ReceiveDataAsync(udpSocket, senderEndPoint).Result;

            await SendDataAsync(udpSocket, senderEndPoint, "Введите Пол студента (Мужской/Женский): ");
            var genderInput = ReceiveDataAsync(udpSocket, senderEndPoint).Result;
            var isMale = genderInput.Trim().Equals("Мужской", StringComparison.OrdinalIgnoreCase);

            await SendDataAsync(udpSocket, senderEndPoint, "Введите Дату рождения студента: ");
            var birthdateInput = ReceiveDataAsync(udpSocket, senderEndPoint).Result;
            if (DateTime.TryParse(birthdateInput, out DateTime birthdate))
            {
                var newStudent = new Student
                {
                    Id = id,
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    Gender = isMale,
                    Birthday = birthdate
                };

                AddStudent(newStudent, db);
                await db.SaveChangesAsync();

                return "Новая запись студента добавлена.";
            }
            else
            {
                return "Некорректный формат даты. Введите дату в формате ГГГГ-ММ-ДД.";
            }
        }












        #region 2
        //public static async void SendListOfStudents(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        //{
        //    try
        //    {
        //      //  Console.WriteLine("Вызван метод FullOutputAsync");
        //        List<Student> students = db.Students.ToList();
        //        await SendDataAsync(udpSocket, senderEndPoint, JsonSerializer.Serialize(students));
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка при вызове метода FullOutputAsync: , {ex.Message} ");
        //    }
        //}

        //public static async Task ReceiveDataForAddAsync(Socket udpSocket, EndPoint senderEndpoint, StudentContext db)
        //{
        //    string receivedStudent = await ReceiveDataAsync(udpSocket, senderEndpoint);
        //    try
        //    {
        //        AddStudent(JsonSerializer.Deserialize<Student>(receivedStudent), db);
        //        Console.WriteLine("Запись добавлена");
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка при вызове метода ReceiveDataForAddAsync , {ex.Message} ");
        //    }
        //}

        //public static async Task RecieveDataForDeleteAsync(Socket udpSocket, EndPoint senderEndPoint, StudentContext db)
        //{
        //    string receivedIndex = await ReceiveDataAsync(udpSocket, senderEndPoint);
        //    try
        //    {
        //        if (int.TryParse(receivedIndex, out int index))
        //        {
        //            DeleteStudent(index, db);
        //            Console.WriteLine($"Удален студент с id = {index}");
        //        }
        //        else Console.WriteLine("Ошибка ввода id");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка при вызове метода RecieveDataForDeleteAsync , {ex.Message} ");
        //    }
        //}

        /*
        public static async Task<string> FullOutputAsync(List<Student> Students)
        {
            Console.WriteLine("Вызван метод FullOutputAsync");
            return await Task.Run(() => FullOutput(Students));
        }
        /////////
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
          */
        #endregion
    }
}
