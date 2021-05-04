using System;
using System.Linq;

namespace ProgramAnalizator
{
    class Program
    {
        static void Main(string[] args)
        {
            string temp = "C:\\Users\\Alex\\source\\repos\\programula\\programula\\Program.cs";

            Console.WriteLine("V = " + new ProgramСounter().Analiz(temp));
        }
    }
}