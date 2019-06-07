using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Database.Models.Comments
{
	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum CommentModerationPolicy
	{
		Premoderation,
		Postmoderation
	}

	public class CommentsPolicy
	{
		[Key]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		public bool IsCommentsEnabled { get; set; }

		[Required]
		public CommentModerationPolicy ModerationPolicy { get; set; }

		[Required]
		public bool OnlyInstructorsCanReply { get; set; }

		public const int MaxCommentLength = 10000;

		[NotMapped]
		public readonly int MaxCommentsCountInLastTime = 10;

		[NotMapped]
		public readonly TimeSpan LastTimeForMaxCommentsLimit = TimeSpan.FromMinutes(5);
	}
}