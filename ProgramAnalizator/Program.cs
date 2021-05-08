using System;
using System.Linq;

namespace ProgramAnalizator
{
    class Program
    {
        static void Main(string[] args)
        {

            // C:\Users\Alex\source\repos\programula\programula\Program.cs
            // C:\Users\Alex\source\repos\programula_2\programula_2\Program.cs
            string temp = "C:\\Users\\Alex\\source\\repos\\programula_2\\programula_2\\Program.cs";

            Console.WriteLine("V = " + new ProgramСounter().Analiz(temp));
        }
    }
}