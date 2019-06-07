using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace uLearn.Courses.Linq.Slides.LINQ
{
	[TestFixture]
	public class S040_SelectManySample
	{
		/*

		Этот метод несколько менее очевиден, чем предыдущие, однако он довольно часто пригождается в самых разных задачах.

		`IEnumerable<R> SelectMany(this IEnumerable<T> items, Func<T, IEnumerable<R>> f)`

		В качестве аргумента он принимает функцию, преобразующую каждый элемент исходной последовательности
		в новую последовательность. А результатом работы является конкатенация всех полученных последовательностей.

		Следующий пример пояснит работу этого метода:
		*/

		[Test]
		public void SelectManyDemo()
		{
			string[] words = {"ab", "", "c", "de"};
			IEnumerable<char> letters = words.SelectMany(w => w.ToCharArray());
			Assert.That(letters, Is.EqualTo(new[] {'a', 'b', 'c', 'd', 'e'}));
		}

		/* 
		Впрочем строка уже сама по себе является последовательностью символов и реализует интерфейс `IEnumerable<char>`,
		поэтому вызов `ToCharArray` на самом деле лишний.
		*/

		[Test]
		public void SelectManyDemo2()
		{
			string[] words = {"ab", "", "c", "de"};
			var letters = words.SelectMany(w => w); // <= исчез вызов ToCharArray
			Assert.That(letters, Is.EqualTo(new[] {'a', 'b', 'c', 'd', 'e'}));
		}
	}
}