using AISLab2.DataBase;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISLab2
{
    public class DataInteraction
    {
        public static string SingleOutput(Student student)
        {
            string output;
            output = $"\nID: {student.Id}";
            output += $"\nФИО: {student.FirstName} {student.LastName} {student.MiddleName}";
            output += $"\nПол: {ConvertGender(student.Gender)}";
            output += $"\nДата рождения: {student.Birthday}\n";
            return output;
        }

        //public static string FullOutput(List<Student> Students)
        //{
        //    string output ="";
        //    foreach (Student student in Students)
        //    {
        //        output += SingleOutput(student);
        //    }
        //    return output;
        //}

        public static string OutputById(List<Student> Students, int searchId)
        {
            string output = "";
            if (searchId <= 0 && searchId > Students.Count())
            {
                return "\nОшибка.Студента с данным ID не существует";
            }

            foreach (Student student in Students)
            {
                if (student.Id == searchId)
                {
                    output += $"\nСтудент с ID = {searchId}";
                    output += SingleOutput(student);
                    return output;
                }
            }
            return "\nОшибка.Студента с данным ID не существует";
        }
      
        public static string DropRecord(List<Student> Students,StudentContext db , int dropId)
        {
            foreach (Student student in Students)
            {
                if (student.Id == dropId)
                {
                    Students.Remove(student);
                    db.Students.Remove(student);
                    return "\nЗапись успешно удалена.";
                }
            }
            return "\nОшибка.Студента с данным ID не существует";
        }

        //public static string AddRecord(List<Student> Students)  
        //{
        //    Console.WriteLine("\nВведите ID");
        //    uint varstudentid = Check.UintCheck();

        //    Console.WriteLine("Введите имя:");
        //    string varfirstname = Check.StringCheck();

        //    Console.WriteLine("Введите фамилию: ");
        //    string varlastname = Check.StringCheck();

        //    Console.WriteLine("Введите Отчество: ");
        //    string varmiddlename = Check.StringCheck();

        //    Console.WriteLine("Введите пол (0 - Женский, 1 - Мужской): ");
        //    bool vargender = Check.BoolCheck();

        //    Console.WriteLine("Введите дату рождения (ГГГГ-ММ-ДД): ");
        //    string varbirthday = Check.DateCheck();

        //    Student varstudent = new Student(varstudentid, varfirstname, varlastname, varmiddlename, vargender, varbirthday);
        //    Students.Add(varstudent);
        //    return "\nЗапись успешно добавлена.";
        //}
        //public static string SaveRecords(List<Student> Students, string filePath)
        //{
        //    string output;
        //    try
        //    {
        //        using (var swriter = new StreamWriter(filePath)) // Запись данных в файл
        //        using (var csvwriter = new CsvWriter(swriter, CultureInfo.InvariantCulture)) // Запись данных в формат CSV.
        //        {
        //            csvwriter.WriteRecords(Students);
        //        }
        //        output = "\n Данные успешно сохранены. \n";
        //    }
        //    catch (Exception ex)
        //    {
        //        output = $"Произошла ошибка при сохранении файла: {ex.Message}";
        //    }
        //    return output;
        //}

        public static string ConvertGender(bool tmpgender)
        {
            if (tmpgender == true) { return "М"; }
            return "Ж";
        }
    }
}
