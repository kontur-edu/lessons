using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Ulearn.Core.Courses.Slides.Quizzes
{
	public class RegexInfo
	{
		[XmlText]
		public string Pattern;

		[XmlAttribute("ignoreCase")]
		public bool IgnoreCase;

		private Regex regex;

		[XmlIgnore]
		public Regex Regex => regex ?? (regex = new Regex("^" + Pattern + "$", IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));

		public override bool Equals(object obj)
		{
			if (!(obj is RegexInfo))
				return false;
			if (obj == this)
				return true;

			return Equals((RegexInfo)obj);
		}

		protected bool Equals(RegexInfo other)
		{
			return string.Equals(Pattern, other.Pattern) && IgnoreCase == other.IgnoreCase;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Pattern != null ? Pattern.GetHashCode() : 0) * 397) ^ IgnoreCase.GetHashCode();
			}
		}
	}
}