using System.Threading.Tasks;
using Ulearn.VideoAnnotations.Api.Models.Parameters.Annotations;
using Ulearn.VideoAnnotations.Api.Models.Responses.Annotations;

namespace Ulearn.VideoAnnotations.Api.Client
{
	public interface IVideoAnnotationsClient
	{
		Task<AnnotationsResponse> GetAnnotationsAsync(AnnotationsParameters parameters);
		Task ClearAsync();
	}
}