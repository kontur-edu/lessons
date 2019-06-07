using System.Collections.Generic;
using Ulearn.Core.CSharp;

namespace Ulearn.Core
{
	public interface ISolutionValidator
	{
		///<summary>Проверка того, что код не является полным исходником</summary>>
		string FindFullSourceError(string userCode);

		///<summary>Проверка всего решения на компилируемость</summary>
		string FindSyntaxError(string solution);

		///<summary>Проверка валидаторами кода, который написал студент.
		/// Если решение верное и валидаторами найдены стилевые ошибки, задача принимается, а студенту показывается сообщение с ошибками</summary>
		List<SolutionStyleError> FindValidatorErrors(string userCode, string solution);

		///<summary>Проверка валидаторами кода, который написал студент.
		/// Если решение верное и валидаторами найдены стилевые ошибки, задача не принимается, т.к. студент не решил ее требущимся способом</summary>
		string FindStrictValidatorErrors(string userCode, string solution);
	}
}