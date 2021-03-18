using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Database.Extensions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.RunCheckerJobApi;

namespace Database.Repos
{
	public class UserSolutionsRepo : IUserSolutionsRepo
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(UserSolutionsRepo));
		private readonly UlearnDb db;
		private readonly ITextsRepo textsRepo;
		private readonly IWorkQueueRepo workQueueRepo;
		private readonly IWebCourseManager courseManager;
		private const int queueId = 1;

		public UserSolutionsRepo(UlearnDb db,
			ITextsRepo textsRepo, IWorkQueueRepo workQueueRepo,
			IWebCourseManager courseManager)
		{
			this.db = db;
			this.textsRepo = textsRepo;
			this.workQueueRepo = workQueueRepo;
			this.courseManager = courseManager;
		}

		public async Task<int> AddUserExerciseSubmission(
			string courseId, Guid slideId,
			string code, string compilationError, string output,
			string userId, string executionServiceName, string displayName,
			Language language,
			string sandbox,
			bool hasAutomaticChecking,
			AutomaticExerciseCheckingStatus? status = AutomaticExerciseCheckingStatus.Waiting)
		{
			if (string.IsNullOrWhiteSpace(code))
				code = "// no code";
			var hash = (await textsRepo.AddText(code)).Hash;
			var compilationErrorHash = (await textsRepo.AddText(compilationError)).Hash;
			var outputHash = (await textsRepo.AddText(output)).Hash;

			AutomaticExerciseChecking automaticChecking;
			if (hasAutomaticChecking)
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
					Status = status.Value,
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
				Sandbox = sandbox
			};

			db.UserExerciseSubmissions.Add(submission);

			await db.SaveChangesAsync();

			return submission.Id;
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
				var solutionForLike = await db.UserExerciseSubmissions.FindAsync(solutionId);
				if (solutionForLike == null)
					throw new Exception("Solution " + solutionId + " not found");
				var hisLike = await db.SolutionLikes.FirstOrDefaultAsync(like => like.UserId == userId && like.SubmissionId == solutionId);
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

				await transaction.CommitAsync();

				return Tuple.Create(likesCount, !votedAlready);
			}
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, bool includeManualAndAutomaticCheckings = true)
		{
			var query = db.UserExerciseSubmissions.AsQueryable();
			if (includeManualAndAutomaticCheckings)
				query = query
					.Include(s => s.ManualCheckings)
					.Include(s => s.AutomaticChecking);
			return query.Where(x => x.CourseId == courseId);
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
				submissions = submissions.Where(s => !filterOptions.UserIds.Contains(s.UserId));
			else
				submissions = submissions.Where(s => filterOptions.UserIds.Contains(s.UserId));
			return submissions;
		}

		public IQueryable<AutomaticExerciseChecking> GetAutomaticExerciseCheckingsByUsers(string courseId, Guid slideId, List<string> userIds)
		{
			var query = db.AutomaticExerciseCheckings.Where(c => c.CourseId == courseId && c.SlideId == slideId);
			if (userIds != null)
				query = query.Where(v => userIds.Contains(v.UserId));
			return query;
		}

		public async Task<List<AcceptedSolutionInfo>> GetBestTrendingAndNewAcceptedSolutions(string courseId, List<Guid> slidesIds)
		{
			var prepared = await GetAllAcceptedSubmissions(courseId, slidesIds)
				.GroupBy(x => x.CodeHash,
					(codeHash, ss) => new
					{
						codeHash,
						timestamp = ss.Min(s => s.Timestamp)
					})
				.Join(
					GetAllAcceptedSubmissions(courseId, slidesIds),
					g => g,
					s => new { codeHash = s.CodeHash, timestamp = s.Timestamp }, (k, s) => new { submission = s, k.timestamp })
				.Select(x => new { x.submission.Id, likes = x.submission.Likes.Count, x.timestamp })
				.ToListAsync();

			var best = prepared
				.OrderByDescending(x => x.likes);
			var timeNow = DateTime.Now;
			var trending = prepared
				.OrderByDescending(x => (x.likes + 1) / timeNow.Subtract(x.timestamp).TotalMilliseconds);
			var newest = prepared
				.OrderByDescending(x => x.timestamp);
			var selectedSubmissionsIds = best.Take(3).Concat(trending.Take(3)).Concat(newest).Distinct().Take(10).Select(x => x.Id);

			var selectedSubmissions = await db.UserExerciseSubmissions
				.Where(s => selectedSubmissionsIds.Contains(s.Id))
				.Select(s => new
				{
					s.Id,
					Code = s.SolutionCode.Text,
					Likes = s.Likes.Select(y => y.UserId)
				})
				.ToListAsync();
			return selectedSubmissions
				.Select(s => new AcceptedSolutionInfo(s.Code, s.Id, s.Likes))
				.OrderByDescending(info => info.UsersWhoLike.Count)
				.ToList();
		}

		public async Task<List<AcceptedSolutionInfo>> GetBestTrendingAndNewAcceptedSolutions(string courseId, Guid slideId)
		{
			return await GetBestTrendingAndNewAcceptedSolutions(courseId, new List<Guid> { slideId });
		}

		public async Task<int> GetAcceptedSolutionsCount(string courseId, Guid slideId)
		{
			return await GetAllAcceptedSubmissions(courseId, new List<Guid> { slideId })
				.Select(x => x.UserId)
				.Distinct()
				.CountAsync();
		}

		public async Task<bool> IsCheckingSubmissionByUser(string courseId, Guid slideId, string userId, DateTime periodStart, DateTime periodFinish)
		{
			var automaticCheckingsIds = await GetAllSubmissions(courseId, new List<Guid> { slideId }, periodStart, periodFinish)
				.Where(s => s.UserId == userId)
				.Select(s => s.AutomaticCheckingId)
				.ToListAsync();
			return await db.AutomaticExerciseCheckings
				.AnyAsync(c => automaticCheckingsIds.Contains(c.Id) && c.Status != AutomaticExerciseCheckingStatus.Done);
		}

		public async Task<HashSet<Guid>> GetIdOfPassedSlides(string courseId, string userId)
		{
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted}, TransactionScopeAsyncFlowOption.Enabled))
			{
				var ids = await db.AutomaticExerciseCheckings
					.Where(x => x.IsRightAnswer && x.CourseId == courseId && x.UserId == userId)
					.Select(x => x.SlideId)
					.Distinct()
					.ToListAsync();
				scope.Complete();
				return new HashSet<Guid>(ids);
			}
		}

		public async Task<bool> IsSlidePassed(string courseId, string userId, Guid slideId)
		{
			using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted}, TransactionScopeAsyncFlowOption.Enabled))
			{
				var result = await db.AutomaticExerciseCheckings
					.AnyAsync(x => x.IsRightAnswer && x.CourseId == courseId && x.UserId == userId && x.SlideId == slideId);
				scope.Complete();
				return result;
			}
		}

		public IQueryable<UserExerciseSubmission> GetAllSubmissions(int max, int skip)
		{
			return db.UserExerciseSubmissions
				.OrderByDescending(x => x.Timestamp)
				.Skip(skip)
				.Take(max);
		}

		public async Task<AutomaticExerciseCheckingStatus?> GetSubmissionAutomaticCheckingStatus(int id)
		{
			return await FuncUtils.TrySeveralTimesAsync(async () => await TryGetSubmissionAutomaticCheckingStatus(id), 3, () =>  Task.Delay(200));
		}

		private async Task<AutomaticExerciseCheckingStatus?> TryGetSubmissionAutomaticCheckingStatus(int id)
		{
			var statuses = await db.UserExerciseSubmissions
				.Where(s => s.Id == id)
				.Select(s => s.AutomaticChecking.Status)
				.ToListAsync();
			if (!statuses.Any())
				return null;
			return statuses.First();
		}

		public async Task<UserExerciseSubmission> GetUnhandledSubmission(string agentName, List<string> sandboxes)
		{
			try
			{
				return await TryGetExerciseSubmission(agentName, sandboxes);
			}
			catch (Exception e)
			{
				log.Error(e, "GetUnhandledSubmission() error");
				return null;
			}
		}

		private async Task<UserExerciseSubmission> TryGetExerciseSubmission(string agentName, List<string> sandboxes)
		{
			UserExerciseSubmission submission = null;
			while (submission == null)
			{
				var work = await workQueueRepo.TakeNoTracking(queueId, sandboxes);
				if (work == null)
					return null;

				var workItemId= int.Parse(work.ItemId);
				submission = await db.UserExerciseSubmissions
					.Include(s => s.AutomaticChecking)
					.Include(s => s.SolutionCode)
					.FirstOrDefaultAsync(s => s.Id == workItemId
						&& (s.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Waiting || s.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Running));
				var minutes = TimeSpan.FromMinutes(15);
				var notSoLongAgo = DateTime.Now - TimeSpan.FromMinutes(15);
				if (submission == null)
				{
					await workQueueRepo.Remove(work.Id);
				}
				else if (submission.Timestamp < notSoLongAgo)
				{
					await workQueueRepo.Remove(work.Id);
					submission.AutomaticChecking.Status = submission.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Waiting
						? AutomaticExerciseCheckingStatus.RequestTimeLimit
						: AutomaticExerciseCheckingStatus.Error;
					await db.SaveChangesAsync();
					if (submission.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Error)
						log.Error($"Не получил ответ от чеккера по проверке {submission.Id} за {minutes} минут");
					if (!UnhandledSubmissionsWaiter.HandledSubmissions.TryAdd(submission.Id, DateTime.Now))
						log.Warn($"Не удалось запомнить, что {submission.Id} не удалось проверить, и этот результат сохранен в базу");
					submission = null;
				}
			}

			submission.AutomaticChecking.Status = AutomaticExerciseCheckingStatus.Running;
			submission.AutomaticChecking.CheckingAgentName = agentName;
			await db.SaveChangesAsync();

			UnhandledSubmissionsWaiter.UnhandledSubmissions.TryRemove(submission.Id, out _);
			return submission;
		}

		public async Task<UserExerciseSubmission> FindSubmissionByIdNoTracking(int id)
		{
			// Без NoTracking результат может взяться из кэша, что не нужно.
			// Минус NoTracking, что не работает ленивая загрузка полей и нужно указать все Include.
			return await FuncUtils.TrySeveralTimesAsync(async () => await TryFindSubmissionByIdNoTracking(id), 3, () => Task.Delay(200));
		}

		private async Task<UserExerciseSubmission> TryFindSubmissionByIdNoTracking(int id)
		{
			return await db.UserExerciseSubmissions
				.AsNoTracking()	
				.Include(s => s.AutomaticChecking).ThenInclude(c => c.Output)
				.Include(s => s.AutomaticChecking).ThenInclude(c => c.DebugLogs)
				.Include(s => s.AutomaticChecking).ThenInclude(c => c.CompilationError)
				.Include(s => s.SolutionCode)
				.Include(s => s.Reviews).ThenInclude(c => c.Author)
				.Include(s => s.ManualCheckings).ThenInclude(c => c.Reviews).ThenInclude(r => r.Author)
				.SingleOrDefaultAsync(x => x.Id == id);
		}

		public async Task<UserExerciseSubmission> FindSubmissionById(int id)
		{
			return await db.UserExerciseSubmissions.FindAsync(id);
		}

		public async Task<UserExerciseSubmission> FindSubmissionById(string idString)
		{
			return int.TryParse(idString, out var id) ? await FindSubmissionById(id) : null;
		}

		public async Task<List<UserExerciseSubmission>> FindSubmissionsByIds(IEnumerable<int> checkingsIds)
		{
			return await db.UserExerciseSubmissions.Where(c => checkingsIds.Contains(c.Id)).ToListAsync();
		}

		public async Task SaveResult(RunningResults result, Func<UserExerciseSubmission, Task> onSave)
		{
			log.Info($"Сохраняю информацию о проверке решения {result.Id}"); 

			await workQueueRepo.RemoveByItemId(queueId, result.Id);

			using (var transaction = db.Database.BeginTransaction())
			{
				var submission = await FindSubmissionById(result.Id);
				if (submission == null)
				{
					log.Warn($"Не нашёл в базе данных решение {result.Id}");
					return;
				}

				var aec = await UpdateAutomaticExerciseChecking(submission.AutomaticChecking, result);
				await SaveAutomaticExerciseChecking(aec);

				await onSave(submission);

				await transaction.CommitAsync();
				db.ChangeTracker.AcceptAllChanges();

				if (!UnhandledSubmissionsWaiter.HandledSubmissions.TryAdd(submission.Id, DateTime.Now))
					log.Warn($"Не удалось запомнить, что проверка {submission.Id} проверена, а результат сохранен в базу");

				log.Info($"Есть информация о следующих проверках, которые ещё не записаны в базу клиентом: [{string.Join(", ", UnhandledSubmissionsWaiter.HandledSubmissions.Keys)}]");
			}
		}

		private async Task SaveAutomaticExerciseChecking(AutomaticExerciseChecking checking)
		{
			log.Info($"Обновляю статус автоматической проверки #{checking.Id}: {checking.Status}");
			db.AddOrUpdate(checking, c => c.Id == checking.Id);
			await UpdateIsRightAnswerForSubmission(checking);
			await db.SaveChangesAsync();
		}

		private async Task UpdateIsRightAnswerForSubmission(AutomaticExerciseChecking checking)
		{
			(await db.UserExerciseSubmissions
					.Where(s => s.AutomaticCheckingId == checking.Id)
					.ToListAsync())
				.ForEach(s => s.AutomaticCheckingIsRightAnswer = checking.IsRightAnswer);
		}

		private async Task<AutomaticExerciseChecking> UpdateAutomaticExerciseChecking(AutomaticExerciseChecking checking, RunningResults result)
		{
			var compilationErrorHash = (await textsRepo.AddText(result.CompilationOutput)).Hash;
			var output = result.GetOutput().NormalizeEoln();
			var outputHash = (await textsRepo.AddText(output)).Hash;
			
			var logs = result.GetLogs().NormalizeEoln();
			var logsHash = (await textsRepo.AddText(logs)).Hash;

			var isWebRunner = checking.CourseId == "web" && checking.SlideId == Guid.Empty;
			var exerciseSlide = isWebRunner
				? null
				: (ExerciseSlide)(await courseManager.GetCourseAsync(checking.CourseId))
					.GetSlideById(checking.SlideId, true);

			var isRightAnswer = IsRightAnswer(result, output, exerciseSlide?.Exercise);

			var elapsed = DateTime.Now - checking.Timestamp;
			elapsed = elapsed < TimeSpan.FromDays(1) ? elapsed : new TimeSpan(0, 23, 59, 59); 
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
				Status = result.Verdict == Verdict.SandboxError ? AutomaticExerciseCheckingStatus.Error : AutomaticExerciseCheckingStatus.Done,
				DisplayName = checking.DisplayName,
				Elapsed = elapsed,
				IsRightAnswer = isRightAnswer,
				CheckingAgentName = checking.CheckingAgentName,
				Points = result.Points,
				DebugLogsHash = logsHash
			};

			return newChecking;
		}

		private bool IsRightAnswer(RunningResults result, string output, AbstractExerciseBlock exerciseBlock)
		{
			if (result.Verdict != Verdict.Ok)
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
			
			if (exerciseBlock.ExerciseType == ExerciseType.CheckPoints)
			{
				if (!result.Points.HasValue)
					return false;
				const float eps = 0.00001f;
				return exerciseBlock.SmallPointsIsBetter ? result.Points.Value < exerciseBlock.PassingPoints + eps : result.Points.Value > exerciseBlock.PassingPoints - eps;
			}

			throw new InvalidOperationException($"Unknown exercise type for checking: {exerciseBlock.ExerciseType}");
		}

		public async Task RunAutomaticChecking(int submissionId, string sandbox, TimeSpan timeout, bool waitUntilChecked, int priority)
		{
			log.Info($"Запускаю автоматическую проверку решения. ID посылки: {submissionId}");
			UnhandledSubmissionsWaiter.UnhandledSubmissions.TryAdd(submissionId, DateTime.Now);
			await workQueueRepo.Add(queueId, submissionId.ToString(), sandbox ?? "csharp", priority);

			if (!waitUntilChecked)
			{
				log.Info($"Не буду ожидать результатов проверки посылки {submissionId}");
				return;
			}

			var sw = Stopwatch.StartNew();
			while (sw.Elapsed < timeout)
			{
				await UnhandledSubmissionsWaiter.WaitUntilSubmissionHandled(TimeSpan.FromSeconds(5), submissionId);
				var submissionAutomaticCheckingStatus = await GetSubmissionAutomaticCheckingStatus(submissionId);
				if (submissionAutomaticCheckingStatus == null)
					break;

				if (submissionAutomaticCheckingStatus == AutomaticExerciseCheckingStatus.Done)
				{
					log.Info($"Посылка {submissionId} проверена");
					return;
				}
				if (submissionAutomaticCheckingStatus == AutomaticExerciseCheckingStatus.Error)
				{
					log.Warn($"Во время проверки посылки {submissionId} произошла ошибка");
					return;
				}
			}

			/* If something is wrong */
			UnhandledSubmissionsWaiter.UnhandledSubmissions.TryRemove(submissionId, out _);
			throw new SubmissionCheckingTimeout();
		}

		public async Task<Dictionary<int, string>> GetSolutionsForSubmissions(IEnumerable<int> submissionsIds)
		{
			var solutionsHashes = await db.UserExerciseSubmissions
				.Where(s => submissionsIds.Contains(s.Id))
				.Select(s => new { Hash = s.SolutionCodeHash, SubmissionId = s.Id })
				.ToListAsync();
			var textsByHash = await textsRepo.GetTextsByHashes(solutionsHashes.Select(s => s.Hash));
			return solutionsHashes.ToDictSafe(
				s => s.SubmissionId,
				s => textsByHash.GetOrDefault(s.Hash, ""));
		}
	}

	public class SubmissionCheckingTimeout : Exception
	{
	}
}