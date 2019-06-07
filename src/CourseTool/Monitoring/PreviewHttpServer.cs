using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using NHttp;
using RunCsJob;
using RunCsJob.Api;
using uLearn.Web.Models;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Helpers;

namespace uLearn.CourseTool.Monitoring
{
	internal class PreviewHttpServer
	{
		private readonly HttpServer server;
		private readonly string courseDir;
		private readonly string htmlDir;
		private readonly int port;
		private DateTime lastChangeTime = DateTime.MinValue;
		private volatile Course course;
		private readonly object locker = new object();
		private readonly ExerciseStudentZipBuilder exerciseStudentZipBuilder = new ExerciseStudentZipBuilder();

		private const string exerciseStudentZipPath = "/Exercise/StudentZip";

		public PreviewHttpServer(string courseDir, string htmlDir, int port)
		{
			server = new HttpServer
			{
				EndPoint = new IPEndPoint(IPAddress.Loopback, port),
			};
			this.courseDir = courseDir;
			this.htmlDir = htmlDir;
			this.port = port;
			CopyStaticToHtmlDir();
		}

		private void CopyStaticToHtmlDir()
		{
			if (!Directory.Exists(htmlDir))
				Directory.CreateDirectory(htmlDir);
			var staticDir = Path.Combine(htmlDir, "static");
			if (!Directory.Exists(staticDir))
				Directory.CreateDirectory(staticDir);
			Utils.DirectoryCopy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "renderer"), htmlDir, true);
		}

		public string FindLastChangedSlideHtmlPath()
		{
			var files = Directory.GetFiles(htmlDir, "*.html");
			if (files.Length == 0)
				return null;
			return files
				.Select(fn => Tuple.Create(File.GetLastWriteTime(fn), Path.GetFileName(fn)))
				.Max().Item2;
		}

		public void Start()
		{
			try
			{
				server.RequestReceived += OnHttpRequest;
				server.UnhandledException += OnException;
				server.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine($"HttpListener Start Error: {e.Message}");
				Console.WriteLine();
				Console.WriteLine(@"On 'access is denied' error do one of the following:");
				Console.WriteLine(@"1. Run this application with admin rights.");
				Console.WriteLine(@"2. OR run this command in command line ('Everyone' may be some specific user):");
				Console.WriteLine($"   netsh http add urlacl url=http://+:{port}/ user=Everyone");
			}
		}

		private void OnException(object sender, HttpExceptionEventArgs e)
		{
			e.Handled = true;
			var buffer = Encoding.UTF8.GetBytes(e.Exception.ToString());
			e.Response.ContentType = "text/plain";
			e.Response.OutputStream.Write(buffer, 0, buffer.Length);
		}

		public void MarkCourseAsChanged()
		{
			lock (locker)
				lastChangeTime = DateTime.Now;
		}

		private void OnHttpRequest(object sender, HttpRequestEventArgs context)
		{
			var query = context.Request.QueryString["query"];
			var path = context.Request.Url.LocalPath;
			var requestTime = DateTime.Now;
			var reloaded = ReloadCourseIfChanged(requestTime);
			if (!new[] { ".js", ".css", ".png", ".jpg", ".woff" }.Any(ext => path.EndsWith(ext)))
				Console.WriteLine($"{requestTime:T} {context.Request.HttpMethod} {context.Request.Url}");

			byte[] responseBytes;			
			/* Serve exercise student zips */
			if (path == exerciseStudentZipPath)
				responseBytes = ServeExerciseStudentZip(context, context.Request.QueryString["slideId"]);
			else
				switch (query)
				{
					case "needRefresh":
						responseBytes = ServeNeedRefresh(reloaded, requestTime).Result;
						break;
					case "submit":
						responseBytes = ServeRunExercise(context, path);
						break;
					case "addLesson":
						responseBytes = ServeAddLesson(context, path);
						break;
					case "addQuiz":
						responseBytes = ServeAddQuiz(context, path);
						break;
					default:
						responseBytes = ServeStatic(context, path);
						break;
				}

			context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length).Wait();
			context.Response.OutputStream.Close();
		}

		private byte[] ServeExerciseStudentZip(HttpRequestEventArgs context, string slideId)
		{
			if (!Guid.TryParse(slideId, out var slideIdGuid))
				return Encoding.UTF8.GetBytes("Invalid slide id");
			var slide = course.FindSlideById(slideIdGuid);
			if (!(slide is ExerciseSlide))
				return Encoding.UTF8.GetBytes("Invalid slide id");
			
			var zipBytes = GenerateExerciseStudentZip(slide);
			context.Response.Headers.Add("Content-Type", "application/zip");
			var projectExerciseBlock = ((slide as ExerciseSlide).Exercise as CsProjectExerciseBlock);
			context.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{projectExerciseBlock?.ExerciseDirName.ToLatin()}.zip\"");
			
			return zipBytes;
		}

		private byte[] GenerateExerciseStudentZip(Slide slide)
		{
			var tempZipFile = Path.GetRandomFileName();
			var tempZipFileInfo = new FileInfo(tempZipFile);

			try
			{
				exerciseStudentZipBuilder.BuildStudentZip(slide, tempZipFileInfo);
				return File.ReadAllBytes(tempZipFile);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.ToString());
				throw;
			}
			finally
			{
				tempZipFileInfo.Delete();
			}
		}

		public int GetSlideIndex(string path)
		{
			return int.Parse(path.Substring(1, 3));
		}

		private byte[] ServeAddQuiz(HttpRequestEventArgs context, string path)
		{
			var quiz = new QuizSlide
			{
				Id = Guid.NewGuid(),
				Title = "Новый quiz"
			};
			return AddNewSlide(context, path, quiz);
		}

		private byte[] ServeAddLesson(HttpRequestEventArgs context, string path)
		{
			var lesson = new Slide(new MarkdownBlock("текст"))
			{
				Id = Guid.NewGuid(),
				Title = "Новый слайд",
			};
			return AddNewSlide(context, path, lesson);
		}

		private byte[] AddNewSlide<TSlide>(HttpRequestEventArgs context, string path, TSlide lessonOrQuiz) where TSlide : Slide
		{
			var prevSlide = course.FindSlideByIndex(GetSlideIndex(path));
			if (prevSlide == null)
			{
				context.Response.StatusCode = 404;
				return new byte[0];
			}

			lessonOrQuiz.DefineBlockTypes();
			
			var serializer = new XmlSerializer(typeof(TSlide));
			var newFile = GenerateSlideFilename<TSlide>(prevSlide);
			using (var s = new FileStream(Path.Combine(prevSlide.Info.Directory.FullName, newFile), FileMode.OpenOrCreate))
				serializer.Serialize(s, lessonOrQuiz);
			ReloadCourse();
			context.Response.Redirect((prevSlide.Index + 1).ToString("000") + ".html");
			return new byte[0];
		}

		private static string GenerateSlideFilename<T>(Slide prevSlide)
		{
			var filename = prevSlide.Info.SlideFile.Name;
			
			var match = Regex.Match(filename, @"^(\w?)([0-9]+)(.+)$");
			if (!match.Success)
				return filename.Remove(filename.Length - prevSlide.Info.SlideFile.Extension.Length) + "_next.xml";

			var prefix = match.Groups[1].Value;
			var num = int.Parse(match.Groups[2].Value);
			return prefix + (num + 1) + ".xml";
		}

		private async Task<byte[]> ServeNeedRefresh(bool reloaded, DateTime requestTime)
		{
			var sw = Stopwatch.StartNew();
			while (true)
			{
				if (reloaded || sw.Elapsed > TimeSpan.FromSeconds(20))
				{
					Console.WriteLine($@"needRefresh:{reloaded}, LastChanged:{lastChangeTime}");
					return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reloaded));
				}
				await Task.Delay(1000).ConfigureAwait(false);
				reloaded = ReloadCourseIfChanged(requestTime);
			}
		}

		private byte[] ServeRunExercise(HttpRequestEventArgs context, string path)
		{
			var code = context.Request.InputStream.GetString();
			var index = GetSlideIndex(path);
			var exercise = ((ExerciseSlide)course.Slides[index]).Exercise;
			var runResult = GetRunResult(exercise, code);
			context.Response.ContentType = "application/json; charset=utf-8";
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(runResult));
		}

		private static RunSolutionResult GetRunResult(AbstractExerciseBlock exercise, string code)
		{
			var buildResult = exercise.BuildSolution(code);
			if (buildResult.HasErrors)
				return new RunSolutionResult { IsCompileError = true, ErrorMessage = buildResult.ErrorMessage, ExecutionServiceName = "uLearn" };
			var result = SandboxRunner.Run(exercise.CreateSubmission(Utils.NewNormalizedGuid(), code));
			var runSolutionResult = new RunSolutionResult
			{
				IsRightAnswer = exercise.IsCorrectRunResult(result),
				ActualOutput = result.GetOutput()?.NormalizeEoln() ?? "",
				ErrorMessage = result.CompilationOutput,
				ExecutionServiceName = "course.exe",
				IsCompileError = result.Verdict == Verdict.CompilationError,
				ExpectedOutput = exercise.ExpectedOutput?.NormalizeEoln() ?? "",
				SubmissionId = 0,
			};
			if (buildResult.HasStyleErrors)
			{
				runSolutionResult.IsStyleViolation = true;
				runSolutionResult.StyleMessage = string.Join("\n", buildResult.StyleErrors.Select(e => e.GetMessageWithPositions()));
			}
			return runSolutionResult;
		}

		private byte[] ServeStatic(HttpRequestEventArgs context, string path)
		{
			try
			{
				context.Response.ContentType = null;
				/*
				Explanation: 
				Default ContentType in NHttp is text/html. It is better to make browser guess it!

				RFC2616:
				«Any HTTP/1.1 message containing an entity-body SHOULD include a
				Content-Type header field defining the media type of that body. If
				and only if the media type is not given by a Content-Type field, the
				recipient MAY attempt to guess the media type via inspection of its
				content and/or the name extension(s) of the URI used to identify the
				resource. If the media type remains unknown, the recipient SHOULD
				treat it as type "application/octet-stream"»
				*/
				return File.ReadAllBytes(htmlDir + "/" + path);
			}
			catch (IOException e)
			{
				context.Response.StatusCode = 404;
				context.Response.ContentType = "text/plain; charset=utf-8";
				return Encoding.UTF8.GetBytes(e.ToString());
			}
		}

		public void ForceReloadCourse()
		{
			lock (locker)
			{
				course = ReloadCourse();
				Console.WriteLine($"Course reloaded. LastChangeTime: {lastChangeTime}");
			}
		}

		Course ReloadCourse()
		{
			var loadedCourse = new CourseLoader().Load(new DirectoryInfo(courseDir));
			var renderer = new SlideRenderer(new DirectoryInfo(htmlDir), loadedCourse);
			foreach (var slide in loadedCourse.Slides)
				renderer.RenderSlideToFile(slide, htmlDir);
			foreach (var unit in loadedCourse.Units.Where(u => u.InstructorNote != null))
				renderer.RenderInstructorNotesToFile(unit, htmlDir);
			return loadedCourse;
		}

		private bool ReloadCourseIfChanged(DateTime requestTime)
		{
			lock (locker)
			{
				// Именно так. Предотвращает частую перезагрузку. Все должно обновляться за счет needrefresh.
				var needReload = lastChangeTime > requestTime.Add(TimeSpan.FromMilliseconds(500));
				if (needReload || course == null)
				{
					course = ReloadCourse();
					Console.WriteLine($"Course reloaded. LastChangeTime: {lastChangeTime}");
				}
				return needReload;
			}
		}
	}
}