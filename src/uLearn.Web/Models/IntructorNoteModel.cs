using Ulearn.Core.Courses;

namespace uLearn.Web.Models
{
	public class IntructorNoteModel
	{
		public IntructorNoteModel(string courseId, InstructorNote note, string noteEditUrl)
		{
			CourseId = courseId;
			Note = note;
			NoteEditUrl = noteEditUrl;
		}

		public string CourseId;
		public string NoteEditUrl;
		public InstructorNote Note;
	}
}