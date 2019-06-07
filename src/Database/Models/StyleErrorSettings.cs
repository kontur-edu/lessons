using System.ComponentModel.DataAnnotations;
using Ulearn.Core.CSharp;

namespace Database.Models
{
	public class StyleErrorSettings
	{
		[Key]
		public StyleErrorType ErrorType { get; set; }
		
		public bool IsEnabled { get; set; }
	}
}