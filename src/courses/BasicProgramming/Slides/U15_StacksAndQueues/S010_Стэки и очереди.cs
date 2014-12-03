using System;
using System.Collections.Generic;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Стэки и очереди", "55477a05-c845-45cc-8beb-a71aef7f1bc4")]
	class S010_Стэки_и_очереди
	{
		//#video lc9gk5AA7Hc
		/*
		## Заметки по лекции
		*/

        // Стэк - это структура данных типа "первым вошел - последним вышел"
        public class ListStack
        {
            List<int> list = new List<int>();
            public void Push(int value)
            {
                list.Add(value);
            }
            public int Pop()
            {
                if (list.Count == 0) throw new InvalidOperationException();
                var result = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return result;
            }
        }

        // Очередь, напротив, это структура данных "первым вошел - первым вышел"
        public class ListQueue
        {
            List<int> list = new List<int>();
            public void Enqueue(int value)
            {
                list.Add(value);
            }

            public int Dequeue()
            {
                if (list.Count == 0) throw new InvalidOperationException();
                var result = list[0];
                list.RemoveAt(0); //в этом месте реализация неэффективна,
                                  //поскольку RemoveAt имеет линейную от размеров листа сложность
                return result;
            }
        }
    }
}
