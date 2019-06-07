using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using Elmah;
using log4net;
using Metrics;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Telegram;

namespace uLearn.Web
{
	public class ErrorLogModule : Elmah.ErrorLogModule
	{
		private readonly ErrorsBot errorsBot;
		
		private static readonly ILog log = LogManager.GetLogger(typeof(ErrorLogModule));

		private static readonly List<string> ignorableForTelegramChannelSubstrings = new List<string>
		{
			"The provided anti-forgery token was meant for user",
			"The required anti-forgery cookie \"__RequestVerificationToken\" is not present.",
			"A potentially dangerous Request.Path value was detected from the client"
		};

		public ErrorLogModule()
		{
			/* TODO (andgein): remove this hack */
			Utils.WebApplicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

			Logged += OnLogged;
			errorsBot = new ErrorsBot();
		}

		private bool IsErrorIgnoredForTelegramChannel(Error error)
		{
			var message = error.Exception.Message;
			return ignorableForTelegramChannelSubstrings.Any(ignorableSubstring => message.Contains(ignorableSubstring));
		}

		private void OnLogged(object sender, ErrorLoggedEventArgs args)
		{
			var error = args.Entry.Error;
			var entryId = args.Entry.Id;
			log.Error($"Произошла ошибка {entryId} (код {error.StatusCode}, подробности в Elmah):\n" +
					$"Query string: {error.QueryString.ToQueryString()}",
				error.Exception);

			if (!IsErrorIgnoredForTelegramChannel(error))
			{
				errorsBot.PostToChannel(entryId, error.Exception);
			}
		}
	}
}