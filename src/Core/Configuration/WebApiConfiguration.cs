using uLearn.Configuration;

/* Move it to Web.Api.Configuration after disabling Ulearn.Web. Not it's here because Ulearn.Web should know about CookieKeyRingDirectory */

namespace Web.Api.Configuration
{
	public class WebApiConfiguration : UlearnConfiguration
	{
		public UlearnWebConfiguration Web { get; set; }
	}

	public class UlearnWebConfiguration
	{
		public string CookieKeyRingDirectory { get; set; }
	}
}