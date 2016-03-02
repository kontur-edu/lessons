using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace uLearn.Web
{
	public static class StringExtensions
	{
		public static string TruncateWithEllipsis(this string s, int maxLength, string ellipsis="...")
		{
			if (ellipsis.Length > maxLength)
				throw new ArgumentOutOfRangeException("length", maxLength, "length must be at least as long as ellipsis.");

			if (s.Length > maxLength - ellipsis.Length)
				return s.Substring(0, maxLength - ellipsis.Length) + ellipsis;

			return s;
		}
	}
}