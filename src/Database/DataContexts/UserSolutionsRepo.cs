using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database.Models;
using EntityFramework.Functions;
using log4net;
using RunCsJob.Api;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;

namespace Database.DataContexts
{
	public class UserSolutionsRepo
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(UserSolutionsRepo));
		private readonly ULearnDb db;
		private readonly TextsRepo textsRepo;
		private readonly VisitsRepo visitsRepo;
		private readonly CourseManager courseManager;

		private static volatile ConcurrentDictionary<int, DateTime> unhandledSubmissions = new ConcurrentDictionary<int, DateTime>();
		private static volatile ConcurrentDictionary<int, DateTime> handledSubmissions = new ConcurrentDictionary<int, DateTime>();
		private static readonly TimeSpan handleTimeout = TimeSpan.FromMinutes(3);

		public UserSolutionsRepo(ULearnDb db, CourseManager courseManager)
		{
			this.db = db;
			this.courseManager = courseManager;
			textsRepo = new TextsRepo(db);
			visitsRepo = new VisitsRepo(db);
		}

		public async Task<UserExerciseSubmission> AddUserExerciseSubmission(
			string courseId, Guid slideId,
			string code, string compilationError, string output,
			string userId, string executionServiceName, string displayName,
			Language language,
			AutomaticExerciseCheckingStatus status = AutomaticExerciseCheckingStatus.Waiting)
		{
			if (string.IsNullOrWhiteSpace(code))
				code = "// no code";
			var hash = (await textsRepo.AddText(code)).Hash;
			var compilationErrorHash = (await textsRepo.AddText(compilationError)).Hash;
			var outputHash = (await textsRepo.AddText(output)).Hash;

			AutomaticExerciseChecking automaticChecking;
			if (language.HasAutomaticChecking())
			{
				automaticChecking = new AutomaticExerciseChecking
				{
					CourseId = courseId,
					SlideId = slideId,
					UserId = userId,
					Timestamp = DateTime.Now,
					CompilationErrorHash = compilationErrorHash,
					IsCompilationError = !string.IsNullOrWhiteSpace(compilationError),
					OutputHash = outputHash,
					ExecutionServiceName = executionServiceName,
					DisplayName = displayName,
					Status = status,
					IsRightAnswer = false,
				};

				db.AutomaticExerciseCheckings.Add(automaticChecking);
			}
			else
			{
				automaticChecking = null;
			}

			var submission = new UserExerciseSubmission
			{
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = DateTime.Now,
				SolutionCodeHash = hash,
				CodeHash = code.Split('\n').Select(x => x.Trim()).Aggregate("", (x, y) => x + y).GetHashCode(),
				Likes = new List<Like>(),
				AutomaticChecking = automaticChecking,
				AutomaticCheckingIsRightAnswer = automaticChecking?.IsRightAnswer ?? true,
				Language = language,
			};

			db.UserExerciseSubmissions.Add(submission);

			await db.SaveChangesAsync();

			return submission;
		}

		public async Task RemoveSubmission(UserExerciseSubmission submission)
		{
			if (submission.Likes != null)
				db.SolutionLikes.RemoveRange(submission.Likes);
			if (submission.AutomaticChecking != null)
				db.AutomaticExerciseCheckings.Remove(submission.AutomaticChecking);
			if (submission.ManualCheckings != null)
				db.ManualExerciseCheckings.RemoveRange(submission.ManualCheckings);

			db.UserExerciseSubmissions.Remove(submission);
			await db.SaveChangesAsync();
		}

		[Obsolete]
		public async Task SetAntiPlagiarismSubmissionId(UserExerciseSubmission submission, int antiPlagiarismSubmissionId)
		{
			submission.AntiPlagiarismSubmissionId = antiPlagiarismSubmissionId;
			await db.SaveChangesAsync();
		}

		///<returns>(likesCount, isLikedByThisUsed)</returns>
		public async Task<Tuple<int, bool>> Like(int solutionId, string userId)
		{
			return await FuncUtils.TrySeveralTimesAsync(() => TryLike(solutionId, userId), 3);
		}

		private async Task<Tuple<int, bool>> TryLike(int solutionId, string userId)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				var solutionForLike = db.UserExerciseSubmissions.Find(solutionId);
				if (solutionForLike == null)
					throw new Exception("Solution " + solutionId + " not found");
				var hisLike = db.SolutionLikes.FirstOrDefault(like => like.UserId == userId && like.SubmissionId == solutionId);
				var votedAlready = hisLike != null;
				var likesCount = solutionForLike.Likes.Count;
				if (votedAlready)
				{
					db.SolutionLikes.Remove(hisLike);
					likesCount--;
				}
				else
				{
					db.SolutionLikes.Add(new Like { SubmissionId = solutionId, Timestamp = DateTime.Now, UserId = userId });
					likesCount++;
				}
				await db.SaveChangesAsync();

				transaction.Commit();
				
				return Tuple.Create(likesCount, !votedAlready);
			}
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId)
		{
			return db.UserExerciseSubmissions.Include(s => s.ManualCheckings).Where(x => x.CourseId == courseId);
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, IEnumerable<Guid> slidesIds)
		{
			return GetAllSubmissions(courseId).Where(x => slidesIds.Contains(x.SlideId));
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish)
		{
			return GetAllSubmissions(courseId, slidesIds)
				.Where(x =>
					periodStart <= x.Timestamp &&
					x.Timestamp <= periodFinish
				);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish)
		{
			return GetAllSubmissions(courseId, slidesIds, periodStart, periodFinish).Where(s => s.AutomaticCheckingIsRightAnswer);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId, IEnumerable<Guid> slidesIds)
		{
			return GetAllSubmissions(courseId, slidesIds).Where(s => s.AutomaticCheckingIsRightAnswer);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId)
		{
			return GetAllSubmissions(courseId).Where(s => s.AutomaticCheckingIsRightAnswer);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, IEnumerable<Guid> slideIds, string userId)
		{
			return GetAllAcceptedSubmissions(courseId, slideIds).Where(s => s.UserId == userId);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, string userId)
		{
			return GetAllAcceptedSubmissions(courseId).Where(s => s.UserId == userId);
		}

		public IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, Guid slideId, string userId)
		{
			return GetAllAcceptedSubmissionsByUser(courseId, new List<Guid> { slideId }, userId);
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissionsByUser(string courseId, Guid slideId, string userId)
		{
			return GetAllSubmissions(courseId, new List<Guid> { slideId }).Where(s => s.UserId == userId);
		}
		
		public IQueryable<UserExerciseSubmission> GetAllSubmissionsByUsers(SubmissionsFilterOptions filterOptions)
		{
			var submissions = GetAllSubmissions(filterOptions.CourseId, filterOptions.SlideIds);
			if (filterOptions.IsUserIdsSupplement)
				submissions = submissions.Where(s => ! filterOptions.UserIds.Contains(s.UserId));
			else
				submissions = submissions.Where(s => filterOptions.UserIds.Contains(s.UserId));
			return submissions;
		}

		public List<AcceptedSolutionInfo> GetBestTrendingAndNewAcceptedSolutions(string courseId, List<Guid> slidesIds)
		{
			var prepared = GetAllAcceptedSubmissions(courseId, slidesIds)
				.GroupBy(x => x.CodeHash, (codeHash, ss) => new { codeHash, timestamp = ss.Min(s => s.Timestamp) })
				.Join(
					GetAllAcceptedSubmissions(courseId, slidesIds),
					g => g,
					s => new { codeHash = s.CodeHash, timestamp = s.Timestamp }, (k, s) => new { submission = s, k.timestamp })
				.Select(x => new { x.submission.Id, likes = x.submission.Likes.Count, x.timestamp })
				.ToList();

			var best = prepared
				.OrderByDescending(x => x.likes);
			var timeNow = DateTime.Now;
			var trending = prepared
				.OrderByDescending(x => (x.likes + 1) / timeNow.Subtract(x.timestamp).TotalMilliseconds);
			var newest = prepared
				.OrderByDescending(x => x.timestamp);
			var selectedSubmissionsIds = best.Take(3).Concat(trending.Take(3)).Concat(newest).Distinct().Take(10).Select(x => x.Id);

			var selectedSubmissions = db.UserExerciseSubmissions
				.Where(s => selectedSubmissionsIds.Contains(s.Id))
				.Select(s => new { s.Id, Code = s.SolutionCode.Text, Likes = s.Likes.Select(y => y.UserId) })
				.ToList();
			return selectedSubmissions
				.Select(s => new AcceptedSolutionInfo(s.Code, s.Id, s.Likes))
				.OrderByDescending(info => info.UsersWhoLike.Count)
				.ToList();
		}

		public List<AcceptedSolutionInfo> GetBestTrendingAndNewAcceptedSolutions(string courseId, Guid slideId)
		{
			return GetBestTrendingAndNewAcceptedSolutions(courseId, new List<Guid> { slideId });
		}

		public int GetAcceptedSolutionsCount(string courseId, Guid slideId)
		{
			return GetAllAcceptedSubmissions(courseId, new List<Guid> { slideId }).Select(x => x.UserId).Distinct().Count();
		}

		public bool IsCheckingSubmissionByUser(string courseId, Guid slideId, string userId, DateTime periodStart, DateTime periodFinish)
		{
			var automaticCheckingsIds = GetAllSubmissions(courseId, new List<Guid> { slideId }, periodStart, periodFinish)
				.Where(s => s.UserId == userId)
				.Select(s => s.AutomaticCheckingId)
				.ToList();
			return db.AutomaticExerciseCheckings.Any(c => automaticCheckingsIds.Contains(c.Id) && c.Status != AutomaticExerciseCheckingStatus.Done);
		}

		public HashSet<Guid> GetIdOfPassedSlides(string courseId, string userId)
		{
			return new HashSet<Guid>(db.AutomaticExerciseCheckings
				.Where(x => x.IsRightAnswer && x.CourseId == courseId && x.UserId == userId)
				.Select(x => x.SlideId)
				.Distinct());
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(int max, int skip)
		{
			return db.UserExerciseSubmissions
				.OrderByDescending(x => x.Timestamp)
				.Skip(skip)
				.Take(max);
		}

		public UserExerciseSubmission FindNoTrackingSubmission(int id)
		{
			return FuncUtils.TrySeveralTimes(() => TryFindNoTrackingSubmission(id), 3, () => Thread.Sleep(TimeSpan.FromMilliseconds(200)));
		}

		private UserExerciseSubmission TryFindNoTrackingSubmission(int id)
		{
			var submission = db.UserExerciseSubmissions.AsNoTracking().SingleOrDefault(x => x.Id == id);
			if (submission == null)
				return null;
			submission.SolutionCode = textsRepo.GetText(submission.SolutionCodeHash);
			
			if (submission.AutomaticChecking != null)
			{
				submission.AutomaticChecking.Output = textsRepo.GetText(submission.AutomaticChecking.OutputHash);
				submission.AutomaticChecking.CompilationError = textsRepo.GetText(submission.AutomaticChecking.CompilationErrorHash);
			}
			
			return submission;
		}

		private static volatile SemaphoreSlim getSubmissionSemaphore = new SemaphoreSlim(1);

		public async Task<UserExerciseSubmission> GetUnhandledSubmission(string agentName, Language language)
		{
			log.Info("GetUnhandledSubmission(): trying to acquire semaphore");
			var semaphoreLocked = await getSubmissionSemaphore.WaitAsync(TimeSpan.FromSeconds(2));
			if (!semaphoreLocked)
			{
				log.Error("GetUnhandledSubmission(): Can't lock semaphore for 2 seconds");
				return null;
			}
			log.Info("GetUnhandledSubmission(): semaphore acquired!");

			try
			{
				return await TryGetExerciseSubmission(agentName, language);
			}
			catch (Exception e)
			{
				log.Error("GetUnhandledSubmission() error", e);
				return null;
			}
			finally
			{
				log.Info("GetUnhandledSubmission(): trying to release semaphore");
				getSubmissionSemaphore.Release();
				log.Info("GetUnhandledSubmission(): semaphore released");
			}
		}

		private async Task<UserExerciseSubmission> TryGetExerciseSubmission(string agentName, Language language)
		{
			var notSoLongAgo = DateTime.Now - TimeSpan.FromMinutes(15);
			UserExerciseSubmission submission;
			using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
			{
				var submissionsQueryable = db.UserExerciseSubmissions
					.AsNoTracking()
					.Where(s =>
						s.Timestamp > notSoLongAgo
						&& s.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Waiting
						&& s.Language == language);
				
				if (!submissionsQueryable.Any())
					return null;
				
				var maxId = submissionsQueryable.Select(s => s.Id).DefaultIfEmpty(-1).Max();
				submission = submissionsQueryable.FirstOrDefault(s => s.Id == maxId);
				if (submission == null)
					return null;
				
				/* Mark submission as "running" */
				submission.AutomaticChecking.Status = AutomaticExerciseCheckingStatus.Running;
				submission.AutomaticChecking.CheckingAgentName = agentName;

				await SaveAll(new List<AutomaticExerciseChecking> { submission.AutomaticChecking }).ConfigureAwait(false);

				transaction.Commit();

				db.ObjectContext().AcceptAllChanges();
			}

			unhandledSubmissions.TryRemove(submission.Id, out _);

			return submission;
		}

		public UserExerciseSubmission FindSubmissionById(int id)
		{
			return db.UserExerciseSubmissions.Find(id);
		}

		public UserExerciseSubmission FindSubmissionById(string id)
		{
			return db.UserExerciseSubmissions.FirstOrDefault(s => s.Id.ToString() == id);
		}

		public List<UserExerciseSubmission> FindSubmissionsByIds(List<string> checkingsIds)
		{
			return db.UserExerciseSubmissions.Where(c => checkingsIds.Contains(c.Id.ToString())).ToList();
		}

		private void UpdateIsRightAnswerForSubmission(AutomaticExerciseChecking checking)
		{
			db.UserExerciseSubmissions
				.Where(s => s.AutomaticCheckingId == checking.Id)
				.ForEach(s => s.AutomaticCheckingIsRightAnswer = checking.IsRightAnswer);
		}

		protected async Task SaveAll(IEnumerable<AutomaticExerciseChecking> checkings)
		{
			foreach (var checking in checkings)
			{
				log.Info($"Обновляю статус автоматической проверки #{checking.Id}: {checking.Status}");
				db.AutomaticExerciseCheckings.AddOrUpdate(checking);
				UpdateIsRightAnswerForSubmission(checking);
			}
			try
			{
				await db.ObjectContext().SaveChangesAsync(SaveOptions.DetectChangesBeforeSave).ConfigureAwait(false);
			}
			catch (DbEntityValidationException e)
			{
				throw new Exception(
					string.Join("\r\n",
						e.EntityValidationErrors.SelectMany(v => v.ValidationErrors).Select(err => err.PropertyName + " " + err.ErrorMessage)));
			}
		}

		public async Task SaveResult(RunningResults result, Func<UserExerciseSubmission, Task> onSave)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				log.Info($"Сохраняю информацию о проверке решения {result.Id}");
				var submission = FindSubmissionById(result.Id);
				if (submission == null)
				{
					log.Warn($"Не нашёл в базе данных решение {result.Id}");
					return;
				}
				var aec = await UpdateAutomaticExerciseChecking(submission.AutomaticChecking, result).ConfigureAwait(false);
				await SaveAll(Enumerable.Repeat(aec, 1)).ConfigureAwait(false);

				await onSave(submission).ConfigureAwait(false);

				transaction.Commit();
				db.ObjectContext().AcceptAllChanges();
				
				if (!handledSubmissions.TryAdd(submission.Id, DateTime.Now))
					log.Warn($"Не удалось запомнить, что проверка {submission.Id} проверена, а результат сохранен в базу");

				log.Info($"Есть информация о следующих проверках, которые ещё не записаны в базу клиентом: [{string.Join(", ", handledSubmissions.Keys)}]");
			}
		}

		private async Task<AutomaticExerciseChecking> UpdateAutomaticExerciseChecking(AutomaticExerciseChecking checking, RunningResults result)
		{
			var compilationErrorHash = (await textsRepo.AddText(result.CompilationOutput)).Hash;
			var output = result.GetOutput().NormalizeEoln();
			var outputHash = (await textsRepo.AddText(output)).Hash;

			var isWebRunner = checking.CourseId == "web" && checking.SlideId == Guid.Empty;
			var exerciseSlide = isWebRunner ? null : (ExerciseSlide)courseManager.GetCourse(checking.CourseId).GetSlideById(checking.SlideId);
			
			var isRightAnswer = IsRightAnswer(result, output, exerciseSlide?.Exercise);
			var score = exerciseSlide != null && isRightAnswer ? exerciseSlide.Scoring.PassedTestsScore: 0;

			/* For skipped slides score is always 0 */
			if (visitsRepo.IsSkipped(checking.CourseId, checking.SlideId, checking.UserId))
				score = 0;

			var newChecking = new AutomaticExerciseChecking
			{
				Id = checking.Id,
				CourseId = checking.CourseId,
				SlideId = checking.SlideId,
				UserId = checking.UserId,
				Timestamp = checking.Timestamp,
				CompilationErrorHash = compilationErrorHash,
				IsCompilationError = result.Verdict == Verdict.CompilationError,
				OutputHash = outputHash,
				ExecutionServiceName = checking.ExecutionServiceName,
				Status = AutomaticExerciseCheckingStatus.Done,
				DisplayName = checking.DisplayName,
				Elapsed = DateTime.Now - checking.Timestamp,
				IsRightAnswer = isRightAnswer,
				Score = score,
				CheckingAgentName = checking.CheckingAgentName,
			};

			return newChecking;
		}

		private bool IsRightAnswer(RunningResults result, string output, AbstractExerciseBlock exerciseBlock)
		{
			if (result.Verdict != Verdict.Ok )
				return false;
			
			/* For sandbox runner */
			if (exerciseBlock == null)
				return false;

			if (exerciseBlock.ExerciseType == ExerciseType.CheckExitCode)
				return true;

			if (exerciseBlock.ExerciseType == ExerciseType.CheckOutput)
			{
				var expectedOutput = exerciseBlock.ExpectedOutput.NormalizeEoln();
				return output.Equals(expectedOutput);
			}

			throw new InvalidOperationException($"Unknown exercise type for checking: {exerciseBlock.ExerciseType}");
		}

		public async Task RunAutomaticChecking(UserExerciseSubmission submission, TimeSpan timeout, bool waitUntilChecked)
		{
			log.Info($"Запускаю автоматическую проверку решения. ID посылки: {submission.Id}");
			unhandledSubmissions.TryAdd(submission.Id, DateTime.Now);

			if (!waitUntilChecked)
			{
				log.Info($"Не буду ожидать результатов проверки посылки {submission.Id}");
				return;
			}

			var sw = Stopwatch.StartNew();
			while (sw.Elapsed < timeout)
			{
				await WaitUntilSubmissionHandled(TimeSpan.FromSeconds(5), submission.Id);
				var updatedSubmission = FindNoTrackingSubmission(submission.Id);
				if (updatedSubmission == null)
					break;

				if (updatedSubmission.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Done)
				{
					log.Info($"Посылка {submission.Id} проверена. Результат: {updatedSubmission.AutomaticChecking.GetVerdict()}");
					return;
				}
			}

			/* If something is wrong */
			unhandledSubmissions.TryRemove(submission.Id, out DateTime value);
			throw new SubmissionCheckingTimeout();
		}

		public Dictionary<int, string> GetSolutionsForSubmissions(IEnumerable<int> submissionsIds)
		{
			var solutionsHashes = db.UserExerciseSubmissions
				.Where(s => submissionsIds.Contains(s.Id))
				.Select(s => new { Hash = s.SolutionCodeHash, SubmissionId = s.Id }).ToList();
			var textsByHash = textsRepo.GetTextsByHashes(solutionsHashes.Select(s => s.Hash));
			return solutionsHashes.ToDictionary(s => s.SubmissionId, s => textsByHash.GetOrDefault(s.Hash, ""));
		}

		public async Task WaitAnyUnhandledSubmissions(TimeSpan timeout)
		{
			var sw = Stopwatch.StartNew();
			while (sw.Elapsed < timeout)
			{
				if (unhandledSubmissions.Count > 0)
				{
					log.Info($"Список невзятых пока на проверку решений: [{string.Join(", ", unhandledSubmissions.Keys)}]");
					ClearHandleDictionaries();
					return;
				}
				await Task.Delay(TimeSpan.FromMilliseconds(100));
			}
		}

		public async Task WaitUntilSubmissionHandled(TimeSpan timeout, int submissionId)
		{
			log.Info($"Вхожу в цикл ожидания результатов проверки решения {submissionId}. Жду {timeout.TotalSeconds} секунд");
			var sw = Stopwatch.StartNew();
			while (sw.Elapsed < timeout)
			{
				if (handledSubmissions.ContainsKey(submissionId))
				{
					DateTime value;
					handledSubmissions.TryRemove(submissionId, out value);
					return;
				}
				await Task.Delay(TimeSpan.FromMilliseconds(100));
			}
		}

		private static void ClearHandleDictionaries()
		{
			var timeout = DateTime.Now.Subtract(handleTimeout);
			ClearHandleDictionary(handledSubmissions, timeout);
			ClearHandleDictionary(unhandledSubmissions, timeout);
		}

		private static void ClearHandleDictionary(ConcurrentDictionary<int, DateTime> dictionary, DateTime timeout)
		{
			foreach (var key in dictionary.Keys)
			{
				DateTime value;
				if (dictionary.TryGetValue(key, out value) && value < timeout)
					dictionary.TryRemove(key, out value);
			}
		}
	}

	public class SubmissionCheckingTimeout : Exception
	{
	}
}