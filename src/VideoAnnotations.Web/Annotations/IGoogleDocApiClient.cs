using System.Threading.Tasks;

namespace Ulearn.VideoAnnotations.Web.Annotations
{
	public interface IGoogleDocApiClient
	{
		Task<string[]> GetGoogleDocContentAsync(string googleDocId);
		string GetGoogleDocLink(string googleDocId);
	}
}