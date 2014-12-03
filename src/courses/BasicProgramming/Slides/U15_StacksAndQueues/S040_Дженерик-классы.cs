using System;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Дженерик-классы", "d72f4e37-b669-47ef-ab8e-fa8aee40f95a")]
	class S040_Дженерик_классы
	{
		//#video I0ZSWNGuSmE
		/*
		## Заметки по лекции
		*/

        //Дженерик - это класс с типом-параметром.
        public class QueueItem<T> // T - это какой-то тип данных
        {
            //Внутри класса QueueItem, T может использоваться везде,
            //где может использоваться тип данных:
            //при объявлении свойств, в аргументах методов, и т.д.
            public T Value { get; set; }
            public QueueItem<T> Next { get; set; }
        }

        public class Queue<T>
        {
            QueueItem<T> head;
            QueueItem<T> tail;

            public bool IsEmpty { get { return head == null; } }

            public void Enqueue(T value)
            {
                if (IsEmpty)
                    tail = head = new QueueItem<T> { Value = value, Next = null };
                else
                {
                    var item = new QueueItem<T> { Value = value, Next = null };
                    tail.Next = item;
                    tail = item;
                }
            }

            public T Dequeue()
            {
                if (head == null) throw new InvalidOperationException();
                var result = head.Value;
                head = head.Next;
                if (head == null)
                    tail = null;
                return result;
            }
        }

        static void Main()
        {
            var myIntQueue = new Queue<int>(); 
            // здесь мы создаем очередь с уже конкретным T=int.
            // всюду, где в определении класса Queue<T> был написан T,
            // для объекта myIntQueue будет как бы написан int.
            
            
            myIntQueue.Enqueue(10);
            myIntQueue.Enqueue(20);
            myIntQueue.Enqueue(30);

            // myIntQueue.Enqueue("Surprise"); 
            // - здесь будет ошибка компиляции, т.к. метод Enqueue принимает значение T
            // а T для myIntQueue равно int.
        }
    }
}
