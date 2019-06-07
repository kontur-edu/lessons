using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.CourseRoles;

namespace Database.Repos.Users.Search
{
	public class SearcherBySocialLogin : ISearcher
	{
		private readonly IUsersRepo usersRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IAccessRestrictor accessRestrictor;

		public SearcherBySocialLogin(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor)
		{
			this.usersRepo = usersRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.accessRestrictor = accessRestrictor;
		}

		public SearchField GetSearchField()
		{
			return SearchField.SocialLogin;
		}

		public Task<IQueryable<ApplicationUser>> GetSearchScopeAsync(IQueryable<ApplicationUser> users, ApplicationUser currentUser, string courseId)
		{
			return accessRestrictor.RestrictUsersSetAsync(
				users, currentUser, courseId,
				hasSystemAdministratorAccess: true, 
				hasCourseAdminAccess: false,
				hasInstructorAccessToGroupMembers: false,
				hasInstructorAccessToCourseInstructors: false
			);
		}

		public Task<bool> IsAvailableForSearchAsync(ApplicationUser currentUser)
		{
			return Task.FromResult(usersRepo.IsSystemAdministrator(currentUser));
		}

		public async Task<IQueryable<ApplicationUser>> SearchAsync(IQueryable<ApplicationUser> users, string term, bool strict = false, int limit = 1000)
		{
			if (string.IsNullOrEmpty(term))
				return Enumerable.Empty<ApplicationUser>().AsQueryable();

			/* This searcher works identically in strict and non-strict mode */
			var userIds = await usersRepo.FindUsersBySocialProviderKeyAsync(term).ConfigureAwait(false);
			return users.Where(u => userIds.Contains(u.Id));
		}
	}
}