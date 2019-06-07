using System.IO;

namespace Ulearn.Core.Courses
{
	public interface ICourseLoader
	{
		Course Load(DirectoryInfo dir);
	}
}