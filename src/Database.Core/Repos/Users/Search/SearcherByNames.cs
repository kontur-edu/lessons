using Database.Repos.CourseRoles;

namespace Database.Repos.Users.Search
{
	public class SearcherByNames : AbstractSearcherForInstructors
	{
		public SearcherByNames(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IAccessRestrictor accessRestrictor)
			:base(usersRepo, courseRolesRepo, accessRestrictor,
				true, true, true, true,
				SearchField.Name,
				u => u.FirstName, u => u.LastName
			)
		{
		}
	}
}