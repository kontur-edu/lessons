using System.ComponentModel.DataAnnotations;

namespace uLearn.Web.Models
{
	public class TextBlob
	{
		[Key]
		[Required]
		[StringLength(40)]
		public string Hash { get; set; }

		[StringLength(4096)]
		[Required]
		public string Text { get; set; }
	}
}