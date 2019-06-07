using System;
using System.Linq;
using CommandLine;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-set-start-dates", HelpText = "Set start dates for chapters")]
	public class OlxSetChapterStartDatesOptions : AbstractOptions
	{
		[Option("startDate", Required = true, HelpText = "Date to open the first chapter")]
		public DateTime StartDate { get; set; }

		[Option("prequelChapterIds", Required = true, HelpText = "Ids of prequel chapters, which should be published the same date as next chapter (coma separated)")]
		public string PrequelChapterIds { get; set; }

		public override void DoExecute()
		{
			var prequelChapterIds = PrequelChapterIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			Console.WriteLine("Loading OLX");
			string olxDir = Dir + "/olx";
			var edxCourse = EdxCourse.Load(olxDir);
			Console.WriteLine("Setting dates OLX...");
			var curDate = StartDate;
			foreach (var chapter in edxCourse.CourseWithChapters.Chapters)
			{
				chapter.Start = curDate;
				chapter.Save(olxDir, withAdditionals:false);
				Console.WriteLine($"Patched start date of {chapter.UrlName} to " + curDate);
				if (!prequelChapterIds.Contains(chapter.UrlName, StringComparer.InvariantCultureIgnoreCase))
					curDate += TimeSpan.FromDays(7); // next week
			}

			Console.WriteLine("Done!");
			//EdxInteraction.CreateEdxCourseArchive(Dir, Config.ULearnCourseId);
		}
	}
}