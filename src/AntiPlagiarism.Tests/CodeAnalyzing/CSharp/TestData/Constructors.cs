using System;

namespace AntiPlagiarism.Tests.CodeAnalyzing.CSharp.TestData
{
	public class Constructors
	{
		public Constructors() : this(1)
		{
			Console.WriteLine("Contructor 1");
		}

		public Constructors(int a)
		{
			Console.WriteLine("Contructor 2");
		}

		public Constructors(Constructors other)
		{
			Console.WriteLine("Contructor 3");
		}

		public void JustMethod()
		{
			Console.WriteLine("JustMethod");
		}
	}
}