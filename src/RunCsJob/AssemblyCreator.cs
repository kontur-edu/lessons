﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CSharp;
using RunCsJob.Api;

namespace RunCsJob
{
	public static class AssemblyCreator
	{
		public static IEnumerable<int> x = Enumerable.Range(1, 1);

		public static CompileResult CreateAssemblyWithRoslyn(FileRunnerSubmission submission, string workingDirectory)
		{
			var currentDirectory = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(workingDirectory);

			IEnumerable<int> x = null;
			var syntaxTree = CSharpSyntaxTree.ParseText(submission.Code);
			var assemblyName = submission.Id;
			var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree },
				new MetadataReference[]
				{
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib
					MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location), // System
					MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Core
					MetadataReference.CreateFromFile(typeof(Point).Assembly.Location), // System.Drawing,
					MetadataReference.CreateFromFile(typeof(ValueTuple).Assembly.Location), // System.ValueTuple
					MetadataReference.CreateFromFile(SystemRuntimeDllPath), // System.Runtime (defines ValueType type),
				}, new CSharpCompilationOptions(OutputKind.ConsoleApplication));
			
			var assemblyFilename = Path.Combine(workingDirectory, assemblyName + ".exe");
			Directory.SetCurrentDirectory(currentDirectory);
			return new CompileResult(compilation.Emit(assemblyFilename), assemblyFilename);
		}
	}

	public class CompileResult
	{
		public CompileResult(EmitResult emitResult, string pathToAssembly)
		{
			EmitResult = emitResult;
			PathToAssembly = pathToAssembly;
		}

		public readonly EmitResult EmitResult;
		public readonly string PathToAssembly;

	}
}