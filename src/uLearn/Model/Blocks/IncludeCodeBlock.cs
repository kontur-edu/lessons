using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace uLearn.Model.Blocks
{
	[XmlType("include-code")]
	public class IncludeCodeBlock : IncludeCode
	{
		[XmlElement("display")]
		public Label[] DisplayLabels { get; set; }

		public IncludeCodeBlock(string file) : base(file)
		{
			File = file;
		}

		public IncludeCodeBlock()
		{
		}


		public override IEnumerable<SlideBlock> BuildUp(BuildUpContext context, IImmutableSet<string> filesInProgress)
		{
			FillProperties(context);
			DisplayLabels = DisplayLabels ?? new Label[0];

			if (DisplayLabels.Length == 0)
			{
				var content = context.FileSystem.GetContent(File);
				yield return new CodeBlock(content, LangId, LangVer);
				yield break;
			}

			var extractor = context.GetExtractor(File, LangId);
			yield return new CodeBlock(string.Join("\r\n\r\n", extractor.GetRegions(DisplayLabels)), LangId, LangVer) { Hide = Hide };
		}
	}
}