using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Model
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public bool Gender { get; set; }
        public DateTime Birthday { get; set; }

        public Student(int Id, string FirstName, string LastName, string MiddleName, bool Gender, DateTime Birthday)
        {
            this.Id = Id;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.MiddleName = MiddleName;
            this.Gender = Gender;
            this.Birthday = Birthday;
        }

        public Student() { }
    }
}
