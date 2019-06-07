using Database.Repos.CourseRoles;

namespace Database.Repos.Users.Search
{
	public class SearcherByEmail : AbstractSearcherForInstructors
	{
		public SearcherByEmail(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor)
			:base(usersRepo, courseRolesRepo, accessRestrictor,
				true, true, true, false,
				SearchField.Email,
				u => u.Email
			)
		{
		}
	}
}