using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Database
{
	public class InitialDataCreator
	{
		private readonly UlearnDb db;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UlearnUserManager userManager;
		private readonly IUsersRepo usersRepo;
		
		private readonly string sysAdminRole = LmsRoleType.SysAdmin.ToString();

		public InitialDataCreator(
			UlearnDb db,
			RoleManager<IdentityRole> roleManager,
			UlearnUserManager userManager,
			IUsersRepo usersRepo
		)
		{
			this.db = db;
			this.roleManager = roleManager;
			this.userManager = userManager;
			this.usersRepo = usersRepo;
		}

		public async Task CreateRolesAsync()
		{
			if (! await db.Roles.AnyAsync(r => r.Name == sysAdminRole).ConfigureAwait(false))
			{
				await roleManager.CreateAsync(new IdentityRole(sysAdminRole)).ConfigureAwait(false);
			}
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task CreateUsersAsync()
		{
			if (! await db.Users.AnyAsync(u => u.UserName == "user").ConfigureAwait(false))
			{
				var user = new ApplicationUser { UserName = "user", FirstName = "User", LastName = "" };
				await userManager.CreateAsync(user, "asdasd").ConfigureAwait(false);
			}
			if (! await db.Users.AnyAsync(u => u.UserName == "admin").ConfigureAwait(false))
			{
				var user = new ApplicationUser { UserName = "admin", FirstName = "System Administrator", LastName = "" };
				await userManager.CreateAsync(user, "fullcontrol").ConfigureAwait(false);
				await userManager.AddToRoleAsync(user, sysAdminRole).ConfigureAwait(false);
			}
			
			await CreateRolesAsync().ConfigureAwait(false);
		}

		public async Task CreateUlearnBotUserAsync()
		{
			await usersRepo.CreateUlearnBotUserIfNotExistsAsync().ConfigureAwait(false);

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task CreateAllAsync()
		{
			await CreateRolesAsync().ConfigureAwait(false);
			await CreateUsersAsync().ConfigureAwait(false);
			await CreateUlearnBotUserAsync().ConfigureAwait(false);
		}
	}
}