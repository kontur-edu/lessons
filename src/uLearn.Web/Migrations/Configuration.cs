using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Web.DataContexts;
using uLearn.Web.Models;

namespace uLearn.Web.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<ULearnDb>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
			CommandTimeout = 60 * 10; 
		}

		protected override void Seed(ULearnDb context)
		{
			var roleStore = new RoleStore<IdentityRole>(context);
			var roleManager = new RoleManager<IdentityRole>(roleStore);
			roleManager.Create(new IdentityRole(LmsRoles.SysAdmin));
			if (!context.Users.Any(u => u.UserName == "user"))
			{
				var userStore = new UserStore<ApplicationUser>();
				var manager = new UserManager<ApplicationUser>(userStore);
				var user = new ApplicationUser { UserName = "user" };
				manager.Create(user, "asdasd");
			}
			if (!context.Users.Any(u => u.UserName == "admin"))
			{
				var userStore = new UserStore<ApplicationUser>();
				var manager = new UserManager<ApplicationUser>(userStore);
				var user = new ApplicationUser { UserName = "admin" };
				manager.Create(user, "fullcontrol");
				manager.AddToRole(user.Id, LmsRoles.SysAdmin);
			}
			context.SaveChanges();
		}
	}
}