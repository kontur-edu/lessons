using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;
using Ulearn.Core;

namespace uLearn.CourseTool
{
	public class CredentialsArray
	{
		public Credentials[] Credentials;
	}

	public class Credentials
	{
		[XmlAttribute("profile")]
		public string Profile;

		[XmlAttribute("service")]
		public string Service;

		public string Email;
		public string Password;

		public Credentials()
		{
		}

		public Credentials(string profile, string service, string email, string password)
		{
			Profile = profile;
			Service = service;
			Email = email;
			SetPassword(password);
		}

		private static Credentials GetCredentialsFromIo(string profile, string service)
		{
			Console.WriteLine("Enter email:");
			var email = Console.ReadLine();
			Console.WriteLine("Enter password:");
			var password = Utils.GetPass();
			return new Credentials(profile, service, email, password);
		}

		public static Credentials GetCredentials(string dir, string profile, string service = null)
		{
			Credentials credentials;
			if (File.Exists(dir + "/credentials.xml"))
			{
				var cred = new FileInfo(dir + "/credentials.xml").DeserializeXml<CredentialsArray>();
				if (cred.Credentials.Any(x => x.Profile == profile && x.Service == service))
					credentials = cred.Credentials.First(x => x.Profile == profile);
				else
				{
					credentials = GetCredentialsFromIo(profile, service);
					cred.Credentials = new List<Credentials>(cred.Credentials) { credentials }.ToArray();
					File.WriteAllText(dir + "/credentials.xml", cred.XmlSerialize());
				}
			}
			else
			{
				credentials = GetCredentialsFromIo(profile, service);
				File.WriteAllText(dir + "/credentials.xml", new CredentialsArray { Credentials = new[] { credentials } }.XmlSerialize());
			}
			return credentials;
		}

		public string GetPassword()
		{
			return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Password), null, DataProtectionScope.CurrentUser));
		}

		public void SetPassword(string password)
		{
			Password = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser));
		}
	}
}