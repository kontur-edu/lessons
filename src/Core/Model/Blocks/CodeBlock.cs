using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Serialization;
using uLearn.Model.Edx.EdxComponents;
using Ulearn.Common.Extensions;

namespace uLearn.Model.Blocks
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

		[XmlAttribute("lang-id")]
		public string LangId { get; set; }

		[XmlAttribute("lang-ver")]
		public string LangVer { get; set; }

		public CodeBlock(string code, string langId, string langVer = null)
		{
			Code = code;
			LangId = langId;
			LangVer = langVer;
		}

		public CodeBlock()
		{
		}

		public override IEnumerable<SlideBlock> BuildUp(BuildUpContext context, IImmutableSet<string> filesInProgress)
		{
			LangId = LangId ?? context.CourseSettings.DefaultLanguage;
			LangVer = LangVer ?? context.CourseSettings.GetLanguageVersion(LangId);
			yield return this;
		}

		public override Component ToEdxComponent(string displayName, Slide slide, int componentIndex)
		{
			var urlName = slide.NormalizedGuid + componentIndex;
			return new CodeComponent(urlName, displayName, urlName, LangId, Code);
		}

		public override string ToString()
		{
			return $"{LangId} code {Code}";
		}

		public override string TryGetText()
		{
			return Code;
		}
	}
}