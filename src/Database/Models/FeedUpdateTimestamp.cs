using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class FeedViewTimestamp
	{
		private const string User_Transport = "User_Transport";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[StringLength(64)]
		[Index("User")]
		[Index(User_Transport, 1)]
		public string UserId { get; set; }

		[Index(User_Transport, 2)]
		public int? TransportId { get; set; }

		public virtual NotificationTransport Transport { get; set; }

		[Index("Timestamp")]
		public DateTime Timestamp { get; set; }
	}
}