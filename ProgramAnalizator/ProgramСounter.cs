using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramAnalizator
{
    class ProgramСounter : Сounter
    {
        // метод по анализу програмного кода из файла
        public double Analiz(string file)
        {
            _myMethods = new();

            // открытие файла
            System.IO.StreamReader sr = new(file, Encoding.Default);
            string line;

            // пока не дойдём до класса Программа - пропускаем строки
            while (!sr.ReadLine().Contains("Program")) { }

            // считывание строки
            while ((line = sr.ReadLine()) != null /*&& !line.Contains("Main(")*/)
            {
                //  если строка содержит метку подпрограммы
                if (IsMathod(line) && !line.TrimStart().StartsWith("//"))
                {
                    // счётчик по анализу для подпрограммы
                    MethodCounter caunter = new(sr, line, _myMethods);

                    // добавление найденрго метода в словарь для собственно разработанных методов
                    _myMethods.Add(caunter);
                }
            }

            // закрытие файла
            sr.Close();

            var temp = _myMethods[_myMethods.Count - 1];

            AddToConteiner(temp._operators, "{", 3);

            // возврат результата
            return СalculateVolume(SumDictionaryKey(temp._operators) + SumDictionaryKey(temp._methods),
                SumDictionaryKey(temp._operands) + SumDictionaryKey(temp._constants), temp._operators.Count + temp._methods.Count, temp._operands.Count + temp._constants.Count);
        }

        // подсчёт количества элементов в контейнере
        protected static int SumDictionaryKey(Dictionary<string, int> dictionary)
        {
            int counter = 0;

            foreach (var item in dictionary)
            {
                counter += item.Value;
            }

            return counter;
        }

        private static double СalculateVolume(int N1, int N2, int n1, int n2) => (N1 + N2) * Math.Log2(n1 + n2);

        // словарь в котором по ключу храниться количество собственно разработанных методов в программном коде
        public List<MethodCounter> _myMethods { private set; get; }
    }
}
