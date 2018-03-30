using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class SlideInfo
	{
		[DataMember(Name = "id")]
		public Guid Id { get; set; }
		
		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "slug")]
		public string Slug { get; set; }

		[DataMember(Name = "max_score")]
		public int MaxScore { get; set; }
		
		[DataMember(Name = "type")]
		public SlideType Type { get; set; }

		[DataMember(Name = "api_url")]
		public string ApiUrl { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum SlideType
	{
		Lesson = 1,
		Exercise = 2,
		Quiz = 3,
	}
}