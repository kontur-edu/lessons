namespace Ulearn.Common.Extensions
{
	public static class PluralizeExtensions
	{
		public static string SelectPluralWordInRussian(this int number, RussianPluralizationOptions options)
		{
			var lastDigit = number % 10;
			var word = options.Five;
			if (number % 100 < 10 || number % 100 > 20)
			{
				if (lastDigit == 1)
					word = options.One;
				if (lastDigit >= 2 && lastDigit <= 4)
					word = options.Two;
			}
			return word;
		}

		public static string PluralizeInRussian(this int number, RussianPluralizationOptions options)
		{
			if (options.smallNumbersAreWords)
			{
				switch (number)
				{
					case 1:
						var one = options.Gender == Gender.Male ? "один" : "одна";
						return $"{(options.hideNumberOne ? "" : one + " ")}{options.One}";
					case 2:
						var two = options.Gender == Gender.Male ? "два" : "две";
						return $"{two} {options.Two}";
					case 3:
						return $"три {options.Two}";
				}
			}
			var word = number.SelectPluralWordInRussian(options);
			return $"{number} {word}";
		}
	}

	public class RussianPluralizationOptions
	{
		public string One;
		public string Two;
		public string Five;

		public Gender Gender = Gender.Male;

		public bool smallNumbersAreWords = true;
		public bool hideNumberOne = true;

		public static readonly RussianPluralizationOptions Score = new RussianPluralizationOptions
		{
			One = "балл",
			Two = "балла",
			Five = "баллов",
			smallNumbersAreWords = false,
			hideNumberOne = false,
		};

		public static readonly RussianPluralizationOptions ScoreDative = new RussianPluralizationOptions
		{
			One = "балла",
			Two = "баллов",
			Five = "баллов",
			smallNumbersAreWords = false,
			hideNumberOne = false,
		};

		public static readonly RussianPluralizationOptions MinuteDative = new RussianPluralizationOptions
		{
			One = "минуту",
			Two = "минуты",
			Five = "минут",
			Gender = Gender.Female,
		};

		public static readonly RussianPluralizationOptions Hour = new RussianPluralizationOptions
		{
			One = "час",
			Two = "часа",
			Five = "часов"
		};

		public static readonly RussianPluralizationOptions Day = new RussianPluralizationOptions
		{
			One = "день",
			Two = "дня",
			Five = "дней"
		};

		public static readonly RussianPluralizationOptions Man = new RussianPluralizationOptions
		{
			One = "человек",
			Two = "человека",
			Five = "человек",
			hideNumberOne = false,
			smallNumbersAreWords = false,
		};

		public static readonly RussianPluralizationOptions Tries = new RussianPluralizationOptions
		{
			One = "попытка",
			Two = "попытки",
			Five = "попыток",
			Gender = Gender.Female,
			hideNumberOne = false,
			smallNumbersAreWords = false,
		};

		public static readonly RussianPluralizationOptions Students = new RussianPluralizationOptions
		{
			One = "студент",
			Two = "студента",
			Five = "студентов",
			Gender = Gender.Male,
			hideNumberOne = false,
			smallNumbersAreWords = false
		};
		
		public static readonly RussianPluralizationOptions StudentsDative = new RussianPluralizationOptions
		{
			One = "студента",
			Two = "студентов",
			Five = "студентов",
			Gender = Gender.Male,
			hideNumberOne = true,
			smallNumbersAreWords = false
		};
	}
}