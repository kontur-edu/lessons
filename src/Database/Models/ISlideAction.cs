using System;

namespace Database.Models
{
	public interface ISlideAction
	{
		int Id { get; }
		string CourseId { get; }
		Guid SlideId { get; }
		string UserId { get; }
	}

	public interface ITimedSlideAction : ISlideAction
	{
		DateTime Timestamp { get; }
	}
}