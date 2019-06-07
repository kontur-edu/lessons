using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using SkbKontur.Passport.Client;
using uLearn.Web.Extensions;
using uLearn.Web.Kontur.Passport.Provider;

namespace uLearn.Web.Kontur.Passport
{
	public class KonturPassportAuthenticationHandler : AuthenticationHandler<KonturPassportAuthenticationOptions>
	{
		private const string xmlSchemaForStringType = "http://www.w3.org/2001/XMLSchema#string";
		private const string konturStateCookieName = "kontur.passport.state";

		private readonly PassportClient passportClient;
		private readonly ILog log;

		public KonturPassportAuthenticationHandler(PassportClient passportClient, ILog log)
		{
			this.passportClient = passportClient;
			this.log = log;
		}

		/// <summary>
		/// The core authentication logic which must be provided by the handler. Will be invoked at most
		/// once per request. Do not call directly, call the wrapping Authenticate method instead.
		/// </summary>
		/// <returns>The ticket data provided by the authentication logic</returns>
		protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			try
			{
				var queryString = HttpUtility.ParseQueryString(Request.QueryString.Value).ToDictionary();

				var state = Request.Cookies[konturStateCookieName];
				if (state == null)
					return null;

				var redirectUri = GetRedirectUri(Request);
				var authenticationResult = passportClient.Authenticate(queryString, redirectUri, state);
				if (authenticationResult.IsError)
				{
					log.Error(authenticationResult.ErrorMessage);
					return null;
				}
				if (!authenticationResult.Authenticated)
				{
					log.Error("Kontur.Passport returned non-authenticated status");
					return null;
				}

				var identity = new ClaimsIdentity(Options.AuthenticationType);

				var userClaims = authenticationResult.UserClaims.ToList();
				log.Info($"Received follow user claims from Kontur.Passport server: {string.Join(", ", userClaims.Select(c => c.Type + ": " + c.Value))}");
				var login = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.LoginClaimType)?.Value;
				var sid = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.SidClaimType)?.Value;
				var email = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.EmailClaimType)?.Value;
				var avatarUrl = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.AvatarUrlClaimType)?.Value;
				var realNameParts = userClaims.FirstOrDefault(c => c.Type == KonturPassportConstants.NameClaimType)?.Value.Split(' ');
				identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, sid, xmlSchemaForStringType, Options.AuthenticationType));
				identity.AddClaim(new Claim(ClaimTypes.Email, email, xmlSchemaForStringType, Options.AuthenticationType));
				identity.AddClaim(new Claim("AvatarUrl", avatarUrl, xmlSchemaForStringType, Options.AuthenticationType));
				if (realNameParts != null && realNameParts.Length > 0)
				{
					/* Suppose that Гейн Андрей Александрович is Surname (Гейн), GivenName (Андрей) and other. So we splitted name from Kontur.Passport into parts */
					identity.AddClaim(new Claim(ClaimTypes.Surname, realNameParts[0], xmlSchemaForStringType, Options.AuthenticationType));
					if (realNameParts.Length > 1)
						identity.AddClaim(new Claim(ClaimTypes.GivenName, realNameParts[1], xmlSchemaForStringType, Options.AuthenticationType));
				}

				/* Replace name from Kontur\andgein to andgein */
				if (login != null && login.Contains(@"\"))
					login = login.Substring(login.IndexOf('\\') + 1);
				
				identity.AddClaim(new Claim(ClaimTypes.Name, login, xmlSchemaForStringType, Options.AuthenticationType));
				identity.AddClaim(new Claim("KonturLogin", login, xmlSchemaForStringType, Options.AuthenticationType));				

				var properties = Options.StateDataFormat.Unprotect(state);
				return new AuthenticationTicket(identity, properties);
			}
			catch (Exception ex)
			{
				log.Error(ex.Message);
				throw;
			}
		}

		private Uri GetRedirectUri(IOwinRequest request)
		{
			var requestProtoAndHost = request.GetRealRequestScheme() + "://" + request.Host;
			return new Uri(requestProtoAndHost + request.PathBase + Options.ReturnEndpointPath);
		}

		/// <summary>
		/// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
		/// deals an authentication interaction as part of it's request flow. (like adding a response header, or
		/// changing the 401 result to 302 of a login page or external sign-in location.)
		/// </summary>
		/// <returns></returns>
		protected override Task ApplyResponseChallengeAsync()
		{
    		if (Response.StatusCode != 401)
			{
				return Task.FromResult<object>(null);
			}

			var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

			if (challenge != null)
			{
				log.Debug("ApplyResponseChallenge");
				var redirectUri = GetRedirectUri(Request);
				var currentUri = Request.Uri.ToString();

				var properties = challenge.Properties;
				if (string.IsNullOrEmpty(properties.RedirectUri))
				{
					properties.RedirectUri = currentUri;
				}

				// OAuth2 10.12 CSRF
				GenerateCorrelationId(properties);

				var state = Options.StateDataFormat.Protect(properties);
				var loginUri = passportClient.GetLoginUri(redirectUri, state);
				log.Debug($"Login url: {loginUri}");
				Response.Cookies.Append(konturStateCookieName, state);

				Response.Redirect(loginUri.ToString());
			}

			return Task.FromResult<object>(null);
		}

		/// <summary>
		/// Called once by common code after initialization. If an authentication middleware responds directly to
		/// specifically known paths it must override this virtual, compare the request path to it's known paths,
		/// provide any response information as appropriate, and true to stop further processing.
		/// </summary>
		/// <returns>Returning false will cause the common code to call the next middleware in line. Returning true will
		/// cause the common code to begin the async completion journey without calling the rest of the middleware
		/// pipeline.</returns>
		public override async Task<bool> InvokeAsync()
		{
			if (Options.ReturnEndpointPath != null &&
				string.Equals(Options.ReturnEndpointPath, Request.Path.Value, StringComparison.OrdinalIgnoreCase))
			{
				var ticket = await AuthenticateAsync();
				ticket.Properties.IsPersistent = true;

				var context = new KonturPassportReturnEndpointContext(Context, ticket)
				{
					SignInAsAuthenticationType = Options.SignInAsAuthenticationType,
					RedirectUri = ticket.Properties.RedirectUri
				};

				if (context.SignInAsAuthenticationType != null && context.Identity != null)
				{
					var grantIdentity = context.Identity;
					if (!string.Equals(grantIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
					{
						grantIdentity = new ClaimsIdentity(grantIdentity.Claims, context.SignInAsAuthenticationType);
					}
					Context.Authentication.SignIn(context.Properties, grantIdentity);
				}

				if (!context.IsRequestCompleted && context.RedirectUri != null)
				{
					var redirectUri = context.RedirectUri;
					Response.Redirect(redirectUri);
					context.RequestCompleted();
				}

				return context.IsRequestCompleted;
			}
			return false;
		}
	}
}