using System;
using System.Collections.Generic;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;

namespace uLearn.Web.Models
{
	public class BlockRenderContext
	{
		public Course Course { get; private set; }
		public Slide Slide { get; private set; }
		public string BaseUrl { get; private set; }
		public dynamic[] BlockData { get; private set; }
		public bool IsGuest { get; set; }
		public bool IsLti { get; set; }
		public AbstractManualSlideChecking ManualChecking { get; set; }
		public int ManualCheckingsLeft { get; set; }
		public bool CanUserFillQuiz { get; set; }
		public bool RevealHidden { get; private set; }
		public bool Autoplay { get; private set; }
		public bool IsManualCheckingReadonly { get; private set; }
		public bool DefaultProhibitFurtherReview { get; set; }
		public Dictionary<string, int> UserScores { get; }

		/* GroupsIds != null if instructor filtered users by group and see their submissions */
		public List<string> GroupsIds { get; set; }

		/* User's version of slide, i.e. for exercises */
		public int? VersionId { get; set; }
		
		public BlockRenderContext(Course course, Slide slide, string baseUrl, dynamic[] blockData,
			bool isGuest = false, bool revealHidden = false, AbstractManualSlideChecking manualChecking = null,
			int manualCheckingsLeft = 0, bool canUserFillQuiz = false, List<string> groupsIds = null, bool isLti = false, bool autoplay = false,
			bool isManualCheckingReadonly = false, bool defaultProhibitFurtherReview = true, Dictionary<string, int> userScores = null)
		{
			if (blockData.Length != slide.Blocks.Length)
				throw new ArgumentException("BlockRenderContext(): BlockData.Length should be slide.Blocks.Length");
			Course = course;
			Slide = slide;
			BaseUrl = baseUrl;
			BlockData = blockData;
			IsGuest = isGuest;
			RevealHidden = revealHidden;
			ManualChecking = manualChecking;
			ManualCheckingsLeft = manualCheckingsLeft;
			CanUserFillQuiz = canUserFillQuiz;
			GroupsIds = groupsIds;
			IsLti = isLti;
			Autoplay = autoplay;
			IsManualCheckingReadonly = isManualCheckingReadonly;
			DefaultProhibitFurtherReview = defaultProhibitFurtherReview;
			UserScores = userScores ?? new Dictionary<string, int>();
		}
		
		[NotNull]
		public dynamic GetBlockData(SlideBlock block)
		{
			var index = Array.IndexOf(Slide.Blocks, block);
			if (index < 0)
				throw new ArgumentException("No block " + block + " in slide " + Slide);
			var data = BlockData[index];


			return data ?? GetDefaultBlockData(block);
		}

		private dynamic GetDefaultBlockData(SlideBlock block)
		{
			if (Slide is QuizSlide)
				return new QuizBlockData(new QuizModel(), 1, new QuizState(QuizStatus.ReadyToSend, 0, 0, Slide.MaxScore));
			if (Slide is ExerciseSlide)
				return new ExerciseBlockData(Course.Id, Slide as ExerciseSlide) { IsGuest = IsGuest, IsLti = IsLti };
			
			throw new ArgumentException($"Internal error. Unknown slide type: {Slide.GetType()}. Should be {nameof(QuizSlide)} or {nameof(ExerciseSlide)}.");
		}
	}
}