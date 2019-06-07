using System;
using System.Collections.Generic;
using AntiPlagiarism.Web.CodeAnalyzing.CSharp;
using AntiPlagiarism.Web.Configuration;
using AntiPlagiarism.Web.Database.Models;
using Microsoft.Extensions.Options;
using Serilog;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.Web.CodeAnalyzing
{
	public class SubmissionSnippetsExtractor
	{
		private readonly CodeUnitsExtractor codeUnitsExtractor;
		private readonly SnippetsExtractor snippetsExtractor;
		private readonly ILogger logger;
		private readonly AntiPlagiarismConfiguration configuration;
		
		private readonly List<ITokenInSnippetConverter> tokenConverters = new List<ITokenInSnippetConverter>
		{
			new TokensKindsOnlyConverter(),
			new TokensKindsAndValuesConverter(),
		};

		public SubmissionSnippetsExtractor(CodeUnitsExtractor codeUnitsExtractor, SnippetsExtractor snippetsExtractor, ILogger logger, IOptions<AntiPlagiarismConfiguration> options)
		{
			this.codeUnitsExtractor = codeUnitsExtractor;
			this.snippetsExtractor = snippetsExtractor;
			this.logger = logger;
			configuration = options.Value;
		}
		
		public IEnumerable<Tuple<int, Snippet>> ExtractSnippetsFromSubmission(Submission submission)
		{
			logger.Information("Достаю сниппеты из решения {submissionId}, длина сниппетов: {tokensCount} токенов", submission.Id, configuration.SnippetTokensCount);
			var codeUnits = codeUnitsExtractor.Extract(submission.ProgramText);
			foreach (var codeUnit in codeUnits)
			{
				foreach (var tokenConverter in tokenConverters)
				{
					var snippets = snippetsExtractor.GetSnippets(codeUnit.Tokens, configuration.SnippetTokensCount, tokenConverter);
					foreach (var (index, snippet) in snippets.Enumerate())
					{
						yield return Tuple.Create(codeUnit.FirstTokenIndex + index, snippet);
					}
				}
			}
		}
	}
}