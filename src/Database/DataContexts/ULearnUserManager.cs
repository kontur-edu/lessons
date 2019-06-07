using System.Threading.Tasks;
using Database.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Database.DataContexts
{
	public class ULearnUserManager : UserManager<ApplicationUser>
	{
		public ULearnUserManager(IUserStore<ApplicationUser> store)
			: base(store)
		{
			UserValidator = new UserValidator<ApplicationUser>(this)
			{
				AllowOnlyAlphanumericUserNames = false
			};
		}

		public ULearnUserManager(ULearnDb db)
			: this(new UserStore<ApplicationUser>(db))
		{
		}

		public override async Task<ApplicationUser> FindByIdAsync(string userId)
		{
			var user = await base.FindByIdAsync(userId);
			if (user != null && user.IsDeleted)
				return null;
			return user;
		}

		public override async Task<ApplicationUser> FindByEmailAsync(string email)
		{
			var user = await base.FindByEmailAsync(email);
			if (user != null && user.IsDeleted)
				return null;
			return user;
		}

		public override async Task<ApplicationUser> FindByNameAsync(string userName)
		{
			var user = await base.FindByNameAsync(userName);
			if (user != null && user.IsDeleted)
				return null;
			return user;
		}
	}
}