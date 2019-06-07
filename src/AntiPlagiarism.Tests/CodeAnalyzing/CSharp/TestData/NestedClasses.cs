using System;

namespace AntiPlagiarism.Tests.CodeAnalyzing.CSharp.TestData
{
	public class OuterClass
	{
		public class InnerClass
		{
			public int InnerField { get; set; }

			public void InnerMethod()
			{
				Console.WriteLine(nameof(InnerMethod));
			}
		}
		
		public string OuterField { get; set; }

		public void OuterMethod()
		{
			Console.WriteLine(nameof(OuterMethod));
		}
	}
}