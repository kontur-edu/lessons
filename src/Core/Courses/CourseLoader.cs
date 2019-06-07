using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Remotion.Linq.Clauses;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Extensions;

namespace Ulearn.Core.Courses
{
	public class CourseLoader : ICourseLoader
	{
		private readonly IUnitLoader unitLoader;

		public CourseLoader(IUnitLoader unitLoader)
		{
			this.unitLoader = unitLoader;
		}

		/* For backward compatibility. In future we should use only DI */
		public CourseLoader()
			: this(new UnitLoader(new XmlSlideLoader()))
		{
		}
		
		public Course Load(DirectoryInfo dir)
		{
			try
			{
				return UnsafeLoad(dir);
			}
			catch (Exception e) when (!e.GetType().IsAssignableFrom(typeof(CourseLoadingException)))
			{
				throw new CourseLoadingException("Не удалось загрузить курс. ", e);
			}
		}

		private Course UnsafeLoad(DirectoryInfo dir)
		{
			var courseId = dir.Name;

			var loadFromDirectory = dir;
			var courseXmls = dir.GetFiles("course.xml", SearchOption.AllDirectories).ToList();
			if (courseXmls.Count == 1)
				loadFromDirectory = courseXmls[0].Directory;
			else
				loadFromDirectory = loadFromDirectory.HasSubdirectory("Slides") ? loadFromDirectory.GetSubdirectory("Slides") : loadFromDirectory;

			CourseSettings settings;
			try
			{
				settings = CourseSettings.Load(loadFromDirectory);
			}
			catch (Exception e)
			{
				throw new CourseLoadingException($"Не удалось прочитать настройки курса из файла course.xml. {e.Message}", e.InnerException);
			}

			if (string.IsNullOrEmpty(settings.Title))
			{
				try
				{
					settings.Title = GetCourseTitleFromFile(loadFromDirectory);
				}
				catch (Exception e)
				{
					throw new CourseLoadingException(
						"Не удалось прочитать настройки курса. Скорее всего, отсутствует или неправильно заполнен файл course.xml."
					);
				}
			}

			var context = new CourseLoadingContext(courseId, settings, dir, loadFromDirectory.GetFile("course.xml"));

			var units = LoadUnits(context).ToList();
			var slides = units.SelectMany(u => u.Slides).ToList();
			CheckDuplicateSlideIds(slides);
			AddDefaultScoringGroupIfNeeded(units, slides, settings);
			CalculateScoringGroupScores(units, settings);

			return new Course(courseId, units, settings, context.CourseXml.Directory);
		}

		private static void CalculateScoringGroupScores(IEnumerable<Unit> units, CourseSettings settings)
		{
			foreach (var unit in units)
			{
				foreach (var slide in unit.Slides.Where(s => s.ShouldBeSolved))
				{
					unit.Scoring.Groups[slide.ScoringGroup].MaxNotAdditionalScore += slide.MaxScore;
					settings.Scoring.Groups[slide.ScoringGroup].MaxNotAdditionalScore += slide.MaxScore;
				}
			}
		}

		private static void AddDefaultScoringGroupIfNeeded(IEnumerable<Unit> units, IEnumerable<Slide> slides, CourseSettings settings)
		{
			if (slides.Any(s => s.ShouldBeSolved && string.IsNullOrEmpty(s.ScoringGroup)))
			{
				var defaultScoringGroup = new ScoringGroup
				{
					Id = "",
					Abbreviation = "Баллы",
					Name = "Упражнения и тесты",
				};
				settings.Scoring.Groups.Add(defaultScoringGroup.Id, defaultScoringGroup);

				/* Add default scoring group to each unit */
				foreach (var unit in units)
					unit.Scoring.Groups.Add(defaultScoringGroup.Id, defaultScoringGroup);
			}
		}

		private IEnumerable<Unit> LoadUnits(CourseLoadingContext context)
		{
			var unitFiles = context.CourseSettings
				.UnitPaths
				.SelectMany(path => context.CourseXml.Directory.GetFilesByMask(path).OrderBy(f => f.FullName, StringComparer.InvariantCultureIgnoreCase))
				.Distinct()
				/* Don't load unit from course file! Even accidentally */
				.Where(f => f != context.CourseXml);

			var unitIds = new HashSet<Guid>();
			var unitUrls = new HashSet<string>();
			var slideIndex = 0;
			foreach (var unitFile in unitFiles)
			{
				var unit = unitLoader.Load(unitFile, context, slideIndex);

				if (unitIds.Contains(unit.Id))
					throw new CourseLoadingException($"Ошибка в курсе \"{context.CourseSettings.Title}\" при загрузке модуля \"{unit.Title}\" из {unitFile.FullName}. " +
													 $"Повторяющийся идентификатор модуля: {unit.Id}. Идентификаторы модулей должны быть уникальными. " +
													 $"К этому времени загружены модули {string.Join(", ", unitIds)}");
				unitIds.Add(unit.Id);

				if (unitUrls.Contains(unit.Url))
					throw new CourseLoadingException(
						$"Ошибка в курсе \"{context.CourseSettings.Title}\" при загрузке модуля \"{unit.Title}\" из {unitFile.FullName}. " +
						$"Повторяющийся url-адрес модуля: {unit.Url}. Url-адреса модулей должны быть уникальными"
					);
				unitUrls.Add(unit.Url);

				yield return unit;

				slideIndex += unit.Slides.Count;
			}
		}

		private static string GetCourseTitleFromFile(DirectoryInfo dir)
		{
			return dir.GetFile("Title.txt").ContentAsUtf8();
		}

		private static void CheckDuplicateSlideIds(IEnumerable<Slide> slides)
		{
			var badSlides =
				slides.GroupBy(x => x.Id)
					.Where(x => x.Count() != 1)
					.Select(x => x.Select(y => y.Title))
					.ToList();
			if (badSlides.Any())
				throw new CourseLoadingException(
					"Идентификаторы слайдов (SlideId) должны быть уникальными.\n" +
					"Слайды с повторяющимися идентификаторами:\n" +
					string.Join("\n", badSlides.Select(x => string.Join("\n", x))));
		}
	}

	public class CourseLoadingException : Exception
	{
		public CourseLoadingException(string message)
			: base(message)
		{
		}

		public CourseLoadingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}