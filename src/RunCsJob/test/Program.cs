using System;

namespace test
{
	internal static class Program
	{
		private static void Main()
		{
			var a = 1;
			var b = (1, 2);
			Console.WriteLine($"Hello world!{a}{b}");
		}
	}
}