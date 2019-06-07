using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using Ulearn.Common.Api;
using Ulearn.Common.Api.Models.Parameters;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.VideoAnnotations.Api.Models.Parameters.Annotations;
using Ulearn.VideoAnnotations.Api.Models.Responses.Annotations;

namespace Ulearn.VideoAnnotations.Api.Client
{
	public class VideoAnnotationsClient : BaseApiClient, IVideoAnnotationsClient
	{
		private readonly ILogger logger;
		public VideoAnnotationsClient(ILogger logger, Uri endpointUrl)
			:base(logger, new ApiClientSettings
			{
				EndpointUrl = endpointUrl,
				ServiceName = "video annotations service",
			})
		{
			this.logger = logger;
		}

		public async Task<AnnotationsResponse> GetAnnotationsAsync(AnnotationsParameters parameters)
		{
			var sw = Stopwatch.StartNew();
			var response = await MakeRequestAsync<AnnotationsParameters, AnnotationsResponse>(HttpMethod.Get, Urls.Annotations, parameters).ConfigureAwait(false);
			logger.Information("GetAnnotationsAsync " + sw.ElapsedMilliseconds + " ms");
			return response;
		}
		
		public Task ClearAsync()
		{
			return MakeRequestAsync<ApiParameters, ApiResponse>(HttpMethod.Post, Urls.Clear, null);
		}
	}
}