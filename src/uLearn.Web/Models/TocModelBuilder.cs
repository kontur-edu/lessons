using System;
using System.Collections.Generic;
using System.Linq;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Units;

namespace uLearn.Web.Models
{
	public class TocModelBuilder
	{
		private readonly Func<Slide, string> getSlideUrl;
		private readonly Func<Slide, int> getSlideScore;
		private readonly Func<Unit, ScoringGroup, int> getAdditionalScore;
		private readonly Func<Slide, int> getSlideMaxScore;
		private readonly Course course;
		private readonly Guid? currentSlideId;
		public Func<Unit, string> GetUnitStatisticsUrl;
		public Func<Unit, string> GetUnitInstructionNotesUrl;
		public Func<Slide, bool> IsSolved = s => false;
		public Func<Slide, bool> IsVisited = s => false;
		public Func<Unit, bool> IsUnitVisible = u => true;
		public Func<Slide, bool> IsSlideHidden = s => true;
		public List<string> EnabledScoringGroupsIds { get; set; }
		public bool IsInstructor;

		public TocModelBuilder(Func<Slide, string> getSlideUrl, Func<Slide, int> getSlideScore, Func<Slide, int> getSlideMaxScore, Func<Unit, ScoringGroup, int> getAdditionalScore, Course course, Guid? currentSlideId)
		{
			this.getSlideUrl = getSlideUrl;
			this.getSlideScore = getSlideScore;
			this.getSlideMaxScore = getSlideMaxScore;
			this.getAdditionalScore = getAdditionalScore;
			this.course = course;
			this.currentSlideId = currentSlideId;
			EnabledScoringGroupsIds = new List<string>();
		}

		public TocModel CreateTocModel(List<TocGroupForStatistics> groupsForStatistics)
		{
			return new TocModel(course, CreateUnits(), groupsForStatistics);
		}

		public TocModel CreateTocModel()
		{
			return new TocModel(course, CreateUnits(), new List<TocGroupForStatistics>());
		}

		private TocUnitModel[] CreateUnits()
		{
			return course.Units
				.Where(u => IsUnitVisible(u) && u.Slides.Any(s => !IsSlideHidden(s)))
				.Select(CreateUnit)
				.ToArray();
		}

		private TocUnitModel CreateUnit(Unit unit)
		{
			var pages = unit.Slides.Where(s => !IsSlideHidden(s)).Select(CreatePage).ToList();
			if (IsInstructor)
			{
				if (unit.InstructorNote != null)
					pages.Add(new TocPageInfo
					{
						Url = GetUnitInstructionNotesUrl(unit),
						Name = "Заметки преподавателю",
						PageType = TocPageType.InstructorNotes,
					});
				pages.Add(new TocPageInfo
				{
					Url = GetUnitStatisticsUrl(unit),
					Name = "Ведомость модуля",
					PageType = TocPageType.UnitStatistics,
				});
			}

			var additionalScores = unit.Scoring.Groups.Values
				.Where(g => g.CanBeSetByInstructor && (g.EnabledForEveryone || EnabledScoringGroupsIds.Contains(g.Id)))
				.ToDictionary(g => g, g => getAdditionalScore(unit, g));
			return new TocUnitModel
			{
				IsCurrent = unit.Slides.Any(s => s.Id == currentSlideId),
				UnitName = unit.Title,
				Pages = pages,
				AdditionalScores = additionalScores,
			};
		}

		private TocPageInfo CreatePage(Slide slide)
		{
			var page = new TocPageInfo
			{
				SlideId = slide.Id,
				Url = getSlideUrl(slide),
				Name = slide.Title,
				ShouldBeSolved = slide.ShouldBeSolved,
				MaxScore = getSlideMaxScore(slide),
				Score = getSlideScore(slide),
				IsCurrent = slide.Id == currentSlideId,
				IsSolved = IsSolved(slide),
				IsVisited = IsVisited(slide),
				PageType = GetPageType(slide)
			};
			return page;
		}

		private TocPageType GetPageType(Slide slide)
		{
			if (slide is QuizSlide)
				return TocPageType.Quiz;
			if (slide is ExerciseSlide)
				return TocPageType.Exercise;
			return TocPageType.Theory;
		}
	}
}