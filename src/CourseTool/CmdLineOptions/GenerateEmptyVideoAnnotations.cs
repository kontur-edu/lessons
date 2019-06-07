using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CommandLine;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("generate-annotations", HelpText = "Generate empty video-annotations for inserting in google-doc")]
	class GenerateEmptyVideoAnnotations : AbstractOptions
	{
		public override void DoExecute()
		{
			var course = new CourseLoader().Load(new DirectoryInfo(Path.Combine(Dir, Config.ULearnCourseId)));
			Console.WriteLine($"{course.Slides.Count} slide(s) have been loaded from {Config.ULearnCourseId}");

			var resultHtmlFilename = $"{Config.ULearnCourseId}.annotations.html";
			using (var writer = new StreamWriter(resultHtmlFilename))
			{
				writer.WriteLine($"<html><head><title>{course.Title}</title><style>.videoId {{ color: #999; font-size: 12px; }}</style></head><body>");
				foreach (var slide in course.Slides)
				{
					var videoBlocks = slide.Blocks.OfType<YoutubeBlock>();
					foreach (var videoBlock in videoBlocks)
					{
						writer.WriteLine($"<h1># {slide.Title.EscapeHtml()}</h1>");
						writer.WriteLine($"<span class='videoId'>{videoBlock.VideoId.EscapeHtml()}</span>");
					}
				}
				writer.WriteLine("</body></html>");
			}

			Console.WriteLine($"HTML has been generated and saved to {resultHtmlFilename}.");
			Console.WriteLine("Open this file in browser and copy its content to freshly created google doc.");
			Console.WriteLine("After it set this google doc's id (i.e. 1oJy12bkv2uyAMFBXEKRnNDIkR930-Z6X1dqR7CPuRkP) as <videoAnnotationsGoogleDoc>...</videoAnnotationsGoogleDoc>");
		}
	}
}