using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AISLab2
{
    public class Check
    {
        public static string StringCheck()
        {
            string input;
            while (true)
            {
                input = Console.ReadLine();
                if (IsAllLetters(input))
                    return input;
                else
                    Console.WriteLine("\nОшибка. Пользователь ввел недопустимые данные");
            }
        }

        public static bool IsAllLetters(string input)
        {
            return Regex.IsMatch(input, @"^[А-яа-я]+$");
        }

        public static string DateCheck()
        {
            string input;
            while (true)
            {
                input = Console.ReadLine();
                if (IsDateValid(input))
                {
                    return input;
                }
                else
                {
                    Console.WriteLine("\nОшибка. Пользователь ввел недопустимые данные");
                }
            }
        }

        public static bool IsDateValid(string input)
        {
            return Regex.IsMatch(input, @"^\d{4}-\d{2}-\d{2}$") && DateTime.TryParseExact(input, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _);
        }

        public static bool BoolCheck(string input)
        {
            bool varbool;
            try
            {
                varbool = bool.Parse(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool BoolCheck()
        {
            int varbool = 0;
            bool exit = false;
            while (!exit)
            {
                varbool = Convert.ToInt32(UintCheck());
                if (varbool == 0 || varbool == 1)
                {
                    exit = true;
                }
                else Console.WriteLine("\nОшибка. Пользователь ввел недопустимые данные");
            }
            if (varbool == 0) return true;
            return false;
        }


        public static bool UintCheck(string input)
        {
            uint varint = 0;
            try
            {
                varint = uint.Parse(input);
                return true;
            }
            catch
            {
                Console.WriteLine("\nОшибка. Пользователь ввел недопустимые данные");
                return false;
            }
        }
        public static uint UintCheck()
        {
            uint varint = 0;
            bool exit = false;
            while (!exit)
            {
                try
                {
                    varint = uint.Parse(Console.ReadLine());
                    exit = true;
                }
                catch
                {
                    Console.WriteLine("\nОшибка. Пользователь ввел недопустимые данные");
                }
            }
            return varint;
        }
    }
}
