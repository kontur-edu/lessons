using System.ComponentModel.DataAnnotations;

namespace uLearn
{
	public enum SubmissionLanguage: short
	{
		[Display(Name = "C#")]
		CSharp = 1,
		
		[Display(Name = "Python 2")]
		Python2 = 2,
		
		[Display(Name = "Python 3")]
		Python3 = 3,
		
		[Display(Name = "Java")]
		Java = 4,
		
		[Display(Name = "JavaScript")]
		JavaScript = 5,
	}

	public static class SubmissionLanguageHelpers
	{
		public static SubmissionLanguage ByLangId(string langId)
		{
			langId = langId.ToLower();
			switch (langId)
			{
				case "csharp":
				case "cs":
					return SubmissionLanguage.CSharp;
				case "python":
				case "py":
					return SubmissionLanguage.Python3;
				case "java":
					return SubmissionLanguage.Java;
				case "javascript":
				case "js":
					return SubmissionLanguage.JavaScript;
				default:
					return SubmissionLanguage.CSharp;
			}
		}

		public static bool HasAutomaticChecking(this SubmissionLanguage language)
		{
			return language == SubmissionLanguage.CSharp || language == SubmissionLanguage.JavaScript;
		}
	}
}