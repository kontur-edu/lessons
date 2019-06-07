using System.Collections.Generic;
using Database;
using Database.Models;
using Ulearn.Common;

namespace uLearn.Web.Models
{
	public class UserListModel
	{
		public List<UserModel> Users { get; set; }
		public Dictionary<string, string> UsersGroups { get; set; }
		public Dictionary<string, string> UsersArchivedGroups { get; set; }
		public bool CanToggleRoles { get; set; }
		public bool ShowDangerEntities { get; set; }
		public bool CanViewAndToggleCourseAccesses { get; set; }
		public bool CanViewAndToogleSystemAccesses { get; set; }
		public bool CanViewProfiles { get; set; }
	}

	public class UserModel
	{
		public UserModel(UserRolesInfo userRoles)
		{
			UserName = userRoles.UserName;
			UserId = userRoles.UserId;
			UserVisibleName = userRoles.UserVisibleName;
			CourseAccesses = new DefaultDictionary<string, Dictionary<CourseAccessType, CourseAccessModel>>();
			SystemAccesses = new Dictionary<SystemAccessType, SystemAccessModel>();
		}

		public string UserId { get; private set; }
		public string UserName { get; private set; }
		public string UserVisibleName { get; private set; }
		public Dictionary<string, ICoursesRolesListModel> CourseRoles { get; set; }
		public DefaultDictionary<string, Dictionary<CourseAccessType, CourseAccessModel>> CourseAccesses { get; set; }
		public Dictionary<SystemAccessType, SystemAccessModel> SystemAccesses { get; set; }
	}

	public interface ICoursesRolesListModel
	{
	}

	public class SingleCourseRolesModel : ICoursesRolesListModel
	{
		public bool HasAccess { get; set; }
		public string ToggleUrl { get; set; }
	}

	public class CourseRoleModel
	{
		public string CourseId { get; set; }
		public string CourseTitle { get; set; }
		public bool HasAccess { get; set; }
		public string ToggleUrl { get; set; }
	}

	public class ManyCourseRolesModel : ICoursesRolesListModel
	{
		public List<CourseRoleModel> CourseRoles { get; set; }
	}


	public abstract class AccessModel
	{
		public bool HasAccess { get; set; }
		public string ToggleUrl { get; set; }
	}

	public class CourseAccessModel : AccessModel
	{
		public string CourseId { get; set; }
	}

	public class SystemAccessModel : AccessModel
	{
	}
}