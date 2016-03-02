using System.Security.Claims;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;

namespace uLearn.Web.Microsoft.Owin.Security.VK.Provider
{
    public class VkAuthenticatedContext : BaseContext
    {
        public VkAuthenticatedContext(IOwinContext context, JObject user, string accessToken)
            : base(context)
        {
            User = user;
            AccessToken = accessToken;

            Id = TryGetValue(user, "uid");
            UserName = (TryGetValue(user, "first_name") + " " + TryGetValue(user, "last_name")).Trim();
            if (string.IsNullOrEmpty(UserName))
            {
                UserName = TryGetValue(user, "screen_name");
            }

            AvatarUrl = TryGetValue(user, "photo_100");
        }

        public JObject User { get; private set; }
        public string AccessToken { get; private set; }

        public string Id { get; private set; }
        public string UserName { get; private set; }

        public string AvatarUrl { get; private set; }

        public ClaimsIdentity Identity { get; set; }
        public AuthenticationProperties Properties { get; set; }

        private static string TryGetValue(JObject user, string propertyName)
        {
            JToken value;
            return user.TryGetValue(propertyName, out value) ? value.ToString() : string.Empty;
        }
    }
}
