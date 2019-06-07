using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Ulearn.Core.Extensions
{
	public static class NameValueCollectionExtensions
	{
		public static List<string> GetMultipleValues(this NameValueCollection collection, string key, bool splitCommaSeparated = true)
		{
			var values = collection.GetValues(key);
			if (values == null)
				return new List<string>();

			var valuesList = new List<string>(values);
			if (splitCommaSeparated)
				valuesList = valuesList.SelectMany(s => s.Split(',')).ToList();

			return valuesList;
		}
	}
}