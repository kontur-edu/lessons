using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using Database.DataContexts;
using log4net;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Controllers
{
	public class TelegramController : JsonDataContractController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(TelegramController));

		private readonly string webhookSecret;
		private readonly string protocol;
		private static TelegramBotClient telegramBot;

		private readonly NotificationsRepo notificationsRepo;

		public TelegramController()
		{
			var db = new ULearnDb();
			notificationsRepo = new NotificationsRepo(db);

			webhookSecret = WebConfigurationManager.AppSettings["ulearn.telegram.webhook.secret"] ?? "";
			var botToken = WebConfigurationManager.AppSettings["ulearn.telegram.botToken"];
			if (botToken != null)
				telegramBot = new TelegramBotClient(botToken);

			protocol = Convert.ToBoolean(WebConfigurationManager.AppSettings["ulearn.requireHttps"] ?? "true") ? "https" : "http";
		}

		[System.Web.Mvc.HttpPost]
		public async Task<ActionResult> Webhook(string secret)
		{
			if (secret != webhookSecret)
			{
				log.Warn($"Пришёл запрос с неправильным секретом: «{secret}» вместо «{webhookSecret.MaskAsSecret()}»");
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			var update = GetJsonObjectFromRequestBody<Update>();
			if (update == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			// Fix for bug: Telegram.Bot.Client doesn't know about some new messages types
			if (update.Message == null)
				return new HttpStatusCodeResult(HttpStatusCode.OK);

			switch (update.Type)
			{
				case UpdateType.Message:
					await OnMessage(update.Message);
					break;
			}

			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		private T GetJsonObjectFromRequestBody<T>()
		{
			var memstream = new MemoryStream();
			Request.InputStream.Position = 0;
			Request.InputStream.CopyTo(memstream);
			memstream.Position = 0;
			using (var reader = new StreamReader(memstream))
				return reader.ReadToEnd().DeserializeJson<T>();
		}

		public async Task OnMessage(Message message)
		{
			if (telegramBot == null)
				return;

			var text = message.Text;
			var chatId = message.Chat.Id;

			var reply = "";
			if (text == "/start" || text == "/connect")
			{
				var chatTitle = GetChatTitle(message.Chat);
				var secretHash = notificationsRepo.GetSecretHashForTelegramTransport(chatId, chatTitle, webhookSecret);
				var createTransportUrl = Url.Action("AddTelegram", "Account", new { chatId = chatId, chatTitle = chatTitle, hash = secretHash }, "https");

				reply = $"Чтобы получать уведомления от ulearn.me в телеграм, перейдите по ссылке {createTransportUrl}";
				if (text == "/start")
					reply = "Добро пожаловать к боту ulearn.me!\n\n" + reply;
			}
			else if (text == "/help")
			{
				reply = "Наш бот поддерживает следующие команды:\n\n" +
						"/connect — подключает получение уведомлений от ulearn.me в телеграм\n" +
						"/help — показывает это сообщение\n";
			}
			else
			{
				reply = "Не понимаю такой команды. Нажмите /help, чтобы узнать, что я могу.";
			}

			if (!string.IsNullOrEmpty(reply))
			{
				try
				{
					await telegramBot.SendTextMessageAsync(chatId, reply);
				}
				catch (ApiRequestException e) when (e.Message.Contains("bot was blocked by the user"))
				{
					log.Warn(e);
				}
			}
		}

		private static string GetChatTitle(Chat chat)
		{
			var chatTitle = "@" + chat.Username;
			if (string.IsNullOrEmpty(chatTitle))
				chatTitle = $"{chat.FirstName} {chat.LastName}";
			if (string.IsNullOrEmpty(chatTitle))
				chatTitle = chat.Title;
			return chatTitle;
		}
	}
}