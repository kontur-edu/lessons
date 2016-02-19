using System;

namespace uLearn.Web
{
	public static class DateTimeExtensions
	{
		private const int secondsInHour = 60 * 60;
		private const int secondsInDay = 24 * secondsInHour;

		public static string ToPrettyString(this DateTime dateTime)
		{
			return string.Format("{0} в {1}", dateTime.ToLongDateString(), dateTime.ToShortTimeString());
		}

		public static string ToAgoPrettyString(this DateTime from)
		{
			return DateTime.Now.Subtract(from).ToPrettyString();
		}

		public static string ToPrettyString(this TimeSpan timeSpan)
		{
			var daysAgo = (int)timeSpan.TotalDays;
			var secondsAgo = (int)timeSpan.TotalSeconds;

			if (daysAgo < 0)
				return null;
			
			if (daysAgo == 0)
			{
				if (secondsAgo < 60)
					return "только что";
				if (secondsAgo < secondsInHour)
				{
					var minutesAgo = secondsAgo / 60;
					return string.Format("{0} назад", minutesAgo.PluralizeInRussian(new RussianPluralizationContext {One = "минута", Two = "минуты", Five = "минут"}));
				}
				if (secondsAgo < secondsInDay)
				{
					var hoursAgo = secondsAgo / secondsInHour;
					return string.Format("{0} назад", hoursAgo.PluralizeInRussian(new RussianPluralizationContext {One = "час", Two = "часа", Five = "часов"}));
				}
			}
			if (daysAgo == 1)
				return "вчера";

			return string.Format("{0} назад", daysAgo.PluralizeInRussian(new RussianPluralizationContext { One = "день", Two = "дня", Five = "дней" }));
			
		}
	}
}