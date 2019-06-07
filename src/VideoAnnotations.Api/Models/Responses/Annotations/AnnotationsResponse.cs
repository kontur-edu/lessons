using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace Ulearn.VideoAnnotations.Api.Models.Responses.Annotations
{
	[DataContract]
	public class AnnotationsResponse : SuccessResponse
	{
		[DataMember(Name = "annotation")]
		public Annotation Annotation { get; set; }
	}

	[DataContract]
	public class Annotation
	{
		[DataMember(Name = "text")]
		public string Text { get; set; }

		[DataMember(Name = "fragments")]
		public List<AnnotationFragment> Fragments { get; set; }
	}

	[DataContract]
	public class AnnotationFragment
	{
		[DataMember(Name = "offset")]
		public TimeSpan Offset { get; set; }
		
		[DataMember(Name = "text")]
		public string Text { get; set; }
	}
}