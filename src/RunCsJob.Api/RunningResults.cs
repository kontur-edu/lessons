namespace RunCsJob.Api
{
	public class RunningResults
	{
		public string Id { get; set; }

		public Verdict Verdict { get; set; }

		public string CompilationOutput { get; set; }

		public string Output { get; set; }

		public string Error { get; set; }

		public RunningResults()
		{
			/* It's need to be here for model unpacking in API queries */
		}

		public RunningResults(string id, Verdict verdict, string compilationOutput = "", string output = "", string error = "")
		{
			Id = id;
			Verdict = verdict;
			CompilationOutput = compilationOutput;
			Output = output;
			Error = error;
		}

		public RunningResults(Verdict verdict, string compilationOutput = "", string output = "", string error = "")
		{
			Verdict = verdict;
			CompilationOutput = compilationOutput;
			Output = output;
			Error = error;
		}

		public override string ToString()
		{
			return $"Id: {Id}, Verdict: {Verdict}: {(Verdict == Verdict.SandboxError ? Error : Verdict == Verdict.CompilationError ? CompilationOutput : Output)}";
		}

		public string GetOutput()
		{
			var output = Output;
			if (!string.IsNullOrEmpty(Error))
				output += "\n" + Error;

			switch (Verdict)
			{
				case Verdict.Ok:
					return output;
				case Verdict.TimeLimit:
					return output + "\n Ваше решение не успело пройти все тесты за 10 секунд";
				default:
					return output + "\n" + Verdict;
			}
		}
	}
}