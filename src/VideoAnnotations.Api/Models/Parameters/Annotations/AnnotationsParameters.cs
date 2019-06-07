using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Parameters;
using Ulearn.Common.Api.Models.Validations;

namespace Ulearn.VideoAnnotations.Api.Models.Parameters.Annotations
{
	public class AnnotationsParameters : ApiParameters
	{
		[FromQuery(Name = "google_doc_id")]
		[Required]
		[NotEmpty]
		public string GoogleDocId { get; set; }
		
		[FromQuery(Name = "video_id")]
		[Required]
		[NotEmpty]
		public string VideoId { get; set; }
	}
}