using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class LtiConsumer
	{
		[Key]
		public int ConsumerId { get; set; }

		[Required]
		[StringLength(64)]
		public string Name { get; set; }

		[Required]
		[StringLength(64)]
		[Index("Key")]
		public string Key { get; set; }

		[Required]
		[StringLength(64)]
		public string Secret { get; set; }
	}
}