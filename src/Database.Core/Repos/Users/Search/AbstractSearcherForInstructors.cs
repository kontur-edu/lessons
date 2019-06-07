using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.CourseRoles;
using NinjaNye.SearchExtensions;

namespace Database.Repos.Users.Search
{
	public abstract class AbstractSearcherForInstructors : ISearcher
	{
		protected readonly IUsersRepo usersRepo;
		protected readonly ICourseRolesRepo courseRolesRepo;
		protected readonly IAccessRestrictor accessRestrictor;
		
		private readonly bool hasSystemAdministratorAccess;
		private readonly bool hasCourseAdminAccess;
		private readonly bool hasInstructorAccessToGroupMembers;
		private readonly bool hasInstructorAccessToCourseInstructors;
		private readonly SearchField searchField;

		private readonly Expression<Func<ApplicationUser, string>>[] userProperties;

		public AbstractSearcherForInstructors(
			IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor,
			bool hasSystemAdministratorAccess, bool hasCourseAdminAccess, bool hasInstructorAccessToGroupMembers, bool hasInstructorAccessToCourseInstructors,
			SearchField searchField,
			params Expression<Func<ApplicationUser, string>>[] userProperties
		)
		{
			if (userProperties.Length == 0)
				throw new ArgumentException("UserProperties should be specified", nameof(userProperties));
			
			this.usersRepo = usersRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.accessRestrictor = accessRestrictor;
			
			this.hasSystemAdministratorAccess = hasSystemAdministratorAccess;
			this.hasCourseAdminAccess = hasCourseAdminAccess;
			this.hasInstructorAccessToGroupMembers = hasInstructorAccessToGroupMembers;
			this.hasInstructorAccessToCourseInstructors = hasInstructorAccessToCourseInstructors;
			
			this.searchField = searchField;
			this.userProperties = userProperties;
		}

		public SearchField GetSearchField()
		{
			return searchField;
		}

		public virtual Task<IQueryable<ApplicationUser>> GetSearchScopeAsync(IQueryable<ApplicationUser> users, ApplicationUser currentUser, string courseId)
		{
			return accessRestrictor.RestrictUsersSetAsync(
				users, currentUser, courseId,
				hasSystemAdministratorAccess,
				hasCourseAdminAccess,
				hasInstructorAccessToGroupMembers,
				hasInstructorAccessToCourseInstructors
			);
		}

		public virtual Task<bool> IsAvailableForSearchAsync(ApplicationUser currentUser)
		{
			if (usersRepo.IsSystemAdministrator(currentUser))
				return Task.FromResult(true);
			
			return courseRolesRepo.HasUserAccessToAnyCourseAsync(currentUser.Id, CourseRoleType.Instructor);
		}

#pragma warning disable 1998
		public virtual async Task<IQueryable<ApplicationUser>> SearchAsync(IQueryable<ApplicationUser> users, string term, bool strict = false, int limit = 1000)
#pragma warning restore 1998
		{
			if (string.IsNullOrEmpty(term))
				return Enumerable.Empty<ApplicationUser>().AsQueryable();

			if (strict)
				return users.Search(userProperties).EqualTo(term);

			return users.Search(userProperties).StartsWith(term).OrderBy(u => u.Id).Take(limit);
		}
	}
}