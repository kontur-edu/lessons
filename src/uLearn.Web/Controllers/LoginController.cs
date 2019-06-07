using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Database.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Kontur.Passport;
using uLearn.Web.Microsoft.Owin.Security.VK;
using uLearn.Web.Models;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Controllers
{
	public class LoginController : BaseUserController
	{
		public ActionResult Index(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		[HandleHttpAntiForgeryException]
		public async Task<ActionResult> Index(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindAsync(model.UserName, model.Password).ConfigureAwait(false);

				if (user == null)
				{
					/* If user with this username is not exists then try to find user with this email.
					   It allows to login not only with username/password, but with email/password */
					var usersWithEmail = usersRepo.FindUsersByEmail(model.UserName);
					
					/* For signing in via email/password we need to be sure that email is confirmed */
					user = usersWithEmail.FirstOrDefault(u => u.EmailConfirmed);

					if (user != null)
					{
						if (!await userManager.CheckPasswordAsync(user, model.Password).ConfigureAwait(false))
							user = null;
					}
				}
				
				if (user != null)
				{
					await AuthenticationManager.LoginAsync(HttpContext, user, model.RememberMe).ConfigureAwait(false);
					await SendConfirmationEmailAfterLogin(user).ConfigureAwait(false);
					return Redirect(this.FixRedirectUrl(returnUrl));
				}
				ModelState.AddModelError("", @"Неверное имя пользователя или пароль");
			}

			/* If we got this far, something failed, redisplay form */
			ViewBag.ReturnUrl = returnUrl;
			return View(model);
		}

		private async Task SendConfirmationEmailAfterLogin(ApplicationUser user)
		{
			if (string.IsNullOrEmpty(user.Email) || user.EmailConfirmed)
				return;

			/* Reset cookie and show popup with remainder if needed */
			Response.Cookies.Add(new HttpCookie("emailIsNotConfirmed") { Value = null, Expires = DateTime.Now.AddDays(-1) });

			if (user.LastConfirmationEmailTime == null)
				await SendConfirmationEmail(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[HandleHttpAntiForgeryException]
		public ActionResult ExternalLogin(string provider, string returnUrl)
		{
			// Request a redirect to the external login provider
			return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
		}

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
				await UpdateUserFieldsFromExternalLoginInfo(user.Id, loginInfo);

				await AuthenticationManager.LoginAsync(HttpContext, user, isPersistent: false);
				await SendConfirmationEmailAfterLogin(user);
				return Redirect(this.FixRedirectUrl(returnUrl));
			}

			if (loginInfo.ExternalIdentity.AuthenticationType == VkAuthenticationConstants.AuthenticationType)
				metricSender.SendCount("registration.via_vk.try");
			else if (loginInfo.ExternalIdentity.AuthenticationType == KonturPassportConstants.AuthenticationType)
				metricSender.SendCount("registration.via_kontur_passport.try");

			// If the user does not have an account, then prompt the user to create an account
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

			Gender? loginGender = null;
			loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.Gender)?.TryParseToNullableEnum(out loginGender);
			return View("ExternalLoginConfirmation",
				new ExternalLoginConfirmationViewModel { UserName = null, Email = loginInfo.Email, Gender = loginGender });
		}

		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		[HandleHttpAntiForgeryException]
		public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Manage", "Account");
			}
			var info = await AuthenticationManager.GetExternalLoginInfoAsync(HttpContext);
			if (info == null)
			{
				return View("ExternalLoginFailure");
			}
			
			ViewBag.LoginProvider = info.Login.LoginProvider;
			ViewBag.ReturnUrl = returnUrl;
			
			if (ModelState.IsValid)
			{
				var userAvatarUrl = info.ExternalIdentity.FindFirstValue("AvatarUrl");
				var firstName = info.ExternalIdentity.FindFirstValue(ClaimTypes.GivenName);
				var lastName = info.ExternalIdentity.FindFirstValue(ClaimTypes.Surname);
				
				/* Some users enter email with trailing whitespaces. Remove them (not users, but spaces!) */
				model.Email = (model.Email ?? "").Trim();
				
				if (!CanNewUserSetThisEmail(model.Email))
				{
					ModelState.AddModelError("Email", AccountController.ManageMessageId.EmailAlreadyTaken.GetDisplayName());
					return View(model);
				}

				var user = new ApplicationUser
				{
					UserName = model.UserName,
					FirstName = firstName,
					LastName = lastName,
					AvatarUrl = userAvatarUrl,
					Email = model.Email,
					Gender = model.Gender,
				};
				var result = await userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await userManager.AddLoginAsync(user.Id, info.Login);
					if (result.Succeeded)
					{
						await userManager.AddPasswordAsync(user.Id, model.Password);
						await AuthenticationManager.LoginAsync(HttpContext, user, isPersistent: false);
						if (!await SendConfirmationEmail(user))
						{
							log.Warn("ExternalLoginConfirmation(): can't send confirmation email");
							return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.ErrorOccured });
						}

						metricSender.SendCount("registration.success");
						if (info.ExternalIdentity.AuthenticationType == VkAuthenticationConstants.AuthenticationType)
							metricSender.SendCount("registration.via_vk.success");
						else if (info.ExternalIdentity.AuthenticationType == KonturPassportConstants.AuthenticationType)
							metricSender.SendCount("registration.via_kontur_passport.success");

						return Redirect(this.FixRedirectUrl(returnUrl));
					}
				}
				this.AddErrors(result);
			}
			
			return View(model);
		}

		public ActionResult ExternalLoginFailure()
		{
			return View();
		}

		[HttpGet]
		[ULearnAuthorize]
		public ActionResult LinkLogin(string provider, string returnUrl)
		{
			return View(new LinkLoginViewModel
			{
				Provider = provider,
				ReturnUrl = returnUrl,
			});
		}

		[HttpPost]
		[ULearnAuthorize]
		[ValidateAntiForgeryToken]
		[HandleHttpAntiForgeryException]
		public ActionResult DoLinkLogin(string provider, string returnUrl="")
		{
			// Request a redirect to the external login provider to link a login for the current user
			return new ChallengeResult(provider, Url.Action("LinkLoginCallback", new { returnUrl = returnUrl }), User.Identity.GetUserId());
		}

		[ULearnAuthorize]
		public async Task<ActionResult> LinkLoginCallback(string returnUrl="")
		{
			var userId = User.Identity.GetUserId();
			var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(HttpContext, XsrfKey, userId);
			if (loginInfo == null)
			{
				log.Warn("LinkLoginCallback: GetExternalLoginInfoAsync() returned null");
				return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.ErrorOccured });
			}
			var result = await userManager.AddLoginAsync(userId, loginInfo.Login);
			if (result.Succeeded)
			{
				await UpdateUserFieldsFromExternalLoginInfo(userId, loginInfo);
				
				if (!string.IsNullOrEmpty(returnUrl))
					return Redirect(this.FixRedirectUrl(returnUrl));

				return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.LoginAdded });
			}
			var otherUser = await userManager.FindAsync(loginInfo.Login);
			return RedirectToAction("Manage", "Account", new { Message = AccountController.ManageMessageId.AlreadyLinkedToOtherUser, Provider = loginInfo.Login.LoginProvider, OtherUserId = otherUser?.Id ?? "" });
		}

		private async Task UpdateUserFieldsFromExternalLoginInfo(string userId, ExternalLoginInfo loginInfo)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
				return;
			
			var avatarUrl = loginInfo.ExternalIdentity.FindFirstValue("AvatarUrl");
			var konturLogin = loginInfo.ExternalIdentity.FindFirstValue("KonturLogin");
			var sex = loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.Gender);
			Gender? userSex = null;
			if (Enum.TryParse(sex, out Gender parsedSex))
				userSex = parsedSex;

			if (!string.IsNullOrEmpty(avatarUrl))
				user.AvatarUrl = avatarUrl;
			if (!string.IsNullOrEmpty(konturLogin))
				user.KonturLogin = konturLogin;
			if (userSex != null && user.Gender == null)
				user.Gender = userSex;
			
			await userManager.UpdateAsync(user);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[HandleHttpAntiForgeryException]
		public ActionResult LogOff()
		{
			AuthenticationManager.Logout(HttpContext);
			return RedirectToAction("Index", "Home");
		}

		public ActionResult ExternalLoginsListPartial(ExternalLoginsListModel model)
		{
			if (User.Identity.IsAuthenticated)
			{
				var userId = User.Identity.GetUserId();
				var user = userManager.FindById(userId);
				model.UserLogins = user.Logins.ToList();
			}
			return PartialView(model);
		}

		public ActionResult EnsureKonturProfileLogin(string returnUrl)
		{
			if (User.Identity.IsAuthenticated)
			{
				var userId = User.Identity.GetUserId();
				var user = userManager.FindById(userId);
				var hasKonturPassportLogin = user.Logins.Any(l => l.LoginProvider == KonturPassportConstants.AuthenticationType);
				if (hasKonturPassportLogin)
					return Redirect(this.FixRedirectUrl(returnUrl));

				return View("AddKonturProfileLogin", model: returnUrl);
			}
			else
			{
				var newReturnUrl = Url.Action("EnsureKonturProfileLogin", "Login", new { returnUrl = returnUrl });
				return View("EnsureKonturProfileLogin", model: newReturnUrl);
			}
		}

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

	public class LinkLoginViewModel
	{
		public string Provider { get; set; }

		public string ReturnUrl { get; set; }
	}
}