using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace uLearn.Web
{
	public static class PluralizeExtensions
	{
		public static string PluralizeInRussian(this int number, RussianPluralizationContext context, bool smallNumbersAreWords=true, bool hideNumberOne=true)
		{
			if (smallNumbersAreWords)
			{
				// TODO: feminine
				switch (number)
				{
					case 1:
						return string.Format("{0}{1}", hideNumberOne ? "" : "один ", context.One);
					case 2:
						return string.Format("два {0}", context.Two);
					case 3:
						return string.Format("три {0}", context.Two);
				}
			}
			var lastDigit = number % 10;
			var word = context.Five;
			if (number % 100 < 10 || number % 100 > 20)
			{
				if (lastDigit == 1)
					word = context.One;
				if (lastDigit >= 2 && lastDigit <= 4)
					word = context.Two;
			}
			return string.Format("{0} {1}", number, word);
		}
	}

	public class RussianPluralizationContext
	{
		public string One;
		public string Two;
		public string Five;
	}
}