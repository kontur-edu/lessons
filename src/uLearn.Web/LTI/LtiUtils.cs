using System;
using Database.DataContexts;
using Database.Models;
using log4net;
using LtiLibrary.Core.Outcomes.v1;
using uLearn.Web.Controllers;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;

namespace uLearn.Web.LTI
{
	public static class LtiUtils
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LtiUtils));

		public static void SubmitScore(string courseId, Slide slide, string userId, Visit visit = null)
		{
			var db = new ULearnDb();
			var ltiRequestsRepo = new LtiRequestsRepo(db);
			var consumersRepo = new ConsumersRepo(db);
			var visitsRepo = new VisitsRepo(db);

			var ltiRequest = ltiRequestsRepo.Find(courseId, userId, slide.Id);
			if (ltiRequest == null)
				throw new Exception("LtiRequest for user '" + userId + "' not found");

			var consumerSecret = consumersRepo.Find(ltiRequest.ConsumerKey).Secret;

			var score = visit?.Score ?? visitsRepo.GetScore(courseId, slide.Id, userId);

			log.Info($"Надо отправить результаты слайда {slide.Id} пользователя {userId} по LTI. Нашёл LtiRequest: {ltiRequest.JsonSerialize()}");
			UriBuilder uri;
			try
			{
				uri = new UriBuilder(ltiRequest.LisOutcomeServiceUrl);
			}
			catch (Exception e)
			{
				log.Error($"Неверный адрес отправки результатов по LTI: {ltiRequest.LisOutcomeServiceUrl}", e);
				throw;
			}
			if (uri.Host == "localhost")
			{
				uri.Host = "192.168.33.10";
				uri.Port = 80;
				uri.Scheme = "http";
			}

			var maxScore = ControllerUtils.GetMaxScoreForUsersSlide(slide, true, false, false);
			var outputScore = score / (double)maxScore;
			log.Info($"Отправляю результаты на {ltiRequest.LisOutcomeServiceUrl}: {score} из {maxScore} ({outputScore})");

			/* Sometimes score is bigger then slide's MaxScore, i.e. in case of manual checking */
			if (score > maxScore)
				outputScore = 1;
			var result = OutcomesClient.PostScore(uri.ToString(), ltiRequest.ConsumerKey, consumerSecret,
				ltiRequest.LisResultSourcedId, outputScore);

			if (!result.IsValid)
				throw new Exception(uri + "\r\n\r\n" + result.Message);
		}
	}
}