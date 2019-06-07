using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.DataContexts
{
	public class CoursesRepo
	{
		private readonly ULearnDb db;

		public CoursesRepo()
			: this(new ULearnDb())
		{
		}

		public CoursesRepo(ULearnDb db)
		{
			this.db = db;
		}

		public CourseVersion GetPublishedCourseVersion(string courseId)
		{
			return db.CourseVersions.Where(v => v.CourseId == courseId && v.PublishTime != null).OrderByDescending(v => v.PublishTime).FirstOrDefault();
		}

		public IEnumerable<CourseVersion> GetCourseVersions(string courseId)
		{
			return db.CourseVersions.Where(v => v.CourseId == courseId).OrderByDescending(v => v.LoadingTime);
		}

		public async Task<CourseVersion> AddCourseVersion(string courseId, Guid versionId, string authorId,
			string pathToCourseXml, string repoUrl, string commitHash, string description)
		{
			var courseVersion = new CourseVersion
			{
				Id = versionId,
				CourseId = courseId,
				LoadingTime = DateTime.Now,
				PublishTime = null,
				AuthorId = authorId,
				PathToCourseXml = pathToCourseXml,
				CommitHash = commitHash,
				Description = description,
				RepoUrl = repoUrl
			};
			db.CourseVersions.Add(courseVersion);
			await db.SaveChangesAsync();

			return courseVersion;
		}

		public async Task MarkCourseVersionAsPublished(Guid versionId)
		{
			var courseVersion = db.CourseVersions.Find(versionId);
			if (courseVersion == null)
				return;

			courseVersion.PublishTime = DateTime.Now;
			await db.SaveChangesAsync();
		}

		public async Task DeleteCourseVersion(string courseId, Guid versionId)
		{
			var courseVersion = db.CourseVersions.Find(versionId);
			if (courseVersion == null)
				return;

			if (!string.Equals(courseVersion.CourseId, courseId, StringComparison.OrdinalIgnoreCase))
				return;

			db.CourseVersions.Remove(courseVersion);
			await db.SaveChangesAsync();
		}

		/* Course accesses */

		public async Task<CourseAccess> GrantAccess(string courseId, string userId, CourseAccessType accessType, string grantedById)
		{
			var currentAccess = db.CourseAccesses.FirstOrDefault(a => a.CourseId == courseId && a.UserId == userId && a.AccessType == accessType);
			if (currentAccess == null)
			{
				currentAccess = new CourseAccess
				{
					CourseId = courseId,
					UserId = userId,
					AccessType = accessType,
				};
				db.CourseAccesses.Add(currentAccess);
			}
			currentAccess.GrantedById = grantedById;
			currentAccess.GrantTime = DateTime.Now;
			currentAccess.IsEnabled = true;

			await db.SaveChangesAsync();
			return db.CourseAccesses.Include(a => a.GrantedBy).Single(a => a.Id == currentAccess.Id);
		}

		public bool CanRevokeAccess(string courseId, string userId, IPrincipal revokedBy)
		{
			return revokedBy.HasAccessFor(courseId, CourseRole.CourseAdmin);
		}

		public async Task<List<CourseAccess>> RevokeAccess(string courseId, string userId, CourseAccessType accessType)
		{
			var accesses = db.CourseAccesses.Where(a => a.CourseId == courseId && a.UserId == userId && a.AccessType == accessType).ToList();
			foreach (var access in accesses)
				access.IsEnabled = false;

			await db.SaveChangesAsync();
			return accesses;
		}

		public List<CourseAccess> GetCourseAccesses(string courseId)
		{
			return db.CourseAccesses.Include(a => a.User).Where(a => a.CourseId == courseId && a.IsEnabled).ToList();
		}

		public List<CourseAccess> GetCourseAccesses(string courseId, string userId)
		{
			return db.CourseAccesses.Include(a => a.User).Where(a => a.CourseId == courseId && a.UserId == userId && a.IsEnabled).ToList();
		}

		public DefaultDictionary<string, List<CourseAccess>> GetCoursesAccesses(IEnumerable<string> coursesIds)
		{
			return db.CourseAccesses.Include(a => a.User)
				.Where(a => coursesIds.Contains(a.CourseId) && a.IsEnabled)
				.GroupBy(a => a.CourseId)
				.ToDictionary(g => g.Key, g => g.ToList())
				.ToDefaultDictionary();
		}

		public bool HasCourseAccess(string userId, string courseId, CourseAccessType accessType)
		{
			return db.CourseAccesses.Any(a => a.CourseId == courseId && a.UserId == userId && a.AccessType == accessType && a.IsEnabled);
		}
		
		// Add new and remove old course file
		public async Task AddCourseFile(string courseId, Guid versionId, byte[] content)
		{
			var file = new CourseFile
			{
				CourseId = courseId,
				CourseVersionId = versionId,
				File = content
			};
			db.CourseFiles.RemoveRange(db.CourseFiles.Where(f => f.CourseId.Equals(courseId, StringComparison.OrdinalIgnoreCase)));
			db.CourseFiles.Add(file);
			await db.SaveChangesAsync();
		}

		[CanBeNull]
		public CourseFile GetCourseFile(string courseId)
		{
			return db.CourseFiles.FirstOrDefault(f => f.CourseId.Equals(courseId, StringComparison.OrdinalIgnoreCase));
		}
		
		public List<CourseFile> GetCourseFiles(IEnumerable<string> existingOnDiskCourseIds)
		{
			return db.CourseFiles.Where(a => !existingOnDiskCourseIds.Contains(a.CourseId)).ToList();
		}

		[CanBeNull]
		public CourseGit GetCourseRepoSettings(string courseId)
		{
			var data = db.CourseGitRepos.Where(v => v.CourseId == courseId).OrderByDescending(v => v.CreateTime).FirstOrDefault();
			if (data?.RepoUrl == null)
				return null;
			return data;
		}
		
		public async Task SetCourseRepoSettings(CourseGit courseGit)
		{
			courseGit.CreateTime = DateTime.Now;
			db.CourseGitRepos.Add(courseGit);
			await db.SaveChangesAsync();
		}

		public async Task RemoveCourseRepoSettings(string courseId)
		{
			var courseGit = new CourseGit { CourseId = courseId };
			await SetCourseRepoSettings(courseGit).ConfigureAwait(false);
		}

		public List<CourseGit> FindCoursesByRepoUrl(string repoUrl)
		{
			return db.CourseGitRepos.GroupBy(r => r.CourseId).Select(g => g.OrderByDescending(r => r.CreateTime).FirstOrDefault()).Where(r => r.RepoUrl == repoUrl).ToList();
		}

		public async Task UpdateKeysByRepoUrl(string repoUrl, string publicKey, string privateKey)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				var repos = FindCoursesByRepoUrl(repoUrl);
				foreach (var repo in repos)
				{
					repo.PublicKey = publicKey;
					repo.PrivateKey = privateKey;
					await SetCourseRepoSettings(repo).ConfigureAwait(false);
				}
				transaction.Commit();
			}
		}
	}
}