using MarkdownDeep;
using NUnit.Framework;
using uLearn.Model.Blocks;

namespace uLearn.Model
{
	[TestFixture]
	public class LessonXmlLoader_should
	{
		[Test]
		public void Load()
		{
			var xml = "<md hide='true'>asd</md>";

			var block = xml.DeserializeXml<MdBlock>();
			Assert.AreEqual("asd", block.Markdown);
			Assert.IsTrue(block.Hide);
		}

	}
}