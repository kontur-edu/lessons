using System;
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
	[XmlType("includeCode")]
	public class IncludeCodeBlock : SlideBlock
	{
		public IncludeCodeBlock(string codeFile)
		{
			CodeFile = codeFile;
		}

		public IncludeCodeBlock()
		{
		}
		
		[XmlAttribute("file")]
		public string CodeFile { get; set; }
		
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

		[XmlElement("display")]
		public Label[] DisplayLabels { get; set; }
		
		protected void FillProperties(SlideBuildingContext context)
		{
			CodeFile = CodeFile ?? context.Slide.DefaultIncludeCodeFile ?? context.Unit.Settings?.DefaultIncludeCodeFile;
			if (CodeFile == null)
				throw new CourseLoadingException($"У блока <include-code> не указан атрибут file.");
				
			if (!Language.HasValue)
				Language = LanguageHelpers.GuessByExtension(new FileInfo(CodeFile));
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			if (!string.IsNullOrEmpty(CodeFile))
				throw new Exception("IncludeCodeBlock: File string is not empty.");
			return new CodeComponent();
		}

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			FillProperties(context);
			DisplayLabels = DisplayLabels ?? new Label[0];

			if (DisplayLabels.Length == 0)
			{
				var content = context.UnitDirectory.GetContent(CodeFile);
				yield return new CodeBlock(content, Language) { Hide = Hide };
				yield break;
			}

			var extractor = context.GetExtractor(CodeFile, Language);
			yield return new CodeBlock(string.Join("\r\n\r\n", extractor.GetRegions(DisplayLabels, withoutAttributes: true)), Language) { Hide = Hide };
		}
	}
}