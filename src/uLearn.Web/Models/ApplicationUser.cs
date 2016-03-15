using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace uLearn.Web.Models
{
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	public class ApplicationUser : IdentityUser
	{
		public virtual ICollection<UserSolution> Solutions { get; set; }
		public virtual ICollection<UserQuestion> Questions { get; set; }

		public string GroupName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime? LastEdit { get; set; }

		// AvatarUrl is empty if user has no avatar
		public string AvatarUrl { get; set; }

		public bool HasAvatar
		{
			get { return ! string.IsNullOrEmpty(AvatarUrl); }
		}

		public string VisibleName
		{
			get
			{
				if (FirstName + LastName == "")
					return "Пользователь";
				return FirstName + " " + LastName;
			}
		}
	}
}