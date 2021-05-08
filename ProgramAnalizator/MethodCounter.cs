using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramAnalizator
{
    class MethodCounter : Сounter
    {
        // конструктор для создания объекта типа счётчика для метода
        public MethodCounter(System.IO.StreamReader sr, string signatura, List<MethodCounter> myMethods) 
        {
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
                }
                else if (temp.Contains("}"))
                {
                    --skobkiCounter;

                    if (skobkiCounter == 0)
                    {
                        break;
                    }
                }
                else
                {
                    AnalizString(temp, myMethods); // в line содержится очередная строчка из файла
                }
            }

            ReplaseMyMethods(myMethods);
        }

        private void AnalizString(string[] words, List<MethodCounter> myMethods)
        {
            int j = 0;

            if (words.Length == 0)
                return;

            Stack<string> skobki = new();

            Stack<MethodCounter> methods = new();

            Stack<int> paramNamber = new();

            bool flagSkobkaMyMethods = false;

            // это метод возвращающий void
            if (IsMathod(words[0]))
            {
                MethodNotifay(myMethods, skobki, ref flagSkobkaMyMethods, paramNamber, words[0], methods);

                j = 1;
            }

            // анализ полученного слова
            for (int i = j; i < words.Length; i++)
            {
                // если слова не надо пропускать
                if (!_SPECIAL.Contains(words[i]))
                {
                    // коментарии в анализе не участвуют
                    if (words[i] == "//")
                    {
                        return;
                    }

                    // если это строковый тип данных у которого в строке встречается пробел
                    if (CountWords(words[i], "\"") == 1)
                    {
                        string temp = words[i++] + " ";

                        // пока не встретиться закрывающая кавычка
                        while (!words[i].Contains("\""))
                        {
                            temp += (words[i++] + " ");
                        }

                        // добавление в список констант
                        ConstantNotifay(myMethods, skobki, flagSkobkaMyMethods, paramNamber, temp + words[i], methods);
                    }
                    else if (IsDigital(words[i]) || IsString(words[i])) // если это консанта
                    {
                        // добавление в список констант
                        ConstantNotifay(myMethods, skobki, flagSkobkaMyMethods, paramNamber, words[i], methods);
                    }
                    else if (words[i] == "(")
                    {
                        skobki.Push(words[i]);

                        flagSkobkaMyMethods = false;

                        AddToConteiner(_operators, words[i]);
                    }
                    else if (words[i] == ")")
                    {
                        skobki.Pop();

                        if (flagSkobkaMyMethods)
                        {
                            paramNamber.Pop();

                            methods.Pop();
                        }

                        flagSkobkaMyMethods = MyMethodByName(skobki.Count != 0 ? skobki.Peek() : "", myMethods) != null;
                    }
                    else if (words[i] == ",")
                    {
                        if (!flagSkobkaMyMethods)
                        {
                            AddToConteiner(_operators, words[i]);
                        }
                    }
                    // это метод
                    else if (IsMathod(words[i]))
                    {
                        if (flagSkobkaMyMethods)
                        {
                            RepalseMethodParam(paramNamber, words[i], methods, false);
                        }

                        MethodNotifay(myMethods, skobki, ref flagSkobkaMyMethods, paramNamber, words[i], methods);

                        if (!flagSkobkaMyMethods)
                        {
                            AddToConteiner(_operands, words[i]);
                        }
                    }
                    // это операторы
                    else if (_OPERATORS.Contains(words[i]))
                    {
                        // увеличение счётчика для операндов
                        AddToConteiner(_operators, words[i]);
                    }
                    else if (flagSkobkaMyMethods) // это входной параметр инлайн
                    {
                        RepalseMethodParam(paramNamber, words[i], methods, true);

                        // переменная может быть полем, к которому обращаются через оператор .
                        AddToConteiner(_operators, ".", CountWords(words[i], "."));
                    } // это операнд
                    else
                    {
                        // увеличение счётчика для операндов
                        AddToConteiner(_operands, words[i]);

                        // переменная может быть полем, к которому обращаются через оператор .
                        AddToConteiner(_operators, ".", CountWords(words[i], "."));
                    }
                }
            }
        }

        // возращает анализатор метода по имени метода
        private MethodCounter MyMethodByName(string name, List<MethodCounter> myMethods)
        {
            foreach (var method in myMethods)
            {
                if (method.MethodName == name)
                {
                    return method;
                }
            }

            return null;
        }

        // поведение при константах
        private void ConstantNotifay(List<MethodCounter> myMethods, Stack<string> skobki, bool flagSkobkaMyMethods, Stack<int> paramNamber, string word, Stack<MethodCounter> methods)
        {
            if (flagSkobkaMyMethods)
            {
                RepalseMethodParam(paramNamber, word, methods, false);

                AddToConteiner(_operators, ";");
            }

            AddToConteiner(_constants, word);
        }

        private void MethodNotifay(List<MethodCounter> myMethods, Stack<string> skobki, ref bool flagSkobkaMyMethods, Stack<int> paramNamber, string word, Stack<MethodCounter> methods)
        {
            flagSkobkaMyMethods = MyMethodByName(word, myMethods) != null;

            if (flagSkobkaMyMethods)
            {
                if (MyMethodByName(skobki.Peek(), myMethods) == null)
                {
                    AddToConteiner(_operators, ";", -1);
                }

                AddToConteiner(_writingSelfMethods, word);

                methods.Push(MyMethodByName(word, myMethods));

                paramNamber.Push(0);
            }
            else
            {
                AddToConteiner(_methods, word);
            }

            skobki.Push(word);

            AddToConteiner(_operators, ".", CountWords(word, "."));
        }

        private void RepalseMethodParam(Stack<int> paramNamber, string word, Stack<MethodCounter> methods, bool isOperand) 
        {
            var temp = methods.Peek();

            var name = temp.Param[paramNamber.Peek()];

            AddToConteiner(_operators, "=");

            paramNamber.Push(paramNamber.Pop() + 1);

            if (isOperand)
            {
                // увеличение счётчика для операндов
                AddToConteiner(_operands, word, temp._operands[name] + 1);
            }
            else
            {
                AddToConteiner(_operands, (name + $"_{_writingSelfMethods[temp.MethodName]}"), temp._operands[name] + 1);
            }
        }

        private void ReplaseMyMethods(List<MethodCounter> myMethods) 
        {
            foreach (var method in myMethods)
            {
                // сколько раз метод встречается в этом методе
                int k = (_writingSelfMethods.ContainsKey(method.MethodName)) ?
                    _writingSelfMethods[method.MethodName] : 0;

                if (k == 0)
                {
                    continue;
                }

                // проход по контейнеру констант
                IteretingInDictionary1(method, _constants, method._constants, k);

                // проход по контейнеру операндов
                IteretingInDictionary1(method, _operands, method._operands, k);

                // проход по контейнеру операторов
                IteretingInDictionary2(_operators, method._operators, k);

                // проход по контейнеру стандартных методов
                IteretingInDictionary2(_methods, method._methods, k);

                IteretingInDictionary2(_writingSelfMethods, method._writingSelfMethods, k);
            }
        }

        void IteretingInDictionary1(MethodCounter method, Dictionary<string, int> myConteiner, Dictionary<string, int> anotherConteiner, int k)
        {
            foreach (var it in anotherConteiner)
            {
                if (!method.Param.Contains(it.Key))
                {
                    AddToConteiner(myConteiner, it.Key, it.Value * k);
                }
            }
        }

        void IteretingInDictionary2(Dictionary<string, int> myConteiner, Dictionary<string, int> anotherConteiner , int k)
        {
            foreach (var it in anotherConteiner)
            {
                AddToConteiner(myConteiner, it.Key, it.Value * k);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        /// поля

        // имя метода
        public string MethodName { private set; get; }

        // количество входящих параметров
        public string[] Param { private set; get; }

        // словарь в котором по ключу храниться количество операндов в программном коде
        public Dictionary<string, int> _operands { private set; get; } = new();

        public Dictionary<string, int> _constants { private set; get; } = new();

        // словарь в котором по ключу храниться количество операторов в программном коде
        public Dictionary<string, int> _operators { private set; get; } = new();

        // словарь в котором по ключу храниться количество библеотечных методов в программном коде
        public Dictionary<string, int> _methods { private set; get; } = new();

        // словарь в котором по ключу храниться количество библеотечных методов в программном коде
        public Dictionary<string, int> _writingSelfMethods { private set; get; } = new();

        //////////////////////////////////////////////////////////////////////////////////////////
        ///статические поля

        // операторы
        private static readonly string[] _OPERATORS = {  "}", "(", "[", ",", ".", ";", "=", "+", "-", "*", "/", "<<", ">>",
            "+=", "-=", "*=", "/=", "<<=", ">>=", "++", "--", "new", "==", "!=", "<=", ">=", ">", "<", "!"};

        // символы которые нужно пропускать
        private static readonly string[] _SPECIAL = { "{", "]", "int", "string", "double", "char", "float", "return", "static", "void" };
    }
}
