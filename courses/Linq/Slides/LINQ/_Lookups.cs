using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace uLearn.Courses.Linq.Slides.LINQ
{
	[TestFixture]
	public class S150_Lookups
	{
		/*

		Нередко встречается необходимость, сгруппировав элементы, 
		преобразовать их в структуру данных для поиска группы по ключу группировки.

		Это можно было бы сделать с помощью такой комбинации:

		*/

		[Test]
		public void Test()
		{
			string[] names = {"Pavel", "Peter", "Andrew", "Anna", "Alice", "John"};

			var namesByLetter = new Dictionary<char, List<string>>();
			foreach (var group in names.GroupBy(name => name[0]))
				namesByLetter.Add(group.Key, group.ToList());

			Assert.That(namesByLetter['J'], Is.EquivalentTo(new[] { "John" }));
			Assert.That(namesByLetter['P'], Is.EquivalentTo(new[] {"Pavel", "Peter"}));
			Assert.IsFalse(namesByLetter.ContainsKey('Z'));
		}

		/*
		Ровно того же эффекта можно добиться и без цикла при помощи `LINQ`-метода `ToDictionary`:

		`IDictionary<K, V> ToDictionary(this IEnumerable<T> items, Func<T, K> keySelector, Func<T, V> valueSelector)`
		
		*/

		[Test]
		public void TestName()
		{
			string[] names = {"Pavel", "Peter", "Andrew", "Anna", "Alice", "John"};

			Dictionary<char, List<string>> namesByLetter = names
				.GroupBy(name => name[0])
				.ToDictionary(group => group.Key, group => group.ToList());

			Assert.That(namesByLetter['J'], Is.EquivalentTo(new[] { "John" }));
			Assert.That(namesByLetter['P'], Is.EquivalentTo(new[] {"Pavel", "Peter"}));
			Assert.IsFalse(namesByLetter.ContainsKey('Z'));
		}

		/*
		Но еще проще воспользоваться специальным методом `ToLookup`:

		* `ILookup<K, T> ToLookup(this IEnumerable<T> items, Func<T, K> keySelector)`
		* `ILookup<K, V> ToLookup(this IEnumerable<T> items, Func<T, K> keySelector, Func<T, V> valueSelector)`

		*/

		[Test]
		public void ToLookupSample()
		{
			string[] names = {"Pavel", "Peter", "Andrew", "Anna", "Alice", "John"};
			ILookup<char, string> namesByLetter = names.ToLookup(name => name[0], name => name.ToLower());

			Assert.That(namesByLetter['J'], Is.EquivalentTo(new[] {"john"}));
			Assert.That(namesByLetter['P'], Is.EquivalentTo(new[] {"pavel", "peter"}));
			
			// Lookup по неизвестному ключу возвращает пустую коллекцию. 
			//Часто это удобнее, чем поведение Dictionary, который в такой ситуации бросает исключение.
			Assert.That(namesByLetter['Z'], Is.Empty);
		}
	}
}