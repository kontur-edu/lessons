namespace uLearn.CSharp.IndentsValidation.TestData.Correct
{
	public class LocalFunction
	{
		public static Dictionary<string, string> GetMostFrequentNextWords(List<List<string>> text)
		{
			int Compare(KeyValuePair<string, long> a, KeyValuePair<string, long> b)
			{
				return a.Value != b.Value ? a.Value.CompareTo(b.Value) : -string.CompareOrdinal(a.Key, b.Key);
			}

			return GetNGramFrequences(text).ToDictionary(p => p.Key, p => p.Value.MaxItemOrDefault(Compare).Key);
		}
	}
}