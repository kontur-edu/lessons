using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RunCsJob.Api;

namespace RunCsJob
{
	public static class AssemblyCreator
	{
		public static CompileResult CreateAssemblyWithRoslyn(FileRunnerSubmission submission, string workingDirectory, TimeSpan compilationTimeLimit)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(submission.Code);
			var assemblyName = submission.Id;

			var compilation = CSharpCompilation.Create(
				assemblyName, new[] { syntaxTree },
				new MetadataReference[]
				{
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib
					MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location), // System
					MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Core
					MetadataReference.CreateFromFile(typeof(Point).Assembly.Location), // System.Drawing,
					MetadataReference.CreateFromFile(typeof(ValueType).Assembly.Location), // System.Runtime
				}, new CSharpCompilationOptions(OutputKind.ConsoleApplication));
			
			var assemblyFilename = Path.Combine(workingDirectory, assemblyName + ".exe");
			
			using (var cts = new CancellationTokenSource(compilationTimeLimit))
			{
				var startTime = DateTime.Now;
				var emitResult = compilation.Emit(assemblyFilename, cancellationToken: cts.Token);
				return new CompileResult(emitResult, assemblyFilename, DateTime.Now - startTime);
			}
		}
	}

	public class CompileResult
	{
		public readonly EmitResult EmitResult;
		public readonly string PathToAssembly;
		public readonly TimeSpan Elapsed;

		public CompileResult(EmitResult emitResult, string pathToAssembly, TimeSpan elapsed)
		{
			EmitResult = emitResult;
			PathToAssembly = pathToAssembly;
			Elapsed = elapsed;
		}
	}
}