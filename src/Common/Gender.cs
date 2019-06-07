using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ulearn.Common
{
	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum Gender : short
	{
		Male = 1,
		Female = 2
	}

	public static class GenderExtensions
	{
		public static string ChooseEnding(this Gender? gender, string male, string female, string unknown)
		{
			if (gender == null)
				return unknown;
			if (gender == Gender.Male)
				return male;
			if (gender == Gender.Female)
				return female;
			throw new Exception($"Unknown gender: {gender}");
		}

		public static string ChooseEnding(this Gender? gender, string male, string female)
		{
			return gender.ChooseEnding(male, female, $"{male}({female})");
		}

		public static string ChooseEnding(this Gender? gender)
		{
			return gender.ChooseEnding("", "а", "(а)");
		}
	}
}