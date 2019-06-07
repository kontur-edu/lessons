using System.Linq;
using NUnit.Framework;

namespace uLearn.Courses.Linq.Slides.LINQ
{
	public class S013_MethodChaining
	{

		/*
		Несколько последовательных действий с перечисляемым можно объединять в одну цепочку вызовов.
		Такой прием называется [Method Chaining](http://en.wikipedia.org/wiki/Method_chaining).
		Однако для улучшения читаемости вашего кода настоятельно рекомендуется каждый вызов метода помещать в отдельную строку, вот так:
		*/

		[Test]
		public void MethodChainingSample()
		{
			Assert.That(
				new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
					.Where(x => x % 2 == 0)
					.Select(x => x * x)
					.Skip(1)
					.Take(2)
					.ToArray(),
				Is.EqualTo(new[] { 16, 36 }));
		}

		/*
		Method chaining делает код компактнее, но скрывает информацию о типах и семантике промежуточных значений. 
		Иногда всё же стоит оставлять вспомогательные переменные, чтобы сделать код более читаемым.
		
		В коде ниже имя переменной `girls` помогает понять неочевидную задумку автора, который (наивно) считает, 
		что все женские имена заканчиваются на 'a':
		*/

		[Test]
		public void MoreReadable()
		{
			var people = new[]{"Pavel Egorov", "Yuriy Okulovskiy", 
				"Alexandr Denisov", "Ivan Sorokin", 
				"Dasha Zubova", "Irina Gess"};
			
			var names = people.Select(fullname => fullname.Split(' ')[0]);
			
			var girls = names.Where(name => name[name.Length - 1] == 'a');

			Assert.That(girls, Is.EqualTo(new[] {"Dasha", "Irina"}));
		}
	}
}