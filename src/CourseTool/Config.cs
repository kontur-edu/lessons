using System.Linq;
using System.Xml.Serialization;

namespace uLearn.CourseTool
{
	public class Config
	{
		public string Organization;
		public string LtiId;
		public string ULearnCourseId;
		public string ULearnCoursePackageRoot;
		public string Video;
		public Profile[] Profiles;

		[XmlIgnore]
		private string[] ignoredUlearnSlides = new string[0];

		[XmlArrayItem("SlideId")]
		public string[] IgnoredUlearnSlides
		{
			get => ignoredUlearnSlides;
			set => ignoredUlearnSlides = value.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
		}

		public bool EmitSequentialsForInstructorNotes { get; set; }

		public Profile GetProfile(string profile)
		{
			return Profiles.SingleVerbose(x => x.Name == profile, "profile name = " + profile);
		}
	}

	public class Profile
	{
		[XmlAttribute("name")]
		public string Name;
		public string UlearnUrl;
	}
}