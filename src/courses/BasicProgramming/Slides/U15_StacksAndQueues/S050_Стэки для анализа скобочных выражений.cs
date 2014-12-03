using System;
using System.Collections.Generic;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Стэки для анализа скобочных выражений", "ee93f10a-7ad1-468f-8141-3122f81372c4")]
	class S050_Стэки_для_анализа_скобочных_выражений
	{
		//#video IkpaIKNbtpk
		/*
		## Заметки по лекции
		*/
        public class Program
        {
            public static void MainX()
            {
                Console.WriteLine(IsCorrectString("(([])[])"));
                Console.WriteLine(IsCorrectString("((][])"));
                Console.WriteLine(IsCorrectString("((("));
                Console.WriteLine(IsCorrectString("(x)"));
            }

            public static bool IsCorrectString(string str)
            {
                var stack = new Stack<char>();
                foreach (var e in str)
                {
                    switch (e)
                    {
                        case '[':
                        case '(':
                            stack.Push(e);
                            break;

                        case ']':
                            if (stack.Count == 0) return false;
                            if (stack.Pop() != '[') return false;
                            break;

                        case ')':
                            if (stack.Count == 0) return false;
                            if (stack.Pop() != '(') return false;
                            break;
                        default:
                            return false;
                    }
                }
                return stack.Count == 0;
            }

            //Немного улучшим решение, обойдемся без DRY
            public static bool IsCorrectString1(string str)
            {
                var pairs = new Dictionary<char, char>();
                pairs.Add('(', ')');
                pairs.Add('[', ']');
                var stack = new Stack<char>();
                foreach (var e in str)
                {
                    if (pairs.ContainsKey(e)) stack.Push(e);
                    else if (pairs.ContainsValue(e))
                    {
                        if (stack.Count == 0 || pairs[stack.Pop()] != e) return false;
                    }
                    else return false;
                }
                return stack.Count == 0;
            }
        }
    }
}
