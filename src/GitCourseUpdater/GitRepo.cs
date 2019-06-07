using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using JetBrains.Annotations;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using LibGit2Sharp.Ssh;
using Newtonsoft.Json;
using Serilog;
using Ulearn.Common.Extensions;

namespace GitCourseUpdater
{
	public interface IGitRepo : IDisposable
	{
		MemoryStream GetCurrentStateAsZip(string courseSubdirectoryInRepo = null);
		CommitInfo GetCurrentCommitInfo();
		CommitInfo GetCommitInfo(string hash);
		IEnumerable<string> GetChangedFiles(string fromHash, string toHash, string courseSubdirectoryInRepo = null);
		void Checkout(string commitHashOrBranchName);
	}

	public class GitException : Exception
	{
		public bool MayBeSSHException;
		
		public GitException(string massage, Exception innerException)
			: base(massage, innerException)
		{ }
	}
	
	public class GitRepo : IGitRepo
	{
		private string url;
		private CredentialsHandler credentialsHandler;
		private DirectoryInfo reposBaseDir;
		private string repoDirName; 
		private Repository repo;
		private ILogger logger;
		private string privateKeyPath;
		private string publicKeyPath;
		private static object @lock = new object(); // Потокобезопасность не гарантируется библиотекой libgit2

		// url example git@github.com:user/myrepo.git
		public GitRepo(string url, DirectoryInfo reposBaseDir, string publicKey, string privateKey, DirectoryInfo keysTempDirectory, ILogger logger)
		{
			this.url = url;
			this.reposBaseDir = reposBaseDir;
			this.logger = logger;
			if (!reposBaseDir.Exists)
				reposBaseDir.Create();

			Monitor.Enter(@lock);

			var filenameGuid = Guid.NewGuid().ToString();
			privateKeyPath = Path.Combine(keysTempDirectory.FullName, filenameGuid);
			publicKeyPath = privateKeyPath + ".pub";
			File.WriteAllText(privateKeyPath, privateKey);
			File.WriteAllText(publicKeyPath, publicKey);
			credentialsHandler = (_, __, ___) => new SshUserKeyCredentials
			{
				Username = "git",
				Passphrase = "",
				PublicKey = publicKeyPath,
				PrivateKey = privateKeyPath,
			};

			try
			{
				if (!TryUpdateExistingRepo())
					Clone();
			}
			catch (LibGit2SharpException ex)
			{
				Dispose(); // Если объект не создан, извне Dispose не вызовут
				if(ex.Message.Contains("SSH"))
					throw new GitException(ex.Message, ex) {MayBeSSHException = true};
				throw;
			}
			catch(Exception ex)
			{
				Dispose(); // Если объект не создан, извне Dispose не вызовут
				throw;
			}
		}
		
		public MemoryStream GetCurrentStateAsZip(string courseSubdirectoryInRepo = null)
		{
			logger.Information($"Start load '{repoDirName}' to zip");
			var dir = reposBaseDir.GetSubdirectory(repoDirName).FullName;
			dir = courseSubdirectoryInRepo == null ? dir : Path.Combine(dir, courseSubdirectoryInRepo);
			var zip = ZipHelper.CreateFromDirectory(dir, CompressionLevel.Optimal, false, Encoding.UTF8,
				s => !s.StartsWith(".git"));
			logger.Information($"Successfully load '{repoDirName}' to zip");
			return zip;
		}
		
		public CommitInfo GetCurrentCommitInfo()
		{
			var lastCommit = repo.Commits.First();
			var commitInfo = ToCommitInfo(lastCommit);
			logger.Information($"GetCurrentCommitInfo '{repoDirName}': {JsonConvert.SerializeObject(commitInfo)}");
			return commitInfo;
		}
		
		[CanBeNull]
		public CommitInfo GetCommitInfo(string hash)
		{
			var commit = repo.Lookup<Commit>(hash);
			if (commit == null)
			{
				logger.Warning($"Commit not found repo '{repoDirName}' hash '{hash}'");
				return null;
			}

			return ToCommitInfo(commit);
		}
		

		// null, если коммит не найден. Возвращает полные пути в репозитории
		[CanBeNull]
		public IEnumerable<string> GetChangedFiles(string fromHash, string toHash, string courseSubdirectoryInRepo = null)
		{
			var commitFrom = repo.Lookup<Commit>(fromHash);
			var commitTo = repo.Lookup<Commit>(toHash);
			if (commitFrom == null || commitTo == null)
				return null;
			var treeChanges = repo.Diff.Compare<TreeChanges>(commitFrom.Tree, commitTo.Tree);
			var paths = treeChanges.Select(c => c.Path);
			if (courseSubdirectoryInRepo != null)
				paths = paths.Where(p => p.StartsWith(courseSubdirectoryInRepo));
			return paths;
		}

		private static readonly Regex sha1Regex = new Regex("[a-f0-9]{40}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		public void Checkout(string commitHashOrBranchName)
		{
			if (sha1Regex.IsMatch(commitHashOrBranchName))
				CheckoutCommit(commitHashOrBranchName);
			else
				CheckoutBranchAndResetToOrigin(commitHashOrBranchName);
		}
		
		private void CheckoutCommit(string hash)
		{
			logger.Information($"Start checkout '{url}' commit '{hash}' in '{repoDirName}'");
			Commands.Checkout(repo, hash);
			logger.Information($"Successfully checkout '{url}' commit '{hash}' in '{repoDirName}'");
		}

		private void CheckoutBranchAndResetToOrigin(string name)
		{
			name = name.Replace("origin/", "");
			var remoteBranch = repo.Branches[$"origin/{name}"];
			if (remoteBranch == null)
				throw new ArgumentException($"Branch 'origin/{name}' does not exist");
			if (repo.Branches[name] == null)
			{
				var localBranch = repo.CreateBranch(name, remoteBranch.Tip);
				repo.Branches.Update(localBranch, b =>
				{
					b.Remote = "origin";
					b.UpstreamBranch = $"refs/heads/{name}";
				});
			}
			logger.Information($"Start checkout '{url}' branch '{name}' in '{repoDirName}'");
			Commands.Checkout(repo, name);
			logger.Information($"Successfully checkout '{url}' branch '{name}' in '{repoDirName}'");
			repo.Reset(ResetMode.Hard, remoteBranch.Tip);
			logger.Information($"Successfully reset '{url}' branch '{name}' in '{repoDirName}' to commit '{repo.Branches[name].Tip.Sha}'");
		}
		
		private static CommitInfo ToCommitInfo(Commit commit)
		{
			return new CommitInfo
			{
				Hash = commit.Sha,
				Message = commit.Message,
				AuthorName = commit.Author.Name,
				AuthorEmail = commit.Author.Email,
				Time = commit.Author.When,
			};
		}

		// Пытается найти уже склоненную версию репозитория на диске и обновить её из удаленного репозитория
		private bool TryUpdateExistingRepo()
		{
			try
			{
				repoDirName = GetExistingRepoFolderName();
				if (repoDirName == null)
					return false;
				var repoPath = reposBaseDir.GetSubdirectory(repoDirName);
				repo = new Repository(repoPath.FullName);
				if(HasUncommittedChanges())
					throw new LibGit2SharpException($"Has uncommited changes in '{repoDirName}'");
				FetchAll();
				CheckoutBranchAndResetToOrigin("master");
			}
			catch (Exception ex)
			{
				logger.Warning(ex, $"Could not update existing repository '{repoDirName}'");
				return false;
			}
			return true;
		}
		
		private string GetExistingRepoFolderName()
		{
			var names = reposBaseDir.GetDirectories(Url2Name() + "@*").Select(d => d.Name).ToList();
			if (names.Count == 0)
				return null;
			return names.Max();
		}

		// Создает чистую папку и клонирует в неё
		private void Clone()
		{
			repoDirName = Url2Name() + "@" + DateTime.Now.ToSortable();
			var repoPath = reposBaseDir.GetSubdirectory(repoDirName);
			logger.Information($"Start clone '{url}' into '{repoDirName}'");
			Repository.Clone(url, repoPath.FullName, new CloneOptions { CredentialsProvider = credentialsHandler });
			repo = new Repository(repoPath.FullName);
			logger.Information($"Successfully clone '{url}' into '{repoDirName}'");
		}

		private string Url2Name()
		{
			var parts = url.Split(':');
			var name = parts.Last().Substring(0, parts.Last().Length - 4).Replace('/', '_');
			return GetSafeFilename(name);
		}

		[CanBeNull]
		private static string GetSafeFilename(string filename)
		{
			return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
		}

		private void FetchAll()
		{
			var logMessage = "";
			var options = new FetchOptions { CredentialsProvider = credentialsHandler };
			logger.Information($"Start fetch all '{url}' in '{repoDirName}'");
			foreach (var remote in repo.Network.Remotes)
			{
				var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
				Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
			}
			logger.Information($"Successfully fetch all '{url}' in '{repoDirName}'");
		}

		private bool HasUncommittedChanges()
		{
			var status = repo.RetrieveStatus();
			return status.IsDirty;
			
		}

		public void Dispose()
		{
			try
			{
				repo?.Dispose();
				var privateKeyFileInfo = new FileInfo(privateKeyPath);
				var publicKeyFileInfo = new FileInfo(publicKeyPath);
				if(privateKeyFileInfo.Exists)
					privateKeyFileInfo.Delete();
				if(publicKeyFileInfo.Exists)
					publicKeyFileInfo.Delete();
			}
			finally
			{
				Monitor.Exit(@lock);
			}
		}
	}
}