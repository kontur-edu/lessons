using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Database.DataContexts;
using Database.Models;
using log4net;
using Newtonsoft.Json;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ionic.Zip;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;

namespace uLearn.Web
{
	public class CertificateGenerator
	{
		private readonly UserQuizzesRepo userQuizzesRepo;
		private readonly UserSolutionsRepo userSolutionsRepo;
		private readonly SlideCheckingsRepo slideCheckingsRepo;
		private readonly VisitsRepo visitsRepo;

		private static readonly ILog log = LogManager.GetLogger(typeof(CertificatesRepo));

		public const string TemplateIndexFile = "index.html";
		private readonly Regex templateParameterRegex = new Regex(@"%([-a-z0-9_.]+)(\|(raw|in_quotes|in_html))?%", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private readonly HashSet<string> builtInParameters = new HashSet<string>
		{
			"user.last_name", "user.first_name", "user.name",
			"instructor.last_name", "instructor.first_name", "instructor.name",
			"course.id", "course.title",
			"date", "date.year", "date.month", "date.day",
			"certificate.id", "certificate.url",
			"score",
			"codereviews.passed", "codereviews.passed_maxscore",
			"quizzes.passed", "quizzes.passed_maxscore",
			"exercises.accepted",
		};

		public CertificateGenerator(
			UserQuizzesRepo userQuizzesRepo,
			UserSolutionsRepo userSolutionsRepo,
			SlideCheckingsRepo slideCheckingsRepo,
			VisitsRepo visitsRepo)
		{
			this.userQuizzesRepo = userQuizzesRepo;
			this.userSolutionsRepo = userSolutionsRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.visitsRepo = visitsRepo;
		}

		public CertificateGenerator(ULearnDb db, CourseManager courseManager)
		: this(new UserQuizzesRepo(db),
			new UserSolutionsRepo(db, courseManager),
			new SlideCheckingsRepo(db),
			new VisitsRepo(db))
		{
		}
		
		public FileInfo GetTemplateArchivePath(CertificateTemplate template)
		{
			return GetTemplateArchivePath(template.ArchiveName);
		}

		private static DirectoryInfo GetCertificatesDirectory()
		{
			var certificatesDirectory = ConfigurationManager.AppSettings["ulearn.certificatesDirectory"];
			if (string.IsNullOrEmpty(certificatesDirectory))
				certificatesDirectory = Path.Combine(Utils.GetAppPath(), "Certificates");

			var directory = new DirectoryInfo(certificatesDirectory);
			if (!directory.Exists)
				directory.Create();
			return directory;
		}

		public FileInfo GetTemplateArchivePath(string templateArchiveName)
		{
			return GetCertificatesDirectory().GetFile(templateArchiveName + ".zip");
		}

		public DirectoryInfo GetTemplateDirectory(CertificateTemplate template)
		{
			return GetTemplateDirectory(template.ArchiveName);
		}

		public DirectoryInfo GetTemplateDirectory(string templateArchiveName)
		{
			return GetCertificatesDirectory().GetSubdirectory(templateArchiveName);
		}

		public void EnsureCertificateTemplateIsUnpacked(CertificateTemplate template)
		{
			var certificateDirectory = GetTemplateDirectory(template);
			if (!certificateDirectory.Exists)
			{
				log.Info($"Нет директории с распакованным шаблоном сертификата, Id = {template.Id}");

				var certificateArchive = GetTemplateArchivePath(template);
				if (!certificateArchive.Exists)
					throw new Exception("Can\'t find certificate template");

				log.Info($"Распаковываю шаблон сертификата {template.Id}: \"{certificateArchive.FullName}\" в \"{certificateDirectory.FullName}\"");

				using (var zip = ZipFile.Read(certificateArchive.FullName, new ReadOptions { Encoding = Encoding.UTF8 }))
				{
					zip.ExtractAll(certificateDirectory.FullName, ExtractExistingFileAction.OverwriteSilently);
				}
			}
		}

		public IEnumerable<string> GetTemplateParameters(CertificateTemplate template)
		{
			EnsureCertificateTemplateIsUnpacked(template);

			var templateDirectory = GetTemplateDirectory(template);
			var indexFile = templateDirectory.GetFile(TemplateIndexFile);
			if (!indexFile.Exists)
			{
				log.Error($"Не нашёл файла {TemplateIndexFile} в шаблоне \"{template.Name}\" (Id = {template.Id}, {template.ArchiveName})");
				yield break;
			}

			var foundParameters = new HashSet<string>();

			var matches = templateParameterRegex.Matches(File.ReadAllText(indexFile.FullName));
			foreach (Match match in matches)
			{
				var parameter = match.Groups[1].Value;
				if (!foundParameters.Contains(parameter))
				{
					yield return parameter;
					foundParameters.Add(parameter);
				}
			}
		}

		public IEnumerable<string> GetTemplateParametersWithoutBuiltins(CertificateTemplate template)
		{
			return GetTemplateParameters(template).Where(p => !builtInParameters.Contains(p)).Distinct();
		}

		public IEnumerable<string> GetBuiltinTemplateParameters(CertificateTemplate template)
		{
			return GetTemplateParameters(template).Where(p => builtInParameters.Contains(p)).Distinct();
		}

		public string GetTemplateBuiltinParameterForUser(CertificateTemplate template, Course course, ApplicationUser user, ApplicationUser instructor, string parameterName)
		{
			var mockCertificate = new Certificate
			{
				Id = Guid.Empty,
				User = user,
				UserId = user.Id,
				Instructor = instructor,
				InstructorId = instructor.Id,
				Template = template,
				TemplateId = template.Id,
				Timestamp = DateTime.Now,
			};
			return SubstituteBuiltinParameters($"%{parameterName}|raw%", mockCertificate, course, "<адрес сертификата>");
		}

		public string RenderCertificate(Certificate certificate, Course course, string certificateUrl)
		{
			var templateDirectory = GetTemplateDirectory(certificate.Template);
			var indexFile = templateDirectory.GetFile(TemplateIndexFile);
			var content = File.ReadAllText(indexFile.FullName);

			return SubstituteParameters(content, certificate, course, certificateUrl);
		}

		private string SubstituteOneParameter(string content, string parameterName, string parameterValue)
		{
			if (parameterValue == null)
				parameterValue = "";

			content = content.Replace($"%{parameterName}|raw%", parameterValue);

			var htmlEncodedValue = parameterValue.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;");
			content = content.Replace($"%{parameterName}|in_html%", htmlEncodedValue);
			content = content.Replace($"%{parameterName}%", htmlEncodedValue);

			var quotesEncodedValue = parameterValue.EncodeQuotes();
			content = content.Replace($"%{parameterName}|in_quotes%", quotesEncodedValue);

			return content;
		}

		private string SubstituteOneBoolParameter(string content, string parameterName, bool parameterValue)
		{
			return Regex.Replace(
				content,
				$"%{parameterName}\\|(?<true>[^|%]+)\\|(?<false>[^|%]+)%",
				m => parameterValue ? m.Groups["true"].Value : m.Groups["false"].Value
			);
		}

		private string SubstituteParameters(string content, Certificate certificate, Course course, string certificateUrl)
		{
			var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(certificate.Parameters);
			foreach (var kv in parameters)
			{
				content = SubstituteOneParameter(content, kv.Key, kv.Value);
			}

			content = SubstituteBuiltinParameters(content, certificate, course, certificateUrl);

			return content;
		}

		private string SubstituteBuiltinParameters(string content, Certificate certificate, Course course, string certificateUrl)
		{
			content = ReplaceBasicBuiltinParameters(content, certificate, course, certificateUrl);

			/* Replace %score% for total course score */
			var userScore = visitsRepo.GetScoresForSlides(course.Id, certificate.UserId).Sum(p => p.Value);
			content = SubstituteOneParameter(content, "score", userScore.ToString());

			/* Replace %codereviews.*% */
			content = ReplaceCodeReviewsBuiltinParameters(content, certificate, course);
			/* Replace %quizzes.*% */
			content = ReplaceQuizzesBuiltinParameters(content, certificate, course);

			var acceptedSolutionsCount = userSolutionsRepo.GetAllAcceptedSubmissionsByUser(course.Id, course.Slides.Select(s => s.Id), certificate.UserId).Select(s => s.SlideId).Distinct().Count();
			content = SubstituteOneParameter(content, "exercises.accepted", acceptedSolutionsCount.ToString());

			return content;
		}

		private string ReplaceBasicBuiltinParameters(string content, Certificate certificate, Course course, string certificateUrl)
		{
			content = SubstituteOneParameter(content, "user.first_name", certificate.User.FirstName);
			content = SubstituteOneParameter(content, "user.last_name", certificate.User.LastName);
			content = SubstituteOneParameter(content, "user.name", certificate.User.VisibleName);

			content = SubstituteOneParameter(content, "instructor.first_name", certificate.Instructor.FirstName);
			content = SubstituteOneParameter(content, "instructor.last_name", certificate.Instructor.LastName);
			content = SubstituteOneParameter(content, "instructor.name", certificate.Instructor.VisibleName);

			content = SubstituteOneParameter(content, "course.id", course.Id);
			content = SubstituteOneParameter(content, "course.title", course.Title);

			content = SubstituteOneParameter(content, "date", certificate.Timestamp.ToLongDateString());
			content = SubstituteOneParameter(content, "date.day", certificate.Timestamp.Day.ToString());
			content = SubstituteOneParameter(content, "date.month", certificate.Timestamp.Month.ToString("D2"));
			content = SubstituteOneParameter(content, "date.year", certificate.Timestamp.Year.ToString());

			content = SubstituteOneParameter(content, "certificate.id", certificate.Id.ToString());
			content = SubstituteOneParameter(content, "certificate.url", certificateUrl);

			content = SubstituteOneBoolParameter(content, "by_gender", !certificate.User.Gender.HasValue || certificate.User.Gender == Gender.Male);

			return content;
		}

		private string ReplaceQuizzesBuiltinParameters(string content, Certificate certificate, Course course)
		{
			var passedQuizzesCount = userQuizzesRepo.GetPassedSlideIds(course.Id, certificate.UserId).Count;
			var scoredMaximumQuizzesCount = userQuizzesRepo.GetPassedSlideIdsWithMaximumScore(course.Id, certificate.UserId).Count;

			content = SubstituteOneParameter(content, "quizzes.passed", passedQuizzesCount.ToString());
			content = SubstituteOneParameter(content, "quizzes.passed_maxscore", scoredMaximumQuizzesCount.ToString());
			return content;
		}

		private string ReplaceCodeReviewsBuiltinParameters(string content, Certificate certificate, Course course)
		{
			var codeReviewsCount = slideCheckingsRepo.GetUsersPassedManualExerciseCheckings(course.Id, certificate.UserId).Count();
			var exercisesMaxReviewScores = course.Slides
				.OfType<ExerciseSlide>().
				ToDictionary(s => s.Id, s => s.Scoring.CodeReviewScore);
			var codeReviewsFullCount = slideCheckingsRepo
				.GetUsersPassedManualExerciseCheckings(course.Id, certificate.UserId)
				.Count(s => s.Score == exercisesMaxReviewScores.GetOrDefault(s.SlideId, -1));

			content = SubstituteOneParameter(content, "codereviews.passed", codeReviewsCount.ToString());
			content = SubstituteOneParameter(content, "codereviews.passed_maxscore", codeReviewsFullCount.ToString());
			return content;
		}
	}
}