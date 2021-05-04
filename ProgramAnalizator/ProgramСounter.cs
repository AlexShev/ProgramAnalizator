using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgramAnalizator
{
    class ProgramСounter : Сounter
    {
        // метод по анализу програмного кода из файла
        public double Analiz(string file)
        {
            Inizializet();

            _myMethods = new();

            // открытие файла
            System.IO.StreamReader sr = new(file, Encoding.Default);
            string line;

            // пока не дойдём до класса Программа - пропускаем строки
            while (!sr.ReadLine().Contains("Program")) { }

            // считывание строки
            while ((line = sr.ReadLine()) != null && !line.Contains("Main("))
            {
                //  если строка содержит метку подпрограммы
                if (IsMathod(line) && !line.Contains("//"))
                {
                    // счётчик по анализу для подпрограммы
                    MethodCounter caunter = new(sr, line, _myMethods);

                    // добавление найденрго метода в словарь для собственно разработанных методов
                    _myMethods.Add(caunter);

                    caunter.Modificate(_myMethods);
                }
                else
                {
                    AnalizString(line, _myMethods);
                }
            }

            while ((line = sr.ReadLine()) != null)
            {
                // анализ строки
                AnalizString(line, _myMethods);
            }

            // закрытие файла
            sr.Close();

            // цикл для прибавления того что храниться в подпрограммах
            foreach (var method in _myMethods)
            {
                // сколько раз метод встречается в коде
                int k = (_writingSelfMethods.ContainsKey(method.MethodName)) ? _writingSelfMethods[method.MethodName] : 0;

                if (k == 0)
                {
                    continue;
                }

                // проход по контейнеру операторов
                foreach (var it in method._operators)
                {
                    AddToConteiner(_operators, it.Key, it.Value * k);
                }
                // проход по контейнеру операндов
                foreach (var it in method._operands)
                {
                    if (!method.Param.Contains(it.Key))
                    {
                        AddToConteiner(_operands, it.Key, it.Value * k);
                    }
                }
                // проход по контейнеру стандартных методов
                foreach (var it in method._methods)
                {
                    AddToConteiner(_methods, it.Key, it.Value * k);
                }
                foreach (var it in method._constants)
                {
                    AddToConteiner(_constants, it.Key, it.Value * k);
                }
            }

            // возврат результата
            return СalculateVolume(SumDictionaryKey(_operators) + SumDictionaryKey(_methods),
                SumDictionaryKey(_operands) + SumDictionaryKey(_constants), _operators.Count + _methods.Count, _operands.Count + _constants.Count);
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
