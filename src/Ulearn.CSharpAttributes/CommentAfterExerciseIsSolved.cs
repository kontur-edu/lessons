using System;

namespace uLearn
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class CommentAfterExerciseIsSolved : Attribute
	{
		public CommentAfterExerciseIsSolved(string comment)
		{
			Comment = comment;
		}

		public string Comment { get; set; }
	}
}