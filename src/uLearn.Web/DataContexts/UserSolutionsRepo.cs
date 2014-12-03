using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
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
			var canSeeEverything = user.IsInRole(LmsRoles.Tester) || user.IsInRole(LmsRoles.Admin) || user.IsInRole(LmsRoles.Instructor);
			if (canSeeEverything)
				return courseManager.GetCourse(courseId).Slides.Select(s => s.Info.UnitName).Distinct().ToList();
			return db.Units.Where(u => u.CourseId == courseId && u.PublishTime <= DateTime.Now).Select(u => u.UnitName).ToList();
		}

		public DateTime GetNextUnitPublishTime(string courseId)
		{
			return db.Units.Where(u => u.CourseId == courseId && u.PublishTime > DateTime.Now).Select(u => u.PublishTime).Concat(new[]{DateTime.MaxValue}).Min();
		}
	}

	public class UserSolutionsRepo
	{
		private readonly ULearnDb db;
		private readonly TextsRepo textsRepo = new TextsRepo();

		public UserSolutionsRepo() : this(new ULearnDb())
		{
			
		}

		public UserSolutionsRepo(ULearnDb db)
		{
			this.db = db;
		}

		public async Task<UserSolution> AddUserSolution(string courseId, string slideId, string code, bool isRightAnswer,
			string compilationError,
			string output, string userId)
		{
			var hash = (await textsRepo.AddText(code)).Hash;
			var compilationErrorHash = (await textsRepo.AddText(compilationError)).Hash;
			var outputHash = (await textsRepo.AddText(output)).Hash;

			var userSolution = db.UserSolutions.Add(new UserSolution
			{
				SolutionCodeHash = hash,
				CompilationErrorHash = compilationErrorHash,
				CourseId = courseId,
				SlideId = slideId,
				IsCompilationError = !string.IsNullOrWhiteSpace(compilationError),
				IsRightAnswer = isRightAnswer,
				OutputHash = outputHash,
				Timestamp = DateTime.Now,
				UserId = userId,
				CodeHash = code.Split('\n').Select(x => x.Trim()).Aggregate("", (x, y) => x + y).GetHashCode(),
				Likes = new List<Like>()
			});
			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbEntityValidationException e)
			{
				Debug.Write(
					string.Join("\r\n",
					e.EntityValidationErrors.SelectMany(v => v.ValidationErrors).Select(err => err.PropertyName + " " + err.ErrorMessage)));
			}
			return userSolution;
		}

		public void Delete(UserSolution userSolution)
		{
			db.UserSolutions.Remove(userSolution);
			db.SaveChanges();
		}

		///<returns>(likesCount, isLikedByThisUsed)</returns>
		public async Task<Tuple<int, bool>> Like(int solutionId, string userId)
		{
			var solutionForLike = db.UserSolutions.Find(solutionId);
			if (solutionForLike == null) throw new Exception("Solution " + solutionId + " not found");
			var hisLike = db.SolutionLikes.FirstOrDefault(like => like.UserId == userId && like.UserSolutionId == solutionId);
			var votedAlready = hisLike != null;
			var likesCount = solutionForLike.Likes.Count();
			if (votedAlready)
			{
				db.SolutionLikes.Remove(hisLike);
				likesCount--;
			}
			else
			{
				db.SolutionLikes.Add(new Like {UserSolutionId = solutionId, Timestamp = DateTime.Now, UserId = userId});
				likesCount++;
			}
			await db.SaveChangesAsync();
			return Tuple.Create(likesCount, !votedAlready);
		}

		public List<AcceptedSolutionInfo> GetAllAcceptedSolutions(string courseId, string slideId)
		{
			var prepared = db.UserSolutions
				.Where(x => x.IsRightAnswer && x.SlideId == slideId)
				.GroupBy(x => x.CodeHash)
				.Select(x => new { sol = x.OrderBy(y => y.Timestamp).FirstOrDefault(), likes = x.Sum(s => s.Likes.Count) })
				.ToList();

			var best = prepared
				.OrderByDescending(x => x.likes);
			var timeNow = DateTime.Now;
			var trending = prepared
				.OrderByDescending(x => (x.likes + 1) / timeNow.Subtract(x.sol.Timestamp).TotalMilliseconds);
			var newest = prepared
				.OrderByDescending(x => x.sol.Timestamp);
			var answer = best.Take(3).Concat(trending.Take(3)).Concat(newest).Distinct().Take(10).Select(x => x.sol);
			return answer
				.Select(x => new AcceptedSolutionInfo(x.SolutionCode.Text, x.Id, x.Likes.Select(y => y.UserId)))
				.ToList();
		}

		public bool IsUserPassedTask(string courseId, string slideId, string userId)
		{
			return db.UserSolutions.Any(x => x.SlideId == slideId && x.UserId == userId && x.IsRightAnswer);
		}

		public string FindLatestAcceptedSolution(string courseId, string slideId, string userId)
		{
			var allUserSolutionOnThisTask = db.UserSolutions
				.Where(x => x.SlideId == slideId && x.UserId == userId && x.IsRightAnswer).ToList();
			var answer = allUserSolutionOnThisTask
				.OrderByDescending(x => x.Timestamp)
				.FirstOrDefault();
			return answer == null ? null : answer.SolutionCode.Text;
		}

		public int GetAcceptedSolutionsCount(string slideId, string courseId)
		{
			return db.UserSolutions.Where(x => x.SlideId == slideId && x.IsRightAnswer).Select(x => x.UserId).Distinct().Count();
		}

		public HashSet<string> GetIdOfPassedSlides(string courseId, string userId)
		{
			return new HashSet<string>(db.UserSolutions
				.Where(x => x.IsRightAnswer && x.CourseId == courseId && x.UserId == userId)
				.Select(x => x.SlideId)
				.Distinct());
		}
	}
}