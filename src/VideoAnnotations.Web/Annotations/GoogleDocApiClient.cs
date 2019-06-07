using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ulearn.VideoAnnotations.Web.Configuration;

namespace Ulearn.VideoAnnotations.Web.Annotations
{
	public class GoogleDocApiClient : IGoogleDocApiClient
	{
		private VideoAnnotationsServiceConfiguration Configuration { get; set; }
		
		public GoogleDocApiClient(IOptions<VideoAnnotationsConfiguration> options)
		{
			Configuration = options.Value.VideoAnnotations;
		}

		public async Task<string[]> GetGoogleDocContentAsync(string googleDocId)
		{
			using (var client = new HttpClient { Timeout = Configuration.Timeout })
			{
				var url = $@"https://www.googleapis.com/drive/v3/files/{googleDocId}/export?mimeType=text/plain&key={Configuration.GoogleDocsApiKey}";
				var result = await client.GetStringAsync(url).ConfigureAwait(false);
				return result.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			}
		}

		public string GetGoogleDocLink(string googleDocId)
		{
			return $"https://docs.google.com/document/d/{googleDocId}/edit";
		}
	}
}