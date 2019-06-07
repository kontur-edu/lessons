using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Ulearn.Core.Courses.Slides.Exercises
{
	[XmlType("scoring")]
	public class ExerciseScoringSettings : ISlideScoringSettings
	{
		[XmlAttribute("group")]
		public string ScoringGroup { get; set; }

		[XmlAttribute("passedTestsScore")]
		public int PassedTestsScore { get; set; } = 5;
		
		/* .NET XML Serializer doesn't understand nullable fields, so we use this hack to make int? field */
		[XmlIgnore]
		public int CodeReviewScore => CodeReviewScoreBack ?? 0;

		[XmlIgnore]
		public bool RequireReview => CodeReviewScoreBack.HasValue;
		
		#region NullableCodeReviewScoreHack
		
		private int? CodeReviewScoreBack { get; set; }
		
		[XmlAttribute("codeReviewScore")]
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public int CodeReviewScoreSerialized
		{
			get
			{
				Debug.Assert(CodeReviewScoreBack != null, nameof(Language) + " != null");
				return CodeReviewScoreBack.Value;
			}
			set => CodeReviewScoreBack = value;
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCodeReviewScoreSerialized()
		{
			return CodeReviewScoreBack.HasValue;
		}
		
		#endregion		
	}
}