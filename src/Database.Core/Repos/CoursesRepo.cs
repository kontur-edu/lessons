using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Extensions;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class CoursesRepo : ICoursesRepo
	{
		private readonly UlearnDb db;

		public CoursesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public Task<List<string>> GetPublishedCourseIdsAsync()
		{
			return db.CourseVersions
				.Select(v => v.CourseId)
				.Distinct()
				.ToListAsync();
		}

		public Task<CourseVersion> GetPublishedCourseVersionAsync(string courseId)
		{
			return db.CourseVersions.AsNoTracking()
				.Where(v => v.CourseId == courseId && v.PublishTime != null)
				.OrderByDescending(v => v.PublishTime)
				.FirstOrDefaultAsync();
		}

		public Task<List<CourseVersion>> GetCourseVersionsAsync(string courseId)
		{
			return db.CourseVersions
				.Where(v => v.CourseId == courseId)
				.OrderByDescending(v => v.LoadingTime)
				.ToListAsync();
		}

		public async Task<CourseVersion> AddCourseVersionAsync(string courseId, Guid versionId, string authorId)
		{
			var courseVersion = new CourseVersion
			{
				Id = versionId,
				CourseId = courseId,
				LoadingTime = DateTime.Now,
				PublishTime = null,
				AuthorId = authorId,
			};
			db.CourseVersions.Add(courseVersion);
			await db.SaveChangesAsync().ConfigureAwait(false);

			return courseVersion;
		}

		public async Task MarkCourseVersionAsPublishedAsync(Guid versionId)
		{
			var courseVersion = await db.CourseVersions.FindAsync(versionId).ConfigureAwait(false);
			if (courseVersion == null)
				return;

			courseVersion.PublishTime = DateTime.Now;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task DeleteCourseVersionAsync(string courseId, Guid versionId)
		{
			var courseVersion = await db.CourseVersions.FindAsync(versionId).ConfigureAwait(false);
			if (courseVersion == null)
				return;

			if (!courseId.EqualsIgnoreCase(courseVersion.CourseId))
				return;

			db.CourseVersions.Remove(courseVersion);
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task<List<CourseVersion>> GetPublishedCourseVersionsAsync()
		{
			var courseVersions = await db.CourseVersions.AsNoTracking().ToListAsync().ConfigureAwait(false);
			return courseVersions
				.GroupBy(v => v.CourseId.ToLower())
				.Select(g => g.MaxBy(v => v.PublishTime))
				.ToList();
		}

		/* Course accesses */
		public async Task<CourseAccess> GrantAccessAsync(string courseId, string userId, CourseAccessType accessType, string grantedById)
		{
			var currentAccess = await db.CourseAccesses.FirstOrDefaultAsync(a => a.CourseId == courseId && a.UserId == userId && a.AccessType == accessType).ConfigureAwait(false);
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

			await db.SaveChangesAsync().ConfigureAwait(false);
			return db.CourseAccesses.Include(a => a.GrantedBy).Single(a => a.Id == currentAccess.Id);
		}

		public bool CanRevokeAccess(string courseId, string userId, IPrincipal revokedBy)
		{
			return revokedBy.HasAccessFor(courseId, CourseRoleType.CourseAdmin);
		}

		public async Task<List<CourseAccess>> RevokeAccessAsync(string courseId, string userId, CourseAccessType accessType)
		{
			var accesses = await db.CourseAccesses
				.Where(a => a.CourseId == courseId && a.UserId == userId && a.AccessType == accessType)
				.ToListAsync()
				.ConfigureAwait(false);
			foreach (var access in accesses)
				access.IsEnabled = false;

			await db.SaveChangesAsync().ConfigureAwait(false);
			return accesses;
		}

		public Task<List<CourseAccess>> GetCourseAccessesAsync(string courseId)
		{
			return db.CourseAccesses.Include(a => a.User).Where(a => a.CourseId == courseId && a.IsEnabled).ToListAsync();
		}

		public Task<List<CourseAccess>> GetCourseAccessesAsync(string courseId, string userId)
		{
			return db.CourseAccesses.Include(a => a.User).Where(a => a.CourseId == courseId && a.UserId == userId && a.IsEnabled).ToListAsync();
		}

		public async Task<DefaultDictionary<string, List<CourseAccess>>> GetCoursesAccessesAsync(IEnumerable<string> coursesIds)
		{
			var courseAccesses = await db.CourseAccesses.Include(a => a.User)
				.Where(a => coursesIds.Contains(a.CourseId) && a.IsEnabled)
				.GroupBy(a => a.CourseId)
				.ToDictionaryAsync(g => g.Key, g => g.ToList())
				.ConfigureAwait(false);
			return courseAccesses.ToDefaultDictionary();
		}

		public Task<bool> HasCourseAccessAsync(string userId, string courseId, CourseAccessType accessType)
		{
			return db.CourseAccesses.AnyAsync(a => a.CourseId == courseId && a.UserId == userId && a.AccessType == accessType && a.IsEnabled);
		}

		public Task<List<CourseAccess>> GetUserAccessesAsync(string userId)
		{
			return db.CourseAccesses.Where(a => a.UserId == userId && a.IsEnabled).ToListAsync();
		}

		public Task<List<string>> GetCoursesUserHasAccessTo(string userId, CourseAccessType accessType)
		{
			return db.CourseAccesses.Where(a => a.UserId == userId && a.IsEnabled && a.AccessType == accessType).Select(a => a.CourseId).Distinct().ToListAsync();
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

		public Task<CourseFile> GetCourseFileAsync(string courseId)
		{
			return db.CourseFiles.FirstOrDefaultAsync(f => f.CourseId.Equals(courseId, StringComparison.OrdinalIgnoreCase));
		}
		
		public Task<List<CourseFile>> GetCourseFilesAsync(IEnumerable<string> exceptCourseIds)
		{
			return db.CourseFiles.Where(a => !exceptCourseIds.Contains(a.CourseId)).ToListAsync();
		}
	}
}