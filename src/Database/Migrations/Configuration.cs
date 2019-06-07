using System.Data.Entity.Migrations;
using System.Linq;
using Database.DataContexts;
using Database.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Database.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<ULearnDb>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
			CommandTimeout = 60 * 60; // in seconds
		}

		protected override void Seed(ULearnDb db)
		{
			var roleStore = new RoleStore<IdentityRole>(db);
			var roleManager = new RoleManager<IdentityRole>(roleStore);
			roleManager.Create(new IdentityRole(LmsRoles.SysAdmin.ToString()));

			var userStore = new UserStore<ApplicationUser>(db);
			var manager = new ULearnUserManager(userStore);
			if (!db.Users.Any(u => u.UserName == "user"))
			{
				var user = new ApplicationUser { UserName = "user", FirstName = "User", LastName = "" };
				manager.Create(user, "asdasd");
			}
			if (!db.Users.Any(u => u.UserName == "admin"))
			{
				var user = new ApplicationUser { UserName = "admin", FirstName = "System Administrator", LastName = "" };
				manager.Create(user, "fullcontrol");
				manager.AddToRole(user.Id, LmsRoles.SysAdmin.ToString());
			}
			
			var usersRepo = new UsersRepo(db);
			usersRepo.CreateUlearnBotUserIfNotExists();

			db.SaveChanges();
		}
	}
}