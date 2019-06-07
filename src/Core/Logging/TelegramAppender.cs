using log4net.Appender;
using log4net.Core;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Ulearn.Common.Extensions;
using Ulearn.Core.Telegram;

namespace Ulearn.Core.Logging
{
	public class TelegramAppender : AppenderSkeleton
	{
		private static readonly ErrorsBot errorsBot = new ErrorsBot();
		
		protected override void Append(LoggingEvent loggingEvent)
		{
			/* Ignore telegram exceptions to stop infinity flood */
			if (loggingEvent.ExceptionObject is ApiRequestException)
				return;
			
			var message = $"{loggingEvent.Level} log message from *{loggingEvent.LoggerName.EscapeMarkdown()}*:\n\n```{RenderLoggingEvent(loggingEvent).EscapeMarkdown()}```";
			
			errorsBot.PostToChannel(message, ParseMode.Markdown);
		}
		
		/// <summary>
		/// This appender requires a <see cref="log4net.Layout"/> to be set.
		/// </summary>
		protected override bool RequiresLayout => true;
	}
}