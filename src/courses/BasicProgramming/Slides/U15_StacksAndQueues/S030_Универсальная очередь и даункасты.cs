using System;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Универсальная очередь и даункасты", "fa834208-e11d-411e-bc5e-f4a0f0d2f248")]
	class S030_Универсальная_очередь_и_даункасты
	{
		//#video GKNk5Li702E
		/*
		## Заметки по лекции
		*/
        /*
          Очередь - достаточно сложная структура данных, и хочется сделать ее 
          сразу для всех типов - очередь чисел, строк, и т.д.,
          а не переписывать для каждого типа данных. 
          Простейшее решение - хранить value в виде object
        */
        public class QueueItem
        {
            public object Value { get; set; }
            public QueueItem Next { get; set; }
        }

        public class Queue
        {
            QueueItem head;
            QueueItem tail;

            public bool IsEmpty { get { return head == null; } }

            public void Enqueue(object value)
            {
                if (IsEmpty)
                    tail = head = new QueueItem { Value = value, Next = null };
                else
                {
                    var item = new QueueItem { Value = value, Next = null };
                    tail.Next = item;
                    tail = item;
                }
            }

            public object Dequeue()
            {
                if (head == null) throw new InvalidOperationException();
                var result = head.Value;
                head = head.Next;
                if (head == null)
                    tail = null;
                return result;
            }
        }

        //Однако, такая очередь имеет проблему даункаста:
        public class Program
        {
            static void Main()
            {
                var myIntQueue = new Queue();
                myIntQueue.Enqueue(10);
                myIntQueue.Enqueue(20);
                myIntQueue.Enqueue(30);

                //Но что, если кто-то напишет так?
                myIntQueue.Enqueue("Surprise!");

                int sum = 0;
                while (!myIntQueue.IsEmpty)
                {
                    int value = (int)myIntQueue.Dequeue(); //здесь будет InvalidCastException
                    sum += value;
                }

            }
        }
    }
}
