using Database.Repos.CourseRoles;

namespace Database.Repos.Users.Search
{
	public class SearcherByLogin : AbstractSearcherForInstructors
	{
		public SearcherByLogin(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor)
			:base(usersRepo, courseRolesRepo, accessRestrictor,
				true, true, true, true,
				SearchField.Login,
				u => u.UserName
			)
		{
		}
	}
}