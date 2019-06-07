using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Kontur.Spam.Client;
using log4net;
using Metrics;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;

namespace Notifications
{
	public class KonturSpamEmailSender : IEmailSender
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(KonturSpamEmailSender));

		private readonly MetricSender metricSender;

		private readonly ISpamClient client;
		private readonly string channelId;
		private readonly string templateId;

		public KonturSpamEmailSender()
		{
			var spamEndpoint = ConfigurationManager.AppSettings["ulearn.spam.endpoint"] ?? "";
			var spamLogin = ConfigurationManager.AppSettings["ulearn.spam.login"] ?? "ulearn";
			var spamPassword = ConfigurationManager.AppSettings["ulearn.spam.password"] ?? "";
			channelId = ConfigurationManager.AppSettings["ulearn.spam.channels.notifications"] ?? "";
			templateId = ConfigurationManager.AppSettings["ulearn.spam.templates.withButton"];

			metricSender = new MetricSender(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName);

			try
			{
				client = new SpamClient(new Uri(spamEndpoint), spamLogin, spamPassword);
			}
			catch (Exception e)
			{
				log.Error($"Can\'t initialize Spam.API client to {spamEndpoint}, login {spamLogin}, password {spamPassword.MaskAsSecret()}", e);
				throw;
			}

			log.Info($"Initialized Spam.API client to {spamEndpoint}, login {spamLogin}, password {spamPassword.MaskAsSecret()}");
			log.Info($"Using channel '{channelId}'");
		}

		public async Task SendEmailAsync(string to, string subject, string textContent = null, string htmlContent = null, EmailButton button = null, string textContentAfterButton = null, string htmlContentAfterButton = null)
		{
			var recipientEmailMetricName = to.ToLower().Replace(".", "_");

			metricSender.SendCount("send_email.try");
			metricSender.SendCount($"send_email.try.to.{recipientEmailMetricName}");

			var messageInfo = new MessageSentInfo
			{
				RecipientAddress = to,
				Subject = subject,
			};
			if (!string.IsNullOrEmpty(templateId))
			{
				messageInfo.TemplateId = templateId;
				messageInfo.Variables = new Dictionary<string, object>
				{
					{ "title", subject },
					{ "content", htmlContent },
					{ "text_content", textContent },
					{ "button", button != null },
					{ "button_link", button?.Link },
					{ "button_text", button?.Text },
					{ "content_after_button", htmlContentAfterButton },
					{ "text_content_after_button", textContentAfterButton },
				};
			}
			else
			{
				messageInfo.Html = htmlContent;
				messageInfo.Text = textContent;
			}

			log.Info($"Try to send message to {to} with subject {subject}, text: {textContent?.Replace("\n", @" \\ ")}");
			try
			{
				await client.SentMessageAsync(channelId, messageInfo);
			}
			catch (Exception e)
			{
				log.Warn($"Can\'t send message via Spam.API to {to} with subject {subject}", e);
				metricSender.SendCount("send_email.fail");
				metricSender.SendCount($"send_email.fail.to.{recipientEmailMetricName}");
				throw;
			}

			metricSender.SendCount("send_email.success");
			metricSender.SendCount($"send_email.success.to.{recipientEmailMetricName}");
		}
	}
}