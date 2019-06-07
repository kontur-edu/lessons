using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;
using Ulearn.VideoAnnotations.Api.Models.Responses.Annotations;

namespace Ulearn.VideoAnnotations.Web.Annotations
{
	public class AnnotationsParser : IAnnotationsParser
	{
		private const string fragmentPattern = @"^\*\s+(\d?\d\:\d\d)\s+([\-\—\–]\s+)?(.+)";
		private readonly Regex fragmentRegex = new Regex(fragmentPattern);

		private readonly string[] annotationOffsetFormats = { @"mm\:ss", @"m\:ss" };
		
		private readonly ILogger logger;

		public AnnotationsParser(ILogger logger)
		{
			this.logger = logger;
		}

		public Dictionary<string, Annotation> ParseAnnotations(string[] lines)
		{
			var i = 0;
			while (!lines[i].StartsWith("#"))
				i++;

			var result = new Dictionary<string, Annotation>();
			
			while (i < lines.Length)
			{
				var title = lines[i];
				i++;
				var annotationLines = new List<string>();
				while (i < lines.Length && !lines[i].StartsWith("#"))
				{
					annotationLines.Add(lines[i]);
					i++;
				}

				if (annotationLines.Count == 0)
				{
					logger.Warning("Empty annotation for slide \"{title}\". I will skip it", title);
					continue;
				}

				var videoId = annotationLines[0].Trim();
				logger.Information("Parse slide {title}, video id is {videoId}", title, videoId);
				
				if (annotationLines.Count(line => line.Trim().StartsWith("*")) == 0)
				{
					logger.Warning("Not found time-codes for slide \"{title}\". I will skip it", title);
					continue;
				}
				if (annotationLines.Count(line => !line.Trim().StartsWith("*")) == 0)
				{
					logger.Warning("Not found abstract for slide \"{title}\". I will skip it", title);
					continue;
				}
				if (annotationLines.TakeWhile(line => !line.Trim().StartsWith("*")).Count() != 2)
				{
					logger.Warning("Abstract can not be multiline. I will skip slide \"{title}\"", title);
					continue;
				}
				if (annotationLines.Count(line => line.Trim().StartsWith("*")) == annotationLines.Count - 1)
				{
					logger.Warning("Bad annotation for slide \"{title}\". I will skip it", title);
					continue;
				}

				var text = annotationLines[1];
				var fragments = annotationLines.Skip(2).Select(ParseFragment).ToList();
				if (fragments.Any(f => f == null))
					continue;

				result[videoId] = new Annotation
				{
					Text = text,
					Fragments = fragments.Where(f => f != null).ToList(),
				};
			}

			return result;
		}
		
		private AnnotationFragment ParseFragment(string line)
		{
			var match = fragmentRegex.Match(line);
			if (!match.Success)
				return null;

			var text = match.Groups[3].Value;
			var offsetString = match.Groups[1].Value;
			
			if (!TimeSpan.TryParseExact(offsetString, annotationOffsetFormats, CultureInfo.InvariantCulture, out var offset))
			{
				logger.Warning("Can't parse time-code {offset}, skip it", offsetString);
				return null;
			}
			
			return new AnnotationFragment
			{
				Offset = offset,
				Text = text,
			};
		}
	}

	public class AnnotationParsingException : Exception
	{
		public AnnotationParsingException()
		{
		}

		public AnnotationParsingException(string message)
			: base(message)
		{
		}
	}
}