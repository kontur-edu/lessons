﻿using System.Runtime.Serialization;
using Ulearn.Core.Configuration;

/* Move it to Web.Api.Configuration after disabling Ulearn.Web. Now it's here because Ulearn.Web should know about CookieKeyRingDirectory */

// ReSharper disable once CheckNamespace
namespace Web.Api.Configuration
{
	public class WebApiConfiguration : UlearnConfiguration
	{
		public UlearnWebConfiguration Web { get; set; }
		
		public FrontendConfiguration Frontend { get; set; } 
	}

	public class UlearnWebConfiguration
	{
		public string CookieKeyRingDirectory { get; set; }
		public string CookieName { get; set; }
		public string CookieDomain { get; set; }
		public bool CookieSecure { get; set; }
		
		public CorsConfiguration Cors { get; set; }
		
		public AuthenticationConfiguration Authentication { get; set; }
	}

	public class CorsConfiguration
	{
		public string[] AllowOrigins { get; set; }
	}

	public class AuthenticationConfiguration
	{
		public JwtConfiguration Jwt { get; set; }
	}

	public class JwtConfiguration
	{
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public string IssuerSigningKey { get; set; }
		public int LifeTimeHours { get; set; }
	}


	/*
	  DataContract and DataMembers is needed only for FrontendConfiguration, because backend
	  should serializer JSON with this config for frontend's index.html
	 */
	[DataContract]
	public class FrontendConfiguration
	{
		[DataMember(Name = "api")]
		public ApiConfiguration Api { get; set; }
	} 
	
	[DataContract]
	public class ApiConfiguration
	{
		[DataMember(Name = "endpoint")]
		public string Endpoint { get; set; }	
	}
}