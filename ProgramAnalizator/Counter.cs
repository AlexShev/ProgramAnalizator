using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgramAnalizator
{
    class Сounter
    {
        protected void AnalizString(string[] words, List<MethodCounter> myMethods) 
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
                flagSkobkaMyMethods = MyMethodsContainThisMethod(words[0], myMethods) != null;

                if (flagSkobkaMyMethods)
                {
                    AddToConteiner(_writingSelfMethods, words[0]);

                    methods.Push(MyMethodsContainThisMethod(words[0], myMethods));

                    paramNamber.Push(0);
                }
                else
                {
                    AddToConteiner(_methods, words[0]);
                }

                skobki.Push(words[0]);

                AddToConteiner(_operators, ".", CountWords(words[0], "."));

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

                        flagSkobkaMyMethods = MyMethodsContainThisMethod(skobki.Count != 0 ? skobki.Peek() : "", myMethods) != null;
                    }
                    else if (words[i] == ",")
                    {
                        if (!flagSkobkaMyMethods)
                        {
                            AddToConteiner(_operators, words[i]);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // это метод
                    else if(IsMathod(words[i]))
                    {
                        if (flagSkobkaMyMethods)
                        {
                            var temp = methods.Peek();

                            var name = temp.Param[paramNamber.Peek()];

                            paramNamber.Push(paramNamber.Pop() + 1);

                            AddToConteiner(_operators, "=");

                            AddToConteiner(_operands, (name + $"_{_writingSelfMethods[temp.MethodName]}_{paramNamber.Peek()}"), temp._operands[name] + 1);
                        }

                        flagSkobkaMyMethods = MyMethodsContainThisMethod(words[i], myMethods) != null;

                        if (flagSkobkaMyMethods)
                        {
                            if (MyMethodsContainThisMethod(skobki.Peek(), myMethods) == null)
                            {
                                AddToConteiner(_operators, ";", -1);
                            }

                            AddToConteiner(_writingSelfMethods, words[i]);

                            methods.Push(MyMethodsContainThisMethod(words[i], myMethods));

                            paramNamber.Push(0);
                        }
                        else
                        {
                            AddToConteiner(_methods, words[i]);

                            AddToConteiner(_operands, words[i]);
                        }
                        
                        skobki.Push(words[i]);
                    }
                    // это операторы
                    else if (_OPERATORS.Contains(words[i]))
                    {
                        // увеличение счётчика для операндов
                        AddToConteiner(_operators, words[i]);
                    }
                    else if (flagSkobkaMyMethods) // это входной параметр инлайн
                    {
                        var temp = methods.Peek();

                        var name = temp.Param[paramNamber.Peek()];

                        // увеличение счётчика для операндов
                        AddToConteiner(_operands, words[i], temp._operands[name] + 1);

                        AddToConteiner(_operators, "=");

                        paramNamber.Push(paramNamber.Pop() + 1);
                    } // это операнд
                    else
                    {
                        // увеличение счётчика для операндов
                        AddToConteiner(_operands, words[i]);
                    }

                    // переменная может быть полем, к которому обращаются через оператор .
                    AddToConteiner(_operators, ".", CountWords(words[i], "."));
                }
            }
        }

        protected void ConstantNotifay(List<MethodCounter> myMethods, Stack<string> skobki, bool flagSkobkaMyMethods, Stack<int> paramNamber, string word, Stack<MethodCounter> methods)
        {
            if (flagSkobkaMyMethods)
            {
                var temp = methods.Peek();

                var name = temp.Param[paramNamber.Peek()];

                paramNamber.Push(paramNamber.Pop() + 1);

                AddToConteiner(_operators, "=");

                AddToConteiner(_operands, (name + $"_{_writingSelfMethods[temp.MethodName]}_{paramNamber.Peek()}"), temp._operands[name] + 1);

                AddToConteiner(_operators, ";");
            }

            AddToConteiner(_constants, word);
        }

        // анализ одной строки
        protected void AnalizString(string line, List<MethodCounter> myMethods)
        {
            line = line.Trim();

            if (line == string.Empty)
                return;

            // разделение строки по прбелу
            string[] words = Split(line);
 
            if(words.Length > 0)
                AnalizString(words, myMethods);
        }

        private MethodCounter MyMethodsContainThisMethod(string name, List<MethodCounter> myMethods) 
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

        // сколько раз данная продстока в данной строке
        protected static int CountWords(string baseString, string substring) => baseString.Split(new[] { substring }, StringSplitOptions.None).Length - 1;

        // очистка полей
        protected void Inizializet()
        {
            _operators = new();
            _operands = new();
            _methods = new();
            _writingSelfMethods = new();
            _constants = new();
        }

        // операторы
        private static readonly string[] _OPERATORS = {  "}", "(", "[", ",", ".", ";", "=", "+", "-", "*", "/", "<<", ">>",
            "+=", "-=", "*=", "/=", "<<=", ">>=", "++", "--", "new", "==", "!=", "<=", ">=", ">", "<", "!"};

        // символы которые нужно пропускать
        protected static readonly string[] _SPECIAL = { "{", "]", "int", "string", "double", "char", "float", "return", "static", "void" };

        // словарь в котором по ключу храниться количество операторов в программном коде
        public Dictionary<string, int> _operators { private set; get; }

        // словарь в котором по ключу храниться количество операндов в программном коде
        public Dictionary<string, int> _operands { private set; get; }

        public Dictionary<string, int> _constants { private set; get; }

        // словарь в котором по ключу храниться количество библеотечных методов в программном коде
        public Dictionary<string, int> _methods { private set; get; }

        // словарь в котором по ключу храниться количество библеотечных методов в программном коде
        public Dictionary<string, int> _writingSelfMethods { private set; get; }
    }
}
