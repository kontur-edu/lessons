using System;
using System.Data.Entity;
using Database.DataContexts;
using log4net;
using LtiLibrary.Core.Common;
using LtiLibrary.Core.OAuth;
using LtiLibrary.Owin.Security.Lti;
using LtiLibrary.Owin.Security.Lti.Provider;
using Owin;
using uLearn.Web.Extensions;

namespace uLearn.Web.LTI
{
	public static class LtiAuthenticationExtensions
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LtiAuthenticationExtensions));

		public static IAppBuilder UseLtiAuthentication(this IAppBuilder app)
		{
			app.UseLtiAuthentication(new LtiAuthenticationOptions
			{
				Provider = new LtiAuthenticationProvider
				{
					// Look up the secret for the consumer
					OnAuthenticate = async context =>
					{
						// Make sure the request is not being replayed
						var timeout = TimeSpan.FromMinutes(5);
						var oauthTimestampAbsolute = OAuthConstants.Epoch.AddSeconds(context.LtiRequest.Timestamp);
						if (DateTime.UtcNow - oauthTimestampAbsolute > timeout)
						{
							log.Error($"Неправильное время у LTI-запроса: {context.LtiRequest.Timestamp}, сейчас {DateTime.UtcNow}, прошло больше 5 минут");
							throw new LtiException("Expired " + OAuthConstants.TimestampParameter);
						}

						var db = new ULearnDb();
						var consumer = await db.Consumers.SingleOrDefaultAsync(c => c.Key == context.LtiRequest.ConsumerKey);
						if (consumer == null)
						{
							throw new LtiException("Invalid " + OAuthConstants.ConsumerKeyParameter + " " + context.LtiRequest.ConsumerKey);
						}

						/* Substitute http(s) scheme with real scheme from header */
						var uriBuilder = new UriBuilder(context.LtiRequest.Url)
						{
							Scheme = context.OwinContext.Request.GetRealRequestScheme(),
							Port = context.OwinContext.Request.GetRealRequestPort()
						};
						context.LtiRequest.Url = uriBuilder.Uri;

						var signature = context.LtiRequest.GenerateSignature(consumer.Secret);
						if (!signature.Equals(context.LtiRequest.Signature))
						{
							throw new LtiException("Invalid " + OAuthConstants.SignatureParameter);
						}

						// If we made it this far the request is valid
					},

					// Sign in using application authentication. This handler will create a new application
					// user if no matching application user is found.
					OnAuthenticated = context => SecurityHandler.OnAuthenticated(context),

					// Generate a username using the LisPersonEmailPrimary from the LTI request
					OnGenerateUserName = context => SecurityHandler.OnGenerateUserName(context)
				},
				//SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
				SignInAsAuthenticationType = "Identity.Application"
			});

			return app;
		}
	}
}