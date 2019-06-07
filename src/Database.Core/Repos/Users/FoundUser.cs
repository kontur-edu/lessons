using System.Collections.Generic;
using Database.Models;

namespace Database.Repos.Users
{
	public class FoundUser
	{
		public ApplicationUser User { get; set; }
		
		public HashSet<SearchField> Fields { get; set; }
	}
}