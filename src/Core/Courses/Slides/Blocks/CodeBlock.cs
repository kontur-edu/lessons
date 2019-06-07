using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;
using Ulearn.Core.Model.Edx.EdxComponents;
using Component = Ulearn.Core.Model.Edx.EdxComponents.Component;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	[XmlType("code")]
	public class CodeBlock : SlideBlock
	{
		private string code;

		[XmlText]
		public string Code
		{
			get => code;
			set => code = value.RemoveCommonNesting().TrimEnd();
		}
		
		/* .NET XML Serializer doesn't understand nullable fields, so we use this hack to make Language? field */
		[XmlIgnore]
		public Language? Language { get; set; }

		#region NullableLanguageHack
		
		[XmlAttribute("language")]
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public Language LanguageSerialized
		{
			get
			{
				Debug.Assert(Language != null, nameof(Language) + " != null");
				return Language.Value;
			}
			set => Language = value;
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLanguageSerialized()
		{
			return Language.HasValue;
		}
		#endregion
		
		[XmlIgnore]
		public List<Label> SourceCodeLabels { get; set; } = new List<Label>();

		public CodeBlock(string code, Language? language)
		{
			Code = code;
			Language = language;
		}

		public CodeBlock()
		{
		}

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			if (! Language.HasValue)
				Language = context.CourseSettings.DefaultLanguage;
			yield return this;
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			var urlName = slide.NormalizedGuid + componentIndex;
			Debug.Assert(Language != null, nameof(Language) + " != null");
			return new CodeComponent(urlName, displayName, urlName, Language.Value, Code);
		}

		public override string ToString()
		{
			return $"{Language} code: {Code}";
		}

		public override string TryGetText()
		{
			return Code;
		}
	}
}