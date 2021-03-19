using System.Linq;
using Database.DataContexts;
using Database.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Database
{
	public static class Configuration
	{
		// Вообще-то достаточно вызвать доин раз на новой базе
		public static void Seed(ULearnDb db)
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

			var feedRepo = new FeedRepo(db);
			feedRepo.AddFeedNotificationTransportIfNeeded(null).GetAwaiter().GetResult(); // Feed for comments

			db.SaveChanges();
		}
	}
}