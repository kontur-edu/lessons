using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Common.Extensions;

namespace uLearn
{
	public class Course
	{
		public Course(string id, List<Unit> units, CourseSettings settings, DirectoryInfo directory)
		{
			Id = id;
			Units = units;
			Settings = settings;
			Directory = directory;
		}

		public string Id { get; set; }
		public string Title => Settings.Title;
		public CourseSettings Settings { get; private set; }
		public DirectoryInfo Directory { get; private set; }
		public List<Unit> Units { get; private set; }

		private List<Slide> slidesCache { get; set; }

		public List<Slide> Slides
		{
			get { return slidesCache ?? (slidesCache = Units.SelectMany(u => u.Slides).ToList()); }
		}

		[CanBeNull]
		public Slide FindSlideById(Guid slideId)
		{
			return Slides.FirstOrDefault(x => x.Id == slideId);
		}

		[NotNull]
		public Slide GetSlideById(Guid slideId)
		{
			var slide = FindSlideById(slideId);
			if (slide == null)
				throw new NotFoundException($"No slide with id {slideId}");
			return slide;
		}

		[CanBeNull]
		public Unit FindUnitById(Guid unitId)
		{
			return Units.FirstOrDefault(x => x.Id == unitId);
		}

		[NotNull]
		public Unit GetUnitById(Guid unitId)
		{
			var unit = FindUnitById(unitId);
			if (unit == null)
				throw new NotFoundException($"No unit with id {unitId}");
			return unit;
		}

		[CanBeNull]
		public Slide FindSlide(int index)
		{
			return index >= 0 && index < Slides.Count ? Slides[index] : null;
		}

		public override string ToString()
		{
			return $"Course(Id: {Id}, Title: {Title})";
		}
	}

	public class InstructorNote
	{
		public InstructorNote(string markdown, Unit unit, FileInfo file)
		{
			Markdown = markdown;
			Unit = unit;
			File = file;
		}

		public static InstructorNote Load(FileInfo file, Unit unit)
		{
			return new InstructorNote(file.ContentAsUtf8(), unit, file);
		}

		public string Markdown;
		public Unit Unit;
		public FileInfo File;
	}
}