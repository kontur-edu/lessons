using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Database.Extensions;
using Database.Models;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Units;

namespace Database.DataContexts
{
	public class UnitsRepo
	{
		private readonly ULearnDb db;

		public UnitsRepo(ULearnDb db)
		{
			this.db = db;
		}

		public List<Unit> GetVisibleUnits(Course course, IPrincipal user)
		{
			var canSeeEverything = user.HasAccessFor(course.Id, CourseRole.Tester);
			if (canSeeEverything)
				return course.Units;

			var visibleUnitsIds = new HashSet<Guid>(db.UnitAppearances
				.Where(u => u.CourseId == course.Id && u.PublishTime <= DateTime.Now)
				.Select(u => u.UnitId));
			return course.Units.Where(u => visibleUnitsIds.Contains(u.Id)).ToList();
		}
		
		public bool IsUnitVisibleForStudents(Course course, Guid unitId)
		{
			return db.UnitAppearances.Any(u => u.UnitId == unitId && u.CourseId == course.Id && u.PublishTime <= DateTime.Now);
		}

		public DateTime GetNextUnitPublishTime(string courseId)
		{
			return db.UnitAppearances.Where(u => u.CourseId == courseId && u.PublishTime > DateTime.Now)
				.Select(u => u.PublishTime)
				.Concat(new[] { DateTime.MaxValue }).Min();
		}
	}
}