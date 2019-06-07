using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Ulearn.Common;

namespace Stepik.Api
{
	public class YoutubeVideoUrlExtractor
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(YoutubeVideoUrlExtractor));
		private readonly HttpClient httpClient;
		private const string youtubeBaseUrl = "https://youtube.com";

		public YoutubeVideoUrlExtractor()
		{
			var userAgent = new ProductInfoHeaderValue("Mozilla", "5.0");

			httpClient = new HttpClient
			{
				BaseAddress = new Uri(youtubeBaseUrl),
				DefaultRequestHeaders =
				{
					Referrer = new Uri(youtubeBaseUrl),
					UserAgent = { userAgent }
				}
			};
		}

		/* Available options for `quality` are "hd720", "medium" and "small"
		 * Available options for `videoType` are "video/mp4", "video/webm" and "video/gpp"
		 * TODO (andgein): make enums for quaility and videoType
		 */
		public Task<string> GetVideoUrl(string youtubeUrl, string quality="hd720", string videoType="video/mp4")
		{
			return FuncUtils.TrySeveralTimesAsync(
				() => TryGetVideoUrl(youtubeUrl, quality, videoType),
				5,
				() => Task.Delay(TimeSpan.FromSeconds(1))
			);
		}

		private async Task<string> TryGetVideoUrl(string youtubeUrl, string quality, string videoType)
		{
			var fmtStreams = await GetFmtStreams(youtubeUrl);
			foreach (var fmtStream in fmtStreams)
			{
				var decodedFmtStream = WebUtility.UrlDecode(fmtStream);
				if (string.IsNullOrEmpty(decodedFmtStream))
					continue;

				if (!decodedFmtStream.Contains($"type={videoType}") || !decodedFmtStream.Contains($"quality={quality}"))
					continue;

				var splitted = decodedFmtStream.Split(new[] { @"\u0026" }, StringSplitOptions.None);
				foreach (var part in splitted)
					if (part.StartsWith("url="))
						return part.Substring("url=".Length);
			}

			throw new YoutubeVideoUrlExtractException($"Can't find url for this type ({videoType}) and quality ({quality}) on page");
		}

		private async Task<string[]> GetFmtStreams(string youtubeUrl)
		{
			log.Info($"Making request to {youtubeUrl}");
			var youtubePageResponse = await httpClient.GetAsync(youtubeUrl);
			var youtubePageContent = await youtubePageResponse.Content.ReadAsStringAsync();

			if (! youtubePageResponse.IsSuccessStatusCode)
				throw new YoutubeVideoUrlExtractException($"Can't download youtube's page. HTTP status code is {(int) youtubePageResponse.StatusCode} {youtubePageResponse.StatusCode}. HTTP content: {youtubePageContent}");

			var regex = new Regex("url_encoded_fmt_stream_map\":\\s*\"(.*)\"", RegexOptions.IgnoreCase);
			var matches = regex.Matches(youtubePageContent);
			if (matches.Count == 0)
				throw new YoutubeVideoUrlExtractException("Can't find url_encoded_fmt_stream_map in youtube's page content");

			var match = matches[0];
			var group = match.Groups[1].Value;
			return group.Split(',');
		}
	}

	public class YoutubeVideoUrlExtractException : Exception
	{
		public YoutubeVideoUrlExtractException()
		{
		}

		public YoutubeVideoUrlExtractException(string message)
			: base(message)
		{
		}

		public YoutubeVideoUrlExtractException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected YoutubeVideoUrlExtractException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
