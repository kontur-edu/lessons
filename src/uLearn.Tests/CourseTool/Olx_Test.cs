using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace uLearn.CourseTool
{
	[TestFixture]
	public class Olx_Test
	{
		[Test]
		public void OlxSlideProblem()
		{
			var xmlContent = @"<problem display_name=""Логический тип"" group_access=""{&quot;2115287846&quot;: [1594438175]}"" markdown=""&lt;code&gt;var x = (!(1!=2) == false) &amp;amp;&amp;amp; (2*3 == 8 || false != !false)&lt;/code&gt;&#10;&#10;&gt;&gt;Чему равен x?&lt;&lt;&#10;&#10;=true"" max_attempts=""1"" show_is_answer_correct=""false"" showanswer=""never"">
  <code>var x = (!(1!=2) == false) &amp;&amp; (2*3 == 8 || false != !false)</code>
  <p>Чему равен x?</p>
  <stringresponse answer=""true"" type=""ci"">
    <textline label=""Чему равен x?"" size=""20""/>
  </stringresponse>
</problem>";
			var res = xmlContent.DeserializeXml<SlideProblemComponent>();
		}
	}
}