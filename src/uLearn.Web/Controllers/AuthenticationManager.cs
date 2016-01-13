using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using uLearn.Web.DataContexts;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	public class AuthenticationManager
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly UserRolesRepo userRoles = new UserRolesRepo();

		public AuthenticationManager()
		{
			userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ULearnDb()));
		}

		public static async Task LoginAsync(HttpContextBase context, ApplicationUser user, bool isPersistent)
		{
			await new AuthenticationManager().InternalLoginAsync(context, user, isPersistent);
		}

		public static void Logout(HttpContextBase context)
		{
			GetAuthenticationManager(context).SignOut();
		}

		public static async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(HttpContextBase context, string xsrfKey, string userId)
		{
			return await GetAuthenticationManager(context).GetExternalLoginInfoAsync(xsrfKey, userId);
		}

		private async Task InternalLoginAsync(HttpContextBase context, ApplicationUser user, bool isPersistent)
		{
			var authenticationManager = GetAuthenticationManager(context);
			authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
			var identity = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
			identity.AddCourseRoles(userRoles.GetRoles(user.Id));
			authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
		}

		public static async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(HttpContextBase context)
		{
			return await GetAuthenticationManager(context).GetExternalLoginInfoAsync();
		}

		private static IAuthenticationManager GetAuthenticationManager(HttpContextBase context)
		{
			return context.GetOwinContext().Authentication;
		}
	}
}