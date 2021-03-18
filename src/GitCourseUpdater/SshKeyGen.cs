using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Ulearn.Common;

namespace GitCourseUpdater
{
	public class RSAKey
	{
		public string PublicPEM { get; set; }
		public string PrivatePEM { get; set; }
		public string PublicSSH { get; set; }
	}

	public static class SshKeyGenerator
	{
		public static RSAKey Generate()
		{
			var result = new RSAKey();
			var rsa = new RSACryptoServiceProvider(2048);
			var rsaKeyPair = DotNetUtilities.GetRsaKeyPair(rsa);
			using (var writer = new StringWriter())
			{
				var pemWriter = new PemWriter(writer);
				pemWriter.WriteObject(rsaKeyPair.Public);
				result.PublicPEM = writer.ToString();
				result.PublicPEM = result.PublicPEM.Replace("\r\n", "\n");
			}

			using (var writer = new StringWriter())
			{
				var pemWriter = new PemWriter(writer);
				pemWriter.WriteObject(rsaKeyPair.Private /*, "DES-EDE3-CBC", passphrase.ToCharArray(), new SecureRandom()*/);
				result.PrivatePEM = writer.ToString();
				result.PrivatePEM = result.PrivatePEM.Replace("\r\n", "\n");
			}

			result.PublicSSH = ConvertToOpenSshPublicKey(rsa);
			return result;
		}

		private static string ConvertToOpenSshPublicKey(RSACryptoServiceProvider rsa)
		{
			var n = rsa.ExportParameters(false).Modulus;
			var e = rsa.ExportParameters(false).Exponent;
			string buffer64;
			var sshrsaBytes = Encoding.Default.GetBytes("ssh-rsa");
			using (var ms = StaticRecyclableMemoryStreamManager.Manager.GetStream())
			{
				ms.Write(ToBytes(sshrsaBytes.Length), 0, 4);
				ms.Write(sshrsaBytes, 0, sshrsaBytes.Length);
				ms.Write(ToBytes(e.Length), 0, 4);
				ms.Write(e, 0, e.Length);
				ms.Write(ToBytes(n.Length + 1), 0, 4); //Remove the +1 if not Emulating Putty Gen
				ms.Write(new byte[] { 0 }, 0, 1); //Add a 0 to Emulate PuttyGen
				ms.Write(n, 0, n.Length);
				ms.Flush();
				ms.Position = 0;
				buffer64 = Convert.ToBase64String(ms.ToArray());
			}

			return $"ssh-rsa {buffer64} generated-key";
		}

		private static byte[] ToBytes(int i)
		{
			var bts = BitConverter.GetBytes(i);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bts);
			}

			return bts;
		}
	}
}