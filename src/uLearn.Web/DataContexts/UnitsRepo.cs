using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using uLearn.Web.Models;

namespace uLearn.Web.DataContexts
{
	public class UnitsRepo
	{
		private readonly ULearnDb db;
		private readonly CourseManager courseManager;

		public UnitsRepo() : this(new ULearnDb(), WebCourseManager.Instance)
		{

		}

		public UnitsRepo(ULearnDb db, CourseManager courseManager)
		{
			this.db = db;
			this.courseManager = courseManager;
		}

		public List<string> GetVisibleUnits(string courseId, IPrincipal user)
		{
			var canSeeEverything = user.HasAccessFor(courseId, CourseRoles.Tester);
			if (canSeeEverything)
				return courseManager.GetCourse(courseId).Slides.Select(s => s.Info.UnitName).Distinct().ToList();
			return db.Units.Where(u => u.CourseId == courseId && u.PublishTime <= DateTime.Now).Select(u => u.UnitName).ToList();
		}

		public DateTime GetNextUnitPublishTime(string courseId)
		{
			return db.Units.Where(u => u.CourseId == courseId && u.PublishTime > DateTime.Now).Select(u => u.PublishTime).Concat(new[] { DateTime.MaxValue }).Min();
		}
	}
}