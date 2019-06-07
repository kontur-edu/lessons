using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uLearn.Web.Extensions;
using uLearn.Web.Microsoft.Owin.Security.VK.Provider;

namespace uLearn.Web.Microsoft.Owin.Security.VK
{
	internal class VkAuthenticationHandler : AuthenticationHandler<VkAuthenticationOptions>
	{
		private const string vkApiVersion = "5.73";
		private const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;

		public VkAuthenticationHandler(HttpClient httpClient, ILogger logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
		{
			_logger.WriteVerbose("AuthenticateCore");

			AuthenticationProperties properties = null;

			try
			{
				string code = null;
				string state = null;

				var query = Request.Query;
				var values = query.GetValues("code");
				if (values != null && values.Count == 1)
				{
					code = values[0];
				}
				values = query.GetValues("state");
				if (values != null && values.Count == 1)
				{
					state = values[0];
				}

				properties = Options.StateDataFormat.Unprotect(state);
				if (properties == null)
				{
					return null;
				}

				string tokenEndpoint = "https://oauth.vk.com/access_token";

				string requestPrefix = Request.GetRealRequestScheme() + "://" + Request.Host;
				string redirectUri = requestPrefix + Request.PathBase + Options.ReturnEndpointPath;

				string tokenRequest =
					"?client_id=" + Uri.EscapeDataString(Options.AppId) +
					"&client_secret=" + Uri.EscapeDataString(Options.AppSecret) +
					"&code=" + Uri.EscapeDataString(code) +
					"&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
					"&v=" + vkApiVersion;

				HttpResponseMessage tokenResponse = await _httpClient.GetAsync(tokenEndpoint + tokenRequest, Request.CallCancelled);
				tokenResponse.EnsureSuccessStatusCode();
				string text = await tokenResponse.Content.ReadAsStringAsync();
				var form = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);

				var accessToken = (string)form["access_token"];
				var userId = (long)form["user_id"];

				string graphApiEndpoint = "https://api.vk.com/method/users.get" +
										"?user_id=" + userId +
										"&fields=sex,photo_100,screen_name" +
										"&name_case=Nom" +
										"&access_token=" + Uri.EscapeDataString(accessToken) +
										"&v=" + vkApiVersion;

				HttpResponseMessage graphResponse = await _httpClient.GetAsync(graphApiEndpoint, Request.CallCancelled);
				graphResponse.EnsureSuccessStatusCode();
				text = await graphResponse.Content.ReadAsStringAsync();
				JObject data = JObject.Parse(text);
				var user = (JObject)data["response"].First;

				var context = new VkAuthenticatedContext(Context, user, accessToken);
				context.Identity = new ClaimsIdentity(
					Options.AuthenticationType,
					ClaimsIdentity.DefaultNameClaimType,
					ClaimsIdentity.DefaultRoleClaimType);
				if (!string.IsNullOrEmpty(context.Id))
				{
					context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.Id, XmlSchemaString,
						Options.AuthenticationType));
				}
				if (!string.IsNullOrEmpty(context.UserName))
				{
					context.Identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, context.UserName, XmlSchemaString,
						Options.AuthenticationType));
				}
				if (!string.IsNullOrEmpty(context.FirstName))
					context.Identity.AddClaim(new Claim(ClaimTypes.GivenName, context.FirstName));
				if (!string.IsNullOrEmpty(context.LastName))
					context.Identity.AddClaim(new Claim(ClaimTypes.Surname, context.LastName));
				if (!string.IsNullOrEmpty(context.AvatarUrl))
					context.Identity.AddClaim(new Claim("AvatarUrl", context.AvatarUrl));
				context.Identity.AddClaim(new Claim(ClaimTypes.Gender, context.Sex.ToString()));
				context.Properties = properties;

				await Options.Provider.Authenticated(context);

				return new AuthenticationTicket(context.Identity, context.Properties);
			}
			catch (Exception ex)
			{
				_logger.WriteError(ex.Message);
			}
			return new AuthenticationTicket(null, properties);
		}

		protected override Task ApplyResponseChallengeAsync()
		{
			_logger.WriteVerbose("ApplyResponseChallenge");

			if (Response.StatusCode != 401)
			{
				return Task.FromResult<object>(null);
			}

			AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType,
				Options.AuthenticationMode);

			if (challenge != null)
			{
				string requestPrefix = Request.GetRealRequestScheme() + "://" + Request.Host;

				QueryString currentQueryString = Request.QueryString;
				string currentUri = !currentQueryString.HasValue
					? requestPrefix + Request.PathBase + Request.Path
					: requestPrefix + Request.PathBase + Request.Path + "?" + currentQueryString;

				string redirectUri = requestPrefix + Request.PathBase + Options.ReturnEndpointPath;

				AuthenticationProperties properties = challenge.Properties;
				if (string.IsNullOrEmpty(properties.RedirectUri))
				{
					properties.RedirectUri = currentUri;
				}

				// OAuth2 10.12 CSRF
				GenerateCorrelationId(properties);

				// comma separated
				string scope = string.Join(",", Options.Scope);

				string state = Options.StateDataFormat.Protect(properties);

				string authorizationEndpoint =
					"https://oauth.vk.com/authorize" +
					"?client_id=" + Uri.EscapeDataString(Options.AppId ?? string.Empty) +
					"&scope=" + Uri.EscapeDataString(scope) +
					"&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
					"&response_type=code" +
					"&state=" + Uri.EscapeDataString(state);

				Response.Redirect(authorizationEndpoint);
			}

			return Task.FromResult<object>(null);
		}

		public override async Task<bool> InvokeAsync()
		{
			return await InvokeReplyPathAsync();
		}

		private async Task<bool> InvokeReplyPathAsync()
		{
			_logger.WriteVerbose("InvokeReplyPath");

			if (Options.ReturnEndpointPath != null &&
				String.Equals(Options.ReturnEndpointPath, Request.Path.Value, StringComparison.OrdinalIgnoreCase))
			{
				// TODO: error responses

				var ticket = await AuthenticateAsync();
				ticket.Properties.IsPersistent = true;

				var context = new VkReturnEndpointContext(Context, ticket);
				context.SignInAsAuthenticationType = Options.SignInAsAuthenticationType;
				context.RedirectUri = ticket.Properties.RedirectUri;

				await Options.Provider.ReturnEndpoint(context);

				if (context.SignInAsAuthenticationType != null &&
					context.Identity != null)
				{
					ClaimsIdentity grantIdentity = context.Identity;
					if (!string.Equals(grantIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
					{
						grantIdentity = new ClaimsIdentity(grantIdentity.Claims, context.SignInAsAuthenticationType,
							grantIdentity.NameClaimType, grantIdentity.RoleClaimType);
					}
					Context.Authentication.SignIn(context.Properties, grantIdentity);
				}

				if (!context.IsRequestCompleted && context.RedirectUri != null)
				{
					string redirectUri = context.RedirectUri;
					Response.Redirect(redirectUri);
					context.RequestCompleted();
				}

				return context.IsRequestCompleted;
			}
			return false;
		}
	}
}