using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using log4net;
using Ulearn.Common.Extensions;
using Ulearn.Core.Extensions;

namespace Notifications
{
	public class OneTimeEmailSender
	{
		private const string configFilename = "sender.xml";

		private static readonly ILog log = LogManager.GetLogger(typeof(OneTimeEmailSender));
		private readonly KonturSpamEmailSender emailSender;		

		public OneTimeEmailSender()
		{
			emailSender = new KonturSpamEmailSender();
		}

		public async Task SendEmailsAsync()
		{
			log.Info($"Loading config from {configFilename} (see example in {configFilename}.example)");
			OneTimeEmailSenderConfig config;
			using (var stream = new StreamReader(configFilename))
				config = (OneTimeEmailSenderConfig)new XmlSerializer(typeof(OneTimeEmailSenderConfig)).Deserialize(stream);
			
			log.Info($"Loaded config from {configFilename}:");
			log.Info(config.XmlSerialize());
			
			/* Get text from html by stripping HTML tags if text is not defined */
			if (string.IsNullOrEmpty(config.Text))
				config.Text = config.Html.StripHtmlTags();
			config.Text = config.Text.RemoveCommonNesting().Trim();
				
			foreach (var email in config.Emails)
			{
				log.Info($"Send email to {email}");
				var button = config.Button == null ? null : new EmailButton(config.Button.Link, config.Button.Text);
				await emailSender.SendEmailAsync(email, config.Subject, config.Text, config.Html, button).ConfigureAwait(false);
			}
		}
	}

	[XmlRoot("sender")]
	public class OneTimeEmailSenderConfig
	{
		[XmlElement("email")]
		public List<string> Emails { get; set; }

		[XmlElement("subject")]
		public string Subject { get; set; }

		[XmlElement("text")]
		public string Text { get; set; }
		
		[XmlElement("html")]
		public string Html { get; set; }
		
		[XmlElement("button")]
		public ButtonConfig Button { get; set; }
	}

	public class ButtonConfig
	{
		[XmlElement("link")]
		public string Link { get; set; }
		
		[XmlElement("text")]
		public string Text { get; set; }
	}
}