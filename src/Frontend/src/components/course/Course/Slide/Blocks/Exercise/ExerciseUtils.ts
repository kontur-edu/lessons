import {
	AutomaticExerciseCheckingResult,
	AutomaticExerciseCheckingResult as CheckingResult,
	SolutionRunStatus,
	SubmissionInfo
} from "src/models/exercise";

enum SubmissionColor {
	MaxResult = "MaxResult", // Студенту больше ничего не может сделать, ни сийчам ни в будущем
	NeedImprovements = "NeedImprovements", // Студент может доработать задачу сейчас или в будущем
	WrongAnswer = "WrongAnswer", // Тесты не пройдены или ошибка компиляции, показывается даже в старых версиях
	Message = "Message", // Сообщение, ни на что не влияющее, например, старая версия
}

function GetSubmissionColor(
	solutionRunStatus: SolutionRunStatus | undefined,
	checkingResult: CheckingResult | undefined, // undefined если automaticChecking null
	hasSuccessSolution: boolean, // Задача прошла автопроверку или автопроверки нет?
	selectedSubmissionIsLast: boolean, // Это последнее решение, прошедшее тесты?
	selectedSubmissionIsLastSuccess: boolean, // Это последнее решение, прошедшее тесты?
	prohibitFurtherManualChecking: boolean,
	isSkipped: boolean,
	isMaxScore: boolean, // Балл студента равен максимальному за задачу
): SubmissionColor {
	if(solutionRunStatus === SolutionRunStatus.CompilationError
		|| checkingResult === CheckingResult.CompilationError || checkingResult === CheckingResult.WrongAnswer || checkingResult == CheckingResult.RuntimeError) {
		return SubmissionColor.WrongAnswer;
	}
	if(isSkipped) {
		return selectedSubmissionIsLast ? SubmissionColor.MaxResult : SubmissionColor.Message;
	}
	if(selectedSubmissionIsLastSuccess) {
		return !isMaxScore && !prohibitFurtherManualChecking
			? SubmissionColor.NeedImprovements
			: SubmissionColor.MaxResult;
	}
	return selectedSubmissionIsLast && !isMaxScore && !prohibitFurtherManualChecking
		? SubmissionColor.NeedImprovements
		: SubmissionColor.Message;
}

function IsSuccessSubmission(submission: SubmissionInfo | null): boolean {
	return !!submission && (submission.automaticChecking == null || submission.automaticChecking.result === CheckingResult.RightAnswer);
}

function HasSuccessSubmission(submissions: SubmissionInfo[]): boolean {
	return submissions.some(IsSuccessSubmission);
}

function SubmissionIsLast(submissions: SubmissionInfo[], submission: SubmissionInfo | null): boolean {
	return submissions.length > 0 && submissions[0] === submission;
}

function GetLastSuccessSubmission(submissions: SubmissionInfo[]): SubmissionInfo | null {
	const successSubmissions = submissions.filter(IsSuccessSubmission);
	if(successSubmissions.length > 0) {
		return successSubmissions[0];
	}
	return null;
}

function IsFirstRightAnswer(submissions: SubmissionInfo[], successSubmission: SubmissionInfo): boolean {
	const successSubmissions = submissions.filter(IsSuccessSubmission);
	return successSubmissions.length > 0 && successSubmissions[successSubmissions.length - 1] === successSubmission;
}

function isSubmissionShouldBeEditable(submission: SubmissionInfo): boolean {
	return submission.automaticChecking?.result !== AutomaticExerciseCheckingResult.RightAnswer && submission.automaticChecking?.result !== AutomaticExerciseCheckingResult.NotChecked;
}

export {
	GetSubmissionColor,
	SubmissionColor,
	HasSuccessSubmission,
	SubmissionIsLast,
	GetLastSuccessSubmission,
	IsFirstRightAnswer,
	isSubmissionShouldBeEditable,
};
