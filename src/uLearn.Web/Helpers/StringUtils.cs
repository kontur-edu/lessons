using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace uLearn.Web.Helpers
{
	public class StringUtils
	{
		public static Random random;
		private const string alphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		public static string GenerateAlphanumericString(int length)
		{
			return new string(
				Enumerable.Repeat(alphanumericChars, length).Select(s => s[random.Next(s.Length)]).ToArray()
				);
		}

		public static string GenerateSecureAlphanumericString(int length)
		{
			var chars = alphanumericChars.ToCharArray();
			var randomBytes = new byte[length];
			using (var crypto = new RNGCryptoServiceProvider())
				crypto.GetNonZeroBytes(randomBytes);

			var result = new StringBuilder(length);
			foreach (var randomByte in randomBytes)
				result.Append(chars[randomByte % chars.Length]);
			return result.ToString();
		}
	}
}