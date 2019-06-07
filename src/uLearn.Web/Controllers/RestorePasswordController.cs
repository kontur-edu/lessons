using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using Database.DataContexts;
using Database.Models;
using Kontur.Spam.Client;
using log4net;
using Metrics;
using Microsoft.AspNet.Identity;
using uLearn.Web.Models;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Message = uLearn.Web.Models.Message;

namespace uLearn.Web.Controllers
{
	public class RestorePasswordController : Controller
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(RestorePasswordController));
		private readonly RestoreRequestRepo requestRepo;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly ULearnDb db;
		private readonly MetricSender metricSender;

		private readonly string spamChannelId;
		private readonly SpamClient spamClient;

		public RestorePasswordController(ULearnDb db)
		{
			this.db = db;
			userManager = new ULearnUserManager(db);
			requestRepo = new RestoreRequestRepo(db);
			metricSender = new MetricSender(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName);

			var spamEndpoint = WebConfigurationManager.AppSettings["ulearn.spam.endpoint"] ?? "";
			var spamLogin = WebConfigurationManager.AppSettings["ulearn.spam.login"] ?? "ulearn";
			var spamPassword = WebConfigurationManager.AppSettings["ulearn.spam.password"] ?? "";
			spamChannelId = WebConfigurationManager.AppSettings["ulearn.spam.channels.passwords"] ?? "";

			try
			{
				spamClient = new SpamClient(new Uri(spamEndpoint), spamLogin, spamPassword);
			}
			catch (Exception e)
			{
				log.Error($"Can\'t initialize Spam.API client to {spamEndpoint}, login {spamLogin}, password {spamPassword.MaskAsSecret()}", e);
				throw;
			}
		}

		public RestorePasswordController()
			: this(new ULearnDb())
		{
		}

		public ActionResult Index()
		{
			return View(new RestorePasswordModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[HandleHttpAntiForgeryException]
		public async Task<ActionResult> Index(string username)
		{
			metricSender.SendCount("restore_password.try");
			var users = await FindUsers(username);
			var answer = new RestorePasswordModel
			{
				UserName = username
			};

			if (!users.Any())
			{
				answer.Messages.Add(new Message($"Пользователь {username} не найден"));
				return View(answer);
			}

			metricSender.SendCount("restore_password.found_users");

			foreach (var user in users)
			{
				if (string.IsNullOrWhiteSpace(user.Email))
				{
					answer.Messages.Add(new Message($"У пользователя {user.UserName} не указана электронная почта"));
					continue;
				}

				var requestId = await requestRepo.CreateRequest(user.Id);

				if (requestId == null)
				{
					answer.Messages.Add(new Message($"Слишком частые запросы для пользователя {user.UserName}. Попробуйте ещё раз через несколько минут"));
					continue;
				}

				await SendRestorePasswordEmail(requestId, user);
				answer.Messages.Add(new Message($"Письмо с инструкцией по восстановлению пароля для пользователя {user.UserName} отправлено вам на почту", false));
			}

			return View(answer);
		}

		private async Task<List<ApplicationUser>> FindUsers(string info)
		{
			var user = await userManager.FindByNameAsync(info);
			if (user != null)
				return new List<ApplicationUser> { user };
			return db.Users.Where(u => u.Email == info && ! u.IsDeleted).ToList();
		}

		private async Task SendRestorePasswordEmail(string requestId, ApplicationUser user)
		{
			var url = Url.Action("SetNewPassword", "RestorePassword", new { requestId }, "https");

			var subject = "Восстановление пароля от ulearn.me";
			var textBody = "Чтобы изменить пароль к аккаунту " + user.UserName + ", перейдите по ссылке: " + url + ".";
			var htmlBody = "Чтобы изменить пароль к аккаунту " + user.UserName.EscapeHtml() + ", перейдите по ссылке: <a href=\"" + url + "\">" + url + "</a>.";
			var messageInfo = new MessageSentInfo
			{
				RecipientAddress = user.Email,
				Subject = subject,
				Text = textBody,
				Html = htmlBody
			};

			log.Info($"Пытаюсь отправить емэйл на {user.Email} с темой «{subject}», text: {textBody.Replace("\n", @" \\ ")}");
			try
			{
				await spamClient.SentMessageAsync(spamChannelId, messageInfo);
			}
			catch (Exception e)
			{
				log.Error($"Не смог отправить емэйл через Spam.API на {user.Email} с темой «{subject}»", e);
				throw;
			}

			metricSender.SendCount("restore_password.send_email");
		}

		public ActionResult SetNewPassword(string requestId)
		{
			if (!requestRepo.ContainsRequest(requestId))
				return View(new SetNewPasswordModel
				{
					Errors = new[] { "Запрос не найден" }
				});

			return View(new SetNewPasswordModel
			{
				RequestId = requestId,
				Errors = new string[0]
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ValidateInput(false)]
		[HandleHttpAntiForgeryException]
		public async Task<ActionResult> SetNewPassword(SetNewPasswordModel model)
		{
			var answer = new SetNewPasswordModel
			{
				RequestId = model.RequestId
			};
			if (!ModelState.IsValid)
			{
				answer.Errors = ModelState.Values.SelectMany(state => state.Errors.Select(error => error.ErrorMessage)).ToArray();
				return View(answer);
			}

			var userId = requestRepo.FindUserId(model.RequestId);
			if (userId == null)
			{
				answer.Errors = new[] { "Запрос не найден" };
				answer.RequestId = null;
				return View(answer);
			}

			var result = await userManager.RemovePasswordAsync(userId);
			if (!result.Succeeded)
			{
				answer.Errors = result.Errors.ToArray();
				return View(answer);
			}

			result = await userManager.AddPasswordAsync(userId, model.NewPassword);
			if (!result.Succeeded)
			{
				answer.Errors = result.Errors.ToArray();
				return View(answer);
			}
			metricSender.SendCount("restore_password.set_new_password");

			await requestRepo.DeleteRequest(model.RequestId);

			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				answer.Errors = new[] { "Пользователь был удалён администраторами" };
				return View(answer);
			}
			await AuthenticationManager.LoginAsync(HttpContext, user, false);

			return RedirectToAction("Index", "Home");
		}
	}
}