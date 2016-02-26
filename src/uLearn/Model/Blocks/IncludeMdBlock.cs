using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Serialization;
using uLearn.Model.Edx.EdxComponents;

namespace uLearn.Model.Blocks
{
	[XmlType("include-md")]
	public class IncludeMdBlock : SlideBlock
	{
		[XmlAttribute("file")]
		public string File { get; set; }

		public IncludeMdBlock(string file)
		{
			File = file;
		}

		public IncludeMdBlock()
		{
		}

		public override IEnumerable<SlideBlock> BuildUp(BuildUpContext context, IImmutableSet<string> filesInProgress)
		{
			yield return new MdBlock(context.FileSystem.GetContent(File)) { Hide = Hide };
		}

		public override Component ToEdxComponent(string displayName, Slide slide, int componentIndex)
		{
			throw new System.NotImplementedException();
		}
	}
}