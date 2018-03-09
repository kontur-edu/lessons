using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;
using Ulearn.Common;

namespace uLearn.Web.Microsoft.Owin.Security.VK.Provider
{
	public class VkAuthenticatedContext : BaseContext
	{
		private readonly Regex VkIdRegex = new Regex(@"^id\d+$", RegexOptions.Compiled);

		public VkAuthenticatedContext(IOwinContext context, JObject user, string accessToken)
			: base(context)
		{
			User = user;
			AccessToken = accessToken;

			Id = TryGetValue(user, "id");
			FirstName = TryGetValue(user, "first_name");
			LastName = TryGetValue(user, "last_name");

			var screenName = TryGetValue(user, "screen_name");
			UserName = screenName;
			/* By default username is screenName but in case of idXXXXX we will use string "FirstName LastName" */
			if (VkIdRegex.IsMatch(screenName) && (FirstName + LastName).Trim() != "")
				UserName = (FirstName + " " + LastName).Trim();

			AvatarUrl = TryGetValue(user, "photo_100");
			var sex = TryGetValue(user, "sex");
			if (sex == "1")
				Sex = Gender.Female;
			else if (sex == "2")
				Sex = Gender.Male;
		}

		public JObject User { get; private set; }
		public string AccessToken { get; private set; }

		public string Id { get; private set; }
		public string UserName { get; private set; }

		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string AvatarUrl { get; private set; }
		public Gender? Sex { get; private set; }

		public ClaimsIdentity Identity { get; set; }
		public AuthenticationProperties Properties { get; set; }

		private static string TryGetValue(JObject user, string propertyName)
		{
			JToken value;
			return user.TryGetValue(propertyName, out value) ? value.ToString() : string.Empty;
		}
	}
}