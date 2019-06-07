using System.Web.Configuration;
using System.Web.Hosting;
using Microsoft.Owin;
using Owin;
using Telegram.Bot;
using uLearn.Web;
using Ulearn.Core;

[assembly: OwinStartup(typeof(Startup))]

namespace uLearn.Web
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			/* TODO (andgein): remove this hack */
			Utils.WebApplicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

			ConfigureAuth(app);
			InitTelegramBot();
		}

		public void InitTelegramBot()
		{
			var botToken = WebConfigurationManager.AppSettings["ulearn.telegram.botToken"] ?? "";
			if (string.IsNullOrEmpty(botToken))
				return;

			var telegramBot = new TelegramBotClient(botToken);
			var webhookSecret = WebConfigurationManager.AppSettings["ulearn.telegram.webhook.secret"] ?? "";
			var webhookDomain = WebConfigurationManager.AppSettings["ulearn.telegram.webhook.domain"] ?? "";
			var webhookUrl = $"https://{webhookDomain}/Telegram/Webhook?secret={webhookSecret}";
			telegramBot.SetWebhookAsync(webhookUrl).Wait();
		}
	}
}