using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool
{
	public static class Converter
	{
		private static Sequential[] UnitToSequentials(Course course, Config config, List<Unit> units, int unitIndex, string ulearnBaseUrl, Dictionary<string, string> videoGuids, DirectoryInfo coursePackageRoot)
		{
			var unit = units[unitIndex];
			var result = new List<Sequential>
			{
				new Sequential($"{course.Id}-{unitIndex}-{0}", unit.Title,
					unit.Slides
						.Where(s => !config.IgnoredUlearnSlides.Select(Guid.Parse).Contains(s.Id))
						.SelectMany(y => y.ToVerticals(course.Id, ulearnBaseUrl, videoGuids, config.LtiId, coursePackageRoot))
						.ToArray())
			};
			var note = unit.InstructorNote;
			if (note != null && config.EmitSequentialsForInstructorNotes)
			{
				var displayName = "Заметки преподавателю";
				var sequentialId = $"{course.Id}-{unitIndex}-note-seq";
				var verticalId = $"{course.Id}-{unitIndex}-note-vert";
				var mdBlockId = $"{course.Id}-{unitIndex}-note-md";
				result.Add(new Sequential(sequentialId, displayName,
						new[]
						{
							new Vertical(
								verticalId,
								displayName,
								new[]
								{
									new MarkdownBlock(unit.InstructorNote.Markdown)
										.ToEdxComponent(mdBlockId, displayName, unit.Directory.FullName)
								})
						}) { VisibleToStaffOnly = true }
				);
			}
			return result.ToArray();
		}

		private static Chapter[] CourseToChapters(Course course, Config config, string ulearnBaseUrl, Dictionary<string, string> videoGuids, DirectoryInfo coursePackageRoot)
		{
			var units = course.Units;
			return Enumerable
				.Range(0, units.Count)
				.Select(idx => new Chapter(
					$"{course.Id}-{idx}",
					units[idx].Title,
					null,
					UnitToSequentials(course, config, units, idx, ulearnBaseUrl, videoGuids, coursePackageRoot)))
				.ToArray();
		}

		public static EdxCourse ToEdxCourse(Course course, Config config, string ulearnBaseUrl,
			Dictionary<string, string> youtubeId2UlearnVideoIds, DirectoryInfo coursePackageRoot)
		{
			return new EdxCourse(
				course.Id,
				config.Organization,
				course.Title,
				new[] { "lti" },
				null,
				CourseToChapters(course, config, ulearnBaseUrl, youtubeId2UlearnVideoIds, coursePackageRoot));
		}
	}
}