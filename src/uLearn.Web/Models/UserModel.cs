using System.Collections.Generic;

namespace uLearn.Web.Models
{
	public class UserListModel
	{
		public List<UserModel> Users { get; set; }
		public List<string> Courses { get; set; }
	}

	public class UserModel
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string GroupName { get; set; }
		public List<string> Roles { get; set; }
		public Dictionary<CourseRoles, List<string>> Courses { get; set; }
	}
}