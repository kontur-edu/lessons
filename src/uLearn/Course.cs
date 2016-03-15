using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace uLearn
{
	public class Course
	{
		public Course(string id, string title, Slide[] slides, InstructorNote[] instructorNotes, CourseSettings settings, DirectoryInfo directory)
		{
			Id = id;
			Title = title;
			Slides = slides;
			InstructorNotes = instructorNotes;
			Settings = settings;
			Directory = directory;
		}

		public string Id { get; private set; }
		public string Title { get; private set; }
		public Slide[] Slides { get; private set; }
		public InstructorNote[] InstructorNotes { get; private set; }
		public CourseSettings Settings { get; private set; }
		public DirectoryInfo Directory { get; private set; }

		public string GetDirectoryByUnitName(string unitName)
		{
			return Slides.First(x => x.Info.UnitName == unitName).Info.SlideFile.Directory.FullName;
		}

		public Slide GetSlideById(string slideId)
		{
			return Slides.FirstOrDefault(x => x.Id == slideId);
		}
		
		public Slide FindSlide(int index)
		{
			return index >= 0 && index < Slides.Length ? Slides[index] : null;
		}

		public int GetSlideIndexById(string slideId)
		{
			return Slides.FindIndex(x => x.Id == slideId);
		}

		public IEnumerable<string> GetUnits()
		{
			return Slides.Select(s => s.Info.UnitName).Distinct();
		}

		public InstructorNote FindInstructorNote(string unitName)
		{
			return InstructorNotes.SingleOrDefault(n => n.UnitName == unitName);
		}

		public override string ToString()
		{
			return string.Format("Id: {0}, Title: {1}", Id, Title);
		}
	}

	public class InstructorNote
	{
		public InstructorNote(string markdown, string courseId, string unitName, FileInfo file)
		{
			Markdown = markdown;
			CourseId = courseId;
			UnitName = unitName;
			File = file;
		}

		public string Markdown;
		public string CourseId;
		public string UnitName;
		public FileInfo File;
	}
}