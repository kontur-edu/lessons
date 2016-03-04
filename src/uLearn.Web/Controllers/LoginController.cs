using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	public class LoginController : Controller
	{
		private readonly UserManager<ApplicationUser> userManager;


		public LoginController()
			: this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ULearnDb())))
		{
		}

		public LoginController(UserManager<ApplicationUser> userManager)
		{
			this.userManager = userManager;
		}

		//
		// GET: /Login
		public ActionResult Index(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		//
		// POST: /Login
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Index(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindAsync(model.UserName, model.Password);
				if (user != null)
				{
					await AuthenticationManager.LoginAsync(HttpContext, user, model.RememberMe);
					return Redirect(this.FixRedirectUrl(returnUrl));
				}
				ModelState.AddModelError("", @"Неверное имя пользователя или пароль.");
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// POST: /Login/ExternalLogin
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ExternalLogin(string provider, string returnUrl)
		{
			// Request a redirect to the external login provider
			return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
		}

		//
		// GET: /Login/ExternalLoginCallback
		public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
		{
			var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(HttpContext);
			if (loginInfo == null)
			{
				return RedirectToAction("Index", "Login");
			}

			// Sign in the user with this external login provider if the user already has a login
			var user = await userManager.FindAsync(loginInfo.Login);
			if (user != null)
			{
				await AuthenticationManager.LoginAsync(HttpContext, user, isPersistent: false);
				return Redirect(this.FixRedirectUrl(returnUrl));
			}

			// If the user does not have an account, then prompt the user to create an account
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
			return View("ExternalLoginConfirmation",
				new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
		}

		//
		// POST: /Login/ExternalLoginConfirmation
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Manage", "Account");
			}

			if (ModelState.IsValid)
			{
				// Get the information about the user from the external login provider
				var info = await AuthenticationManager.GetExternalLoginInfoAsync(HttpContext);
				if (info == null)
				{
					return View("ExternalLoginFailure");
				}
				var userAvatarUrl = info.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == "AvatarUrl")?.Value;
				var user = new ApplicationUser { UserName = model.UserName, AvatarUrl = userAvatarUrl};
				var result = await userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await userManager.AddLoginAsync(user.Id, info.Login);
					if (result.Succeeded)
					{
						await AuthenticationManager.LoginAsync(HttpContext, user, isPersistent: false);
						return Redirect(this.FixRedirectUrl(returnUrl));
					}
				}
				this.AddErrors(result);
			}

			ViewBag.ReturnUrl = returnUrl;
			return View(model);
		}

		//
		// GET: /Login/ExternalLoginFailure
		public ActionResult ExternalLoginFailure()
		{
			return View();
		}

		//
		// POST: /Login/LinkLogin
		[HttpPost]
		[ULearnAuthorize]
		[ValidateAntiForgeryToken]
		public ActionResult LinkLogin(string provider)
		{
			// Request a redirect to the external login provider to link a login for the current user
			return new ChallengeResult(provider, Url.Action("LinkLoginCallback"), User.Identity.GetUserId());
		}

		//
		// GET: /Login/LinkLoginCallback
		[ULearnAuthorize]
		public async Task<ActionResult> LinkLoginCallback()
		{
			var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(HttpContext, XsrfKey, User.Identity.GetUserId());
			if (loginInfo == null)
			{
				return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.Error });
			}
			var result = await userManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
			if (result.Succeeded)
			{
				return RedirectToAction("Manage", "Account");
			}
			return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.Error });
		}

		//
		// POST: /Login/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			AuthenticationManager.Logout(HttpContext);
			return RedirectToAction("Index", "Home");
		}

		// TODO: may be need change?
		// Used for XSRF protection when adding external logins
		private const string XsrfKey = "XsrfId";

		private class ChallengeResult : HttpUnauthorizedResult
		{
			public ChallengeResult(string provider, string redirectUri, string userId = null)
			{
				LoginProvider = provider;
				RedirectUri = redirectUri;
				UserId = userId;
			}

			public string LoginProvider { get; set; }
			public string RedirectUri { get; set; }
			public string UserId { get; set; }

			public override void ExecuteResult(ControllerContext context)
			{
				var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
				if (UserId != null)
				{
					properties.Dictionary[XsrfKey] = UserId;
				}
				context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
			}
		}
	}
}