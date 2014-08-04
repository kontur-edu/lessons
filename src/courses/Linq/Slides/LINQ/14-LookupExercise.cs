﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace uLearn.Courses.Linq.Slides
{
	[Slide("Создание обратного индекса", "{52CAF978-4BB7-4CC1-92FB-607153DA0A1E}")]
	public class LookupExercise
	{
		/*

		Обратный индекс — это структура данных, часто использующаяся в задачах 
		полнотекстового поиска нужного документа в большой базе документов.

		По своей сути обратный индекс напоминает индекс в конце бумажных энциклопедий, 
		где для каждого ключевого слова указан список страниц, где оно встречается.

		Вам требуется по списку документов построить обратный индекс.

		Документ определен так:
		*/

		public class Document
		{
			public int Id;
			public string Text;
		}

		/*
		Обратный индекс в нашем случае — это словарь `IDictionary<string, HashSet<int>>`, 
		ключем в котором является слово, а значением — хэштаблица, содержащая идентификаторы
		всех документов, содержащих это слово.
		*/


		
		[Hint("Сегодня никаких подсказок!")]
		[Hint("Да, задача сложная, но тем не менее подсказок не будет!")]
		[Hint("Ну правда, пора научиться решать подобные задачи без подсказок!")]
		[Exercise(SingleStatement = true)]
		public static IDictionary<string, List<int>> BuildInvertedIndex(Document[] documents)
		{
			return
				documents
					.SelectMany(doc =>
						Regex.Split(doc.Text, @"\W+")
							.Where(word => word != "")
							.Select(word => Tuple.Create(word.ToLower(), doc))
					)
					.GroupBy(wordDoc => wordDoc.Item1)
					.ToDictionary(
						group => group.Key,
						group => group.Select(wordDoc => wordDoc.Item2.Id).OrderBy(id => id).ToList());
			// ваш код
		}

		[ExpectedOutput(@"
SearchQuery('world') found documents: 1, 2, 2, 2
SearchQuery('words') found documents: 2, 3
SearchQuery('power') found documents: 3
SearchQuery('ktulhu') found documents: 
")]
		public static void Main()
		{
			Document[] documents =
			{
				new Document {Id = 1, Text = "Hello world!"},
				new Document {Id = 2, Text = "World, world, world... Just words..."},
				new Document {Id = 3, Text = "Words — power"},
				new Document {Id = 4, Text = ""},
			};
			var index = BuildInvertedIndex(documents);
			SearchQuery("world", index);
			SearchQuery("words", index);
			SearchQuery("power", index);
			SearchQuery("ktulhu", index);
		}

		[HideOnSlide]
		private static void SearchQuery(string word, IDictionary<string, List<int>> index)
		{
			List<int> ids = index.TryGetValue(word, out ids) ? ids : new List<int>();
			var docIds = string.Join(", ", ids.Select(id => id.ToString()).ToArray());
			Console.WriteLine("SearchQuery('{0}') found documents: {1}", word, docIds);
			
		}
	}
}