using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramAnalizator
{
    class Сounter
    {
        protected static bool IsMathod(string word) => word.Contains("(") && !word.Equals("(");

        protected static bool IsDigital(string word)
        {
            try
            {
                double.Parse(word);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected static bool IsString(string word) => word.Contains('\"') || word.Contains('\'');

        protected static string[] Split(string line) => line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // сколько раз данная продстока в данной строке
        protected static int CountWords(string baseString, string substring) => baseString.Split(new[] { substring }, StringSplitOptions.None).Length - 1;

        // прибавить слово и количество слов
        protected static void AddToConteiner(Dictionary<string, int> dictionary, string data, int count = 1)
        {
            if (dictionary.ContainsKey(data))
            {
                dictionary[data] += count;
            }
            else
            {
                dictionary.Add(data, count);
            }
        }
    }
}
