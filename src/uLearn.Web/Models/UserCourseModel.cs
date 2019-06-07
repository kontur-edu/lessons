using System;
using System.Collections.Generic;
using System.Linq;
using Database.DataContexts;
using Database.Models;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Units;

namespace uLearn.Web.Models
{
	public class UserCourseModel
	{
		public UserCourseModel(Course course, ApplicationUser user, ULearnDb db)
		{
			Course = course;
			User = user;

			var visits = db.Visits.Where(v => v.UserId == user.Id && v.CourseId == course.Id).GroupBy(v => v.SlideId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
			var unitResults = new Dictionary<Guid, UserCourseUnitModel>();
			foreach (var slide in Course.Slides)
			{
				var unit = slide.Info.Unit;
				if (!unitResults.ContainsKey(unit.Id))
					unitResults.Add(unit.Id, new UserCourseUnitModel
					{
						Unit = unit,
						SlideVisits = new ProgressModel(),
						Exercises = new ProgressModel(),
						Quizes = new ProgressModel(),
						Total = new ProgressModel()
					});

				var res = unitResults[unit.Id];
				var isVisited = visits.ContainsKey(slide.Id);
				var isPassed = isVisited && visits[slide.Id].IsPassed;
				var score = isPassed ? visits[slide.Id].Score : 0;

				res.SlideVisits.Total++;
				res.Total.Total += slide.MaxScore;
				res.Total.Earned += score;

				if (isVisited)
					res.SlideVisits.Earned++;

				if (slide is ExerciseSlide)
				{
					res.Exercises.Total += slide.MaxScore;
					res.Exercises.Earned += score;
				}

				if (slide is QuizSlide)
				{
					res.Quizes.Total += slide.MaxScore;
					res.Quizes.Earned += score;
				}
			}

			Units = course.Units.Select(unit => unitResults[unit.Id]).ToArray();
			Total = new UserCourseUnitModel
			{
				Total = new ProgressModel(),
				Exercises = new ProgressModel(),
				SlideVisits = new ProgressModel(),
				Quizes = new ProgressModel()
			};
			foreach (var result in Units)
			{
				Total.Total.Add(result.Total);
				Total.Exercises.Add(result.Exercises);
				Total.SlideVisits.Add(result.SlideVisits);
				Total.Quizes.Add(result.Quizes);
			}
		}

		public ApplicationUser User { get; set; }
		public Course Course;
		public UserCourseUnitModel[] Units;
		public UserCourseUnitModel Total;
	}


	public class UserCourseUnitModel
	{
		public Unit Unit;
		public ProgressModel Total;
		public ProgressModel Exercises;
		public ProgressModel SlideVisits;
		public ProgressModel Quizes;
	}

	public class ProgressModel
	{
		public int Earned;
		public int Total;

		public decimal? Progress => Total == 0 ? null : (decimal?)Earned / Total;

		public override string ToString()
		{
			if (Progress.HasValue)
				return Progress.Value.ToString("0%");
			return "—";
		}

		public void Add(ProgressModel progress)
		{
			Earned += progress.Earned;
			Total += progress.Total;
		}
	}
}