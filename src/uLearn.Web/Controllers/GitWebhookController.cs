using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using log4net;
using Newtonsoft.Json;
using Ulearn.Core.Configuration;

namespace uLearn.Web.Controllers
{

	public class GitWebhookController : ApiController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(GitWebhookController));
		
		private readonly string gitSecret;
		
		public GitWebhookController()
		{
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			gitSecret = configuration.Git.Webhook.Secret;
		}
		
		[System.Web.Http.HttpPost]
		[System.Web.Http.Route("CoursesWebhook")]
		public async Task<ActionResult> CoursesWebhook()
		{
			string githubEventName = null;
			if (Request.Headers.TryGetValues("X-GitHub-Event", out var githubEventNames))
				githubEventName = githubEventNames.FirstOrDefault();
			string gitlabEventName = null;
			if (Request.Headers.TryGetValues("X-Gitlab-Event", out var gitlabEventNames))
				gitlabEventName = gitlabEventNames.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(githubEventName))
				return await ProcessGithubRequest(githubEventName).ConfigureAwait(false);
			if (!string.IsNullOrWhiteSpace(gitlabEventName))
				return await ProcessGitlabRequest(gitlabEventName).ConfigureAwait(false);
			log.Warn("Event header not found");
			return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
		}

		public async Task<ActionResult> ProcessGithubRequest(string eventName)
		{
			if(eventName != "push")
				return new HttpStatusCodeResult(HttpStatusCode.OK);
			string signature = null;
			if (Request.Headers.TryGetValues("X-Hub-Signature", out var signatures))
				signature = signatures.FirstOrDefault();
			var jsonContent = await Request.Content.ReadAsStringAsync().ConfigureAwait(false);
			if (!IsValidGithubRequest(jsonContent, eventName, signature))
			{
				log.Warn($"Invalid github request eventName: '{eventName}' signature: '{signature}' jsonContent: {jsonContent}");
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}
			var content = JsonConvert.DeserializeObject<GithubPushData>(jsonContent);
			var branch = content.Ref.Replace("refs/heads/", "");
			log.Info("Json content of webhook request: " + jsonContent);
			var url = content.Repository.SshUrl;
			await UpdateRepo(url, branch).ConfigureAwait(false);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		public async Task<ActionResult> ProcessGitlabRequest(string eventName)
		{
			if(eventName != "Push Hook")
				return new HttpStatusCodeResult(HttpStatusCode.OK);
			string token = null;
			if (Request.Headers.TryGetValues("X-Gitlab-Token", out var signatures))
				token = signatures.FirstOrDefault();
			var jsonContent = await Request.Content.ReadAsStringAsync().ConfigureAwait(false);
			if (token != gitSecret)
			{
				log.Warn($"Invalid gitlab request eventName: '{eventName}' token: '{token}' jsonContent: {jsonContent}");
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}
			var content = JsonConvert.DeserializeObject<GitlabPushData>(jsonContent);
			var branch = content.Ref.Replace("refs/heads/", "");
			log.Info("Json content of webhook request: " + jsonContent);
			var url = content.Repository.SshUrl;
			await UpdateRepo(url, branch).ConfigureAwait(false);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		private async Task UpdateRepo(string url, string branch)
		{
			log.Info($"Git webhook push event url '{url}'");
			var adminController = new AdminController();
			await adminController.UploadCoursesWithGit(url, branch).ConfigureAwait(false);
		}
		
		private bool IsValidGithubRequest(string payload, string eventName, string signatureWithPrefix)
		{
			if (string.IsNullOrWhiteSpace(payload))
			{
				throw new ArgumentNullException(nameof(payload));
			}
			if (string.IsNullOrWhiteSpace(eventName))
			{
				throw new ArgumentNullException(nameof(eventName));
			}
			if (string.IsNullOrWhiteSpace(signatureWithPrefix))
			{
				throw new ArgumentNullException(nameof(signatureWithPrefix));
			}

			const string sha1Prefix = "sha1=";
			if (!signatureWithPrefix.StartsWith(sha1Prefix, StringComparison.OrdinalIgnoreCase))
				return false;
			
			var signature = signatureWithPrefix.Substring(sha1Prefix.Length);
			var secret = Encoding.UTF8.GetBytes(gitSecret);
			var payloadBytes = Encoding.UTF8.GetBytes(payload);

			using (var hmSha1 = new HMACSHA1(secret))
			{
				var hash = hmSha1.ComputeHash(payloadBytes);

				var hashString = ToHexString(hash);

				if (hashString.Equals(signature))
				{
					return true;
				}
			}

			return false;
		}

		private static string ToHexString(byte[] bytes)
		{
			var builder = new StringBuilder(bytes.Length * 2);
			foreach (byte b in bytes)
			{
				builder.AppendFormat("{0:x2}", b);
			}

			return builder.ToString();
		}
	}
	
	[DataContract]
	internal class GithubPushData
	{
		[DataMember(Name = "ref")] public string Ref;
		[DataMember(Name = "repository")] public GithubRepository Repository;
	}

	[DataContract]
	internal class GithubRepository
	{
		[DataMember(Name = "ssh_url")] public string SshUrl;
	}
	
	[DataContract]
	internal class GitlabPushData
	{
		[DataMember(Name = "ref")] public string Ref;
		[DataMember(Name = "repository")] public GitlabRepository Repository;
	}
	
	[DataContract]
	internal class GitlabRepository
	{
		[DataMember(Name = "git_ssh_url")] public string SshUrl;
	}
}