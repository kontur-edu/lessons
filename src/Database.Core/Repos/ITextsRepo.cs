using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface ITextsRepo
	{
		Task<TextBlob> AddText(string text);
		TextBlob GetText(string hash);
		Dictionary<string, string> GetTextsByHashes(IEnumerable<string> hashes);
	}
}