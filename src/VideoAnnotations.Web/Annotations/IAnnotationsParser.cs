using System.Collections.Generic;
using Ulearn.VideoAnnotations.Api.Models.Responses.Annotations;

namespace Ulearn.VideoAnnotations.Web.Annotations
{
	public interface IAnnotationsParser
	{
		Dictionary<string, Annotation> ParseAnnotations(string[] lines);
	}
}