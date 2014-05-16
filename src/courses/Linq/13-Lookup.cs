﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SharpLessons.Linqed
{
	[TestFixture]
	public class Lookups
	{
		/*

		##Группировка с помощью ToDictionary и ToLookup

		Нередко встречается необходимость сгруппировав элементы, 
		преобразовать их в структуру данных для поиска группы по ключу группировки.

		Это можно было бы сделать с помощью такой комбинации:

		*/

		[Test]
		[Sample]
		public void Test()
		{
			string[] names = {"Pavel", "Peter", "Andrew", "Anna", "Alice", "John"};

			var namesByLetter = new Dictionary<char, List<string>>();
			foreach (var group in names.GroupBy(name => name[0]))
				namesByLetter.Add(group.Key, group.ToList());

			Assert.That(namesByLetter['J'], Is.EquivalentTo(new[] {"John"}));
			Assert.That(namesByLetter['P'], Is.EquivalentTo(new[] {"Pavel", "Peter"}));
		}

		/*
		Ровно того же эффекта можно добиться и без цикла при помощи Linq-метода `ToDictionary`:

		`IDictionary<K, V> ToDictionary(this IEnumerable<T> items, Func<T, K> keySelector, Func<T, V> valueSelector)`
		
		*/

		[Sample]
		[Test]
		public void TestName()
		{
			string[] names = {"Pavel", "Peter", "Andrew", "Anna", "Alice", "John"};

			Dictionary<char, List<string>> namesByLetter = names
				.GroupBy(name => name[0])
				.ToDictionary(group => group.Key, group => group.ToList());

			Assert.That(namesByLetter['J'], Is.EquivalentTo(new[] {"John"}));
			Assert.That(namesByLetter['P'], Is.EquivalentTo(new[] {"Pavel", "Peter"}));
		}

		/*
		Но ещё проще воспользоваться специальным методом `ToLookup`:

		`ILookup<K, T> ToLookup(this IEnumerable<T> items, Func<T, K> keySelector)`

		*/

		[Sample]
		[Test]
		public void ToLookupSample()
		{
			string[] names = {"Pavel", "Peter", "Andrew", "Anna", "Alice", "John"};
			ILookup<char, string> namesByLetter = names.ToLookup(name => name[0]);

			Assert.That(namesByLetter['J'], Is.EquivalentTo(new[] {"John"}));
			Assert.That(namesByLetter['P'], Is.EquivalentTo(new[] {"Pavel", "Peter"}));
		}
	}
}