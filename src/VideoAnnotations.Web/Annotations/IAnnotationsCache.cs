using System.Collections.Generic;
using Ulearn.VideoAnnotations.Api.Models.Responses.Annotations;

namespace Ulearn.VideoAnnotations.Web.Annotations
{
	public interface IAnnotationsCache
	{
		bool TryGet(string key, out Dictionary<string, Annotation> value);
		void Add(string key, Dictionary<string, Annotation> val);
		void Clear();
	}
}