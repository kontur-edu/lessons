using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Results.ExerciseStatistics
{
	[DataContract]
	public class CourseExercisesStatisticsResult
	{
		[DataMember(Name = "exercises")]
		public Dictionary<Guid, OneExerciseStatistics> Exercises { get; set; }
	}

	[DataContract]
	public class OneExerciseStatistics
	{
		[DataMember(Name = "exercise")]
		public SlideInfo Exercise { get; set; }

		[DataMember(Name = "submissions_count")]
		public int SubmissionsCount { get; set; }
		
		[DataMember(Name = "accepted_count")]
		public int AcceptedCount { get; set; }
		
		[DataMember(Name = "last_dates")]
		public Dictionary<DateTime, OneExerciseStatisticsForDate> LastDates { get; set; }
	}

	[DataContract]
	public class OneExerciseStatisticsForDate
	{
		[DataMember(Name = "submissions_count")]
		public int SubmissionsCount { get; set; }
		
		[DataMember(Name = "accepted_count")]
		public int AcceptedCount { get; set; }
	}
}