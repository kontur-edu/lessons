using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Database.Repos.Users
{
	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum SearchField
	{
		UserId,
		Login,
		Name,
		Email,
		SocialLogin,
	}
}