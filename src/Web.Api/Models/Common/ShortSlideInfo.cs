using System;
using System.Runtime.Serialization;
using Ulearn.Core.Courses.Slides;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class ShortSlideInfo
	{
		[DataMember]
		public Guid Id { get; set; }
		
		[DataMember]
		public string Title { get; set; }

		/// <summary>
		/// Человекочитаемый фрагмент url для слайда
		/// </summary>
		[DataMember]
		public string Slug { get; set; }

		[DataMember]
		public int MaxScore { get; set; }
		
		[DataMember]
		public SlideType Type { get; set; }

		[DataMember]
		public string ApiUrl { get; set; }
	}
}