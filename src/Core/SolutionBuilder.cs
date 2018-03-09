using uLearn.Extensions;
using uLearn.Model.Blocks;

namespace uLearn
{
	public class SolutionBuilder
	{
		public SolutionBuilder(int indexForInsert, string exerciseCode, ValidatorDescription validator)
		{
			IndexForInsert = indexForInsert;
			ExerciseCode = exerciseCode;
			Validator = ValidatorsRepository.Get(validator);
		}

		private readonly int IndexForInsert;
		private readonly string ExerciseCode;
		private readonly ISolutionValidator Validator;

		public SolutionBuildResult BuildSolution(string usersExercise)
		{
			var solution = ExerciseCode.Insert(IndexForInsert, usersExercise + "\r\n");
			return Validator.ValidateSingleFileSolution(usersExercise, solution);
		}
	}
}