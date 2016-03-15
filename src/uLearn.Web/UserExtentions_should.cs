using System.Collections.Generic;
using System.Security.Claims;
using NUnit.Framework;
using uLearn.Web.Models;

namespace uLearn.Web
{
	[TestFixture]
	public class UserExtentions_should
	{
		[Test]
		public void HasAccessFor()
		{
			var user = GetUser();

			Assert.That(user.HasAccessFor("courseAdmin", CourseRole.CourseAdmin));
			Assert.That(user.HasAccessFor("courseAdmin", CourseRole.Instructor));
			Assert.That(user.HasAccessFor("courseAdmin", CourseRole.Tester));

			Assert.That(!user.HasAccessFor("instructor", CourseRole.CourseAdmin));
			Assert.That(user.HasAccessFor("instructor", CourseRole.Instructor));
			Assert.That(user.HasAccessFor("instructor", CourseRole.Tester));

			Assert.That(!user.HasAccessFor("tester", CourseRole.CourseAdmin));
			Assert.That(!user.HasAccessFor("tester", CourseRole.Instructor));
			Assert.That(user.HasAccessFor("tester", CourseRole.Tester));

			Assert.That(!user.HasAccessFor("sysAdmin", CourseRole.CourseAdmin));
			Assert.That(!user.HasAccessFor("sysAdmin", CourseRole.Instructor));
			Assert.That(!user.HasAccessFor("sysAdmin", CourseRole.Tester));

			Assert.That(!user.IsSystemAdministrator());
		}

		[Test]
		public void IsBestAccessFor()
		{
			var user = GetUser();

			Assert.That(user.IsBestAccessFor(CourseRole.CourseAdmin, "courseAdmin"));
			Assert.That(!user.IsBestAccessFor(CourseRole.Instructor, "courseAdmin"));
			Assert.That(!user.IsBestAccessFor(CourseRole.Tester, "courseAdmin"));

			Assert.That(!user.IsBestAccessFor(CourseRole.CourseAdmin, "instructor"));
			Assert.That(user.IsBestAccessFor(CourseRole.Instructor, "instructor"));
			Assert.That(!user.IsBestAccessFor(CourseRole.Tester, "instructor"));

			Assert.That(!user.IsBestAccessFor(CourseRole.CourseAdmin, "tester"));
			Assert.That(!user.IsBestAccessFor(CourseRole.Instructor, "tester"));
			Assert.That(user.IsBestAccessFor(CourseRole.Tester, "tester"));

			Assert.That(!user.IsBestAccessFor(CourseRole.CourseAdmin, "sysAdmin"));
			Assert.That(!user.IsBestAccessFor(CourseRole.Instructor, "sysAdmin"));
			Assert.That(!user.IsBestAccessFor(CourseRole.Tester, "sysAdmin"));
		}

		[Test]
		public void IsBestAccessFor_SysAdmin()
		{
			var admin = GetSysAdmin();

			Assert.That(!admin.IsBestAccessFor(CourseRole.CourseAdmin, "courseAdmin"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Instructor, "courseAdmin"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Tester, "courseAdmin"));

			Assert.That(!admin.IsBestAccessFor(CourseRole.CourseAdmin, "instructor"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Instructor, "instructor"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Tester, "instructor"));

			Assert.That(!admin.IsBestAccessFor(CourseRole.CourseAdmin, "tester"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Instructor, "tester"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Tester, "tester"));

			Assert.That(!admin.IsBestAccessFor(CourseRole.CourseAdmin, "sysAdmin"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Instructor, "sysAdmin"));
			Assert.That(!admin.IsBestAccessFor(CourseRole.Tester, "sysAdmin"));
		}

		private static ClaimsPrincipal GetUser()
		{
			var identities = new ClaimsIdentity();
			identities.AddCourseRoles(new Dictionary<string, CourseRole>
			{
				{ "courseAdmin", CourseRole.CourseAdmin }, { "instructor", CourseRole.Instructor }, { "tester", CourseRole.Tester }
			});
			return new ClaimsPrincipal(identities);
		}

		private static ClaimsPrincipal GetSysAdmin()
		{
			var user = GetUser();
			user.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, LmsRoles.SysAdmin) }));
			return user;
		}
	}
}