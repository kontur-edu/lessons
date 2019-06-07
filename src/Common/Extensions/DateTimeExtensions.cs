using System;

namespace Ulearn.Common.Extensions
{
	public static class DateTimeExtensions
	{
		public static string ToSortable(this DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-ddTHH.mm.ss.ff");
		}
	}
}