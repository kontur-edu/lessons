using System;
using System.Linq;
using System.Text.RegularExpressions;
using uLearn.CSharp;

namespace uLearn.Courses.Linq.Slides.LINQ.LINQ.LINQ
{
	public class S160_LookupExercise : SlideTestBase
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
		Обратный индекс в нашем случае — это словарь `ILookup<string, int>`, 
		ключом в котором является слово, а значениями — идентификаторы
		всех документов, содержащих это слово.
		*/



		public static ILookup<string, int> BuildInvertedIndex(Document[] documents)
		{
			return
				documents
					.SelectMany(doc =>
						Regex.Split(doc.Text, @"\W+")
							.Where(word => word != "")
							.Select(word => Tuple.Create(word.ToLower(), doc.Id))
							.Distinct()
					)
					.ToLookup(
						wd => wd.Item1,
						wd => wd.Item2);
			// ваш код
		}

		public static void Main()
		{
			Document[] documents =
			{
				new Document {Id = 1, Text = "Hello world!"},
				new Document {Id = 2, Text = "World, world, world... Just words..."},
				new Document {Id = 3, Text = "Words — power"},
				new Document {Id = 4, Text = ""}
			};
			var index = BuildInvertedIndex(documents);
			SearchQuery("world", index);
			SearchQuery("words", index);
			SearchQuery("power", index);
			SearchQuery("cthulhu", index);
			SearchQuery("", index);
		}

		private static void SearchQuery(string word, ILookup<string, int> index)
		{

			var ids = index[word].OrderBy(id => id).Select(id => id.ToString());
			var docIds = string.Join(", ", ids.ToArray());
			Console.WriteLine("SearchQuery('{0}') found documents: {1}", word, docIds);

		}
	}
}