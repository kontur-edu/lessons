using System;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Очередь на связных списках", "40f14d47-9e63-4672-9287-f92f4c477861")]
	class S020_Очередь_на_связных_списках
	{
		//#video _CpHhXkWFDA
		/*
		## Заметки по лекции
		*/

        public class QueueItem
        {
            public int Value { get; set; }
            public QueueItem Next { get; set; }
        }

        //Эта очередь основана на связных списках,
        //добавляя и удаляя элементы очереди за O(1)
        public class Queue
        {
            QueueItem head;
            QueueItem tail;

            public void Enqueue(int value)
            {
                if (head == null)
                    tail = head = new QueueItem { Value = value, Next = null };
                else
                {
                    var item = new QueueItem { Value = value, Next = null };
                    tail.Next = item;
                    tail = item;
                }
            }

            public int Dequeue()
            {
                if (head == null) throw new InvalidOperationException();
                var result = head.Value;
                head = head.Next;
                if (head == null)
                    tail = null;
                return result;
            }
        }
    }
}
