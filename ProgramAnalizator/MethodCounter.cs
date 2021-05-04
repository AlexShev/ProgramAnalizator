using System.Collections.Generic;
using System.Linq;

namespace ProgramAnalizator
{
    class MethodCounter : Сounter
    {
        public string MethodName { private set; get; }

        public string[] Param { private set; get; }

        public MethodCounter(System.IO.StreamReader sr, string signatura, List<MethodCounter> myMethods) 
        {
            Inizializet();

            string[] words = Split(signatura);

            List<string> inputData = new();

            // поиск названия метода
            foreach (var word in words)
            {
                if (word.Contains("("))
                {
                    // название метода
                    MethodName = word;
                }
                else if (word != ")" && word != "," && !_SPECIAL.Contains(word))
                {
                    inputData.Add(word);
                }

            }

            Param = inputData.ToArray();

            int skobkiCounter = 0;

            // пока не встретится метка - анализировать строку
            while (true)
            {
                var temp = Split(sr.ReadLine());

                if (temp.Contains("{"))
                {
                    ++skobkiCounter;
   
                    continue;
                }
                else if (temp.Contains("}"))
                {
                    --skobkiCounter;

                    if (skobkiCounter == 0)
                    {
                        break;
                    }

                    continue;
                }

                AnalizString(temp, myMethods); // в line содержится очередная строчка из файла
            }
        }

        public void Modificate(List<MethodCounter> myMethods) 
        {
            int index = -1;

            while (myMethods[++index].MethodName != MethodName)
            {
                // сколько раз метод встречается в этом методе
                int k = (_writingSelfMethods.ContainsKey(myMethods[index].MethodName)) ?
                    _writingSelfMethods[myMethods[index].MethodName] : 0;

                if (k == 0)
                {
                    continue;
                }

                // проход по контейнеру операторов
                foreach (var it in myMethods[index]._operators)
                {
                    AddToConteiner(_operators, it.Key, it.Value * k);
                }
                // проход по контейнеру операндов
                foreach (var it in myMethods[index]._operands)
                {
                    if (!myMethods[index].Param.Contains(it.Key))
                    {
                        AddToConteiner(_operands, it.Key, it.Value * k);
                    }
                }
                // проход по контейнеру стандартных методов
                foreach (var it in myMethods[index]._methods)
                {
                    AddToConteiner(_methods, it.Key, it.Value * k);
                }
                foreach (var it in myMethods[index]._writingSelfMethods)
                {
                    AddToConteiner(_writingSelfMethods, it.Key, it.Value * k);
                }
            }
        }
    }
}
