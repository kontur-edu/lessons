using System;
using System.Collections.Generic;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Стэки для вычислений", "a94f587c-d08e-403b-9c2a-45988f6422a2")]
	class S060_Стэки_для_вычислений
	{
		//#video T_HO3PDheNE
		/*
		## Заметки по лекции
		*/
        static void MainX()
        {
            Console.WriteLine(Compute("23+4*"));
        }
        static int Compute(string str)
        {
            var stack = new Stack<int>();
            foreach (var e in str)
            {
                if (e <= '9' && e >= '0')
                {
                    stack.Push(e - '0');
                    continue;
                }
                switch (e)
                {
                    case '+':
                        stack.Push(stack.Pop() + stack.Pop());
                        break;
                    case '-':
                        stack.Push(stack.Pop() - stack.Pop());
                        break;
                    case '*':
                        stack.Push(stack.Pop() * stack.Pop());
                        break;
                    case '/':
                        stack.Push(stack.Pop() / stack.Pop());
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            return stack.Pop();
        }

        //Как убрать DRY в этом случае?
        //Это можно сделать, но подробности мы рассмотрим позже.
        static int Compute1(string str)
        {
            var operations = new Dictionary<char, Func<int, int, int>>();
            operations.Add('+', (x, y) => x + y);
            operations.Add('-', (x, y) => x - y);
            operations.Add('*', (x, y) => x * y);
            operations.Add('/', (x, y) => x / y);

            var stack = new Stack<int>();
            foreach (var e in str)
            {
                if (e <= '9' && e >= '0')
                    stack.Push(e - '0');
                else if (operations.ContainsKey(e))
                    stack.Push(operations[e](stack.Pop(), stack.Pop()));
                else
                    throw new ArgumentException();
            }
            return stack.Pop();
        }
    }
}
