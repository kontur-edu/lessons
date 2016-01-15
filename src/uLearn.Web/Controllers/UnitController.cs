﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	[PostAuthorize(Roles = LmsRoles.Admin)]
	public class UnitController : Controller
	{
		private readonly CourseManager courseManager;
		private readonly ULearnDb db;

		public UnitController()
		{
			db = new ULearnDb();
			courseManager = WebCourseManager.Instance;
		}

		public ActionResult CourseList(string courseId = "")
		{
			var model = new CourseListViewModel
			{
				Courses = courseManager.GetCourses().ToList(), 
				PackageNames = courseManager.GetStagingPackages().ToList(),
				LastLoadedCourse = courseId
			};
			return View(model);
		}
		
		[HttpPost]
		public ActionResult ReloadCourse(string packageName, string returnUrl = null)
		{
			var courseId = courseManager.ReloadCourse(packageName);
			if (returnUrl != null) return Redirect(returnUrl);
			return RedirectToAction("CourseList", new { courseId });
		}

		public ActionResult SpellingErrors(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			return PartialView(course.SpellCheck());
		}

		public ActionResult List(string courseId)
		{
			Course course = courseManager.GetCourse(courseId);
			List<UnitAppearance> appearances = db.Units.Where(u => u.CourseId == course.Id).ToList();
			List<Tuple<string, UnitAppearance>> unitAppearances =
				course.Slides
					.Select(s => s.Info.UnitName)
					.Distinct()
					.Select(unitName => Tuple.Create(unitName, appearances.FirstOrDefault(a => a.UnitName.RemoveBom() == unitName)))
					.ToList();
			return View(new UnitsListViewModel(course.Id, course.Title, unitAppearances, DateTime.Now));
		}

		[HttpPost]
		public async Task<RedirectToRouteResult> SetPublishTime(string courseId, string unitName, string publishTime)
		{

			var oldInfo = await db.Units.Where(u => u.CourseId == courseId && u.UnitName == unitName).ToListAsync();
			db.Units.RemoveRange(oldInfo);
			var unitAppearance = new UnitAppearance
			{
				CourseId = courseId,
				UnitName = unitName,
				UserName = User.Identity.Name,
				PublishTime = DateTime.Parse(publishTime),
			};
			db.Units.Add(unitAppearance);
			await db.SaveChangesAsync();
			return RedirectToAction("List", new { courseId });
		}

		[HttpPost]
		public async Task<RedirectToRouteResult> RemovePublishTime(string courseId, string unitName)
		{
			var unitAppearance = await db.Units.FirstOrDefaultAsync(u => u.CourseId == courseId && u.UnitName == unitName);
			if (unitAppearance != null)
			{
				db.Units.Remove(unitAppearance);
				await db.SaveChangesAsync();
			}
			return RedirectToAction("List", new { courseId });
		}

		public ActionResult DownloadPackage(string packageName)
		{
			return File(courseManager.GetStagingPackagePath(packageName), "application/zip", packageName);
		}

		[HttpPost]
		public ActionResult UploadCourse(HttpPostedFileBase file)
		{
			if (file == null || file.ContentLength <= 0)
				return RedirectToAction("CourseList");

			var fileName = Path.GetFileName(file.FileName);
			if (fileName == null || !fileName.ToLower().EndsWith(".zip"))
				return RedirectToAction("CourseList");

			var destinationFile = courseManager.StagedDirectory.GetFile(fileName);
			file.SaveAs(destinationFile.FullName);
			var courseId = courseManager.ReloadCourse(fileName);
			return RedirectToAction("CourseList", new { courseId });
		}
	}

	public class UnitsListViewModel
	{
		public string CourseId;
		public string CourseTitle;
		public DateTime CurrentDateTime;
		public List<Tuple<string, UnitAppearance>> Units;

		public UnitsListViewModel(string courseId, string courseTitle, List<Tuple<string, UnitAppearance>> units,
			DateTime currentDateTime)
		{
			CourseId = courseId;
			CourseTitle = courseTitle;
			Units = units;
			CurrentDateTime = currentDateTime;
		}
	}

	public class CourseListViewModel
	{
		public List<Course> Courses;
		public List<StagingPackage> PackageNames;
		public string LastLoadedCourse { get; set; }
	}

}