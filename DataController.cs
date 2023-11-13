using AISLab2.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISLab2
{
    internal class DataController
    {
        public static void AddStudent(Student student, StudentContext db)
        {
            db.Students.Add(student);
            db.SaveChanges();
        }

        public static void DeleteStudent(int studId, StudentContext db)
        {
            Student student = db.Students.FirstOrDefault(s => s.Id == studId);
            if (student != null)
            {
                db.Students.Remove(student);
                db.SaveChanges();
            }
        } 
        public static async Task<List<Student>> GetStudents(StudentContext db)
        {
            return await Task.Run(() => db.Students.ToList());
        }

    }
}
