import { Dispatch } from "redux";

import { deleteCodeReviewComment, sendCodeReviewComment, submitCode } from "src/api/exercise";

import { Language } from "src/consts/languages";
import {
	EXERCISE_ADD_SUBMISSION,
	EXERCISE_DELETE_REVIEW_COMMENT_START,
	EXERCISE_DELETE_REVIEW_COMMENT_SUCCESS,
	EXERCISE_DELETE_REVIEW_COMMENT_FAIL,

	EXERCISE_ADD_REVIEW_COMMENT_START,
	EXERCISE_ADD_REVIEW_COMMENT_SUCCESS,
	EXERCISE_ADD_REVIEW_COMMENT_FAIL,
	ExerciseAction,
} from "src/actions/slides.types";
import { ReviewCommentResponse, RunSolutionResponse, SolutionRunStatus, } from "src/models/exercise";
import { userProgressUpdateAction } from "src/actions/userProgress";

export const addSubmissionAction = (
	courseId: string,
	slideId: string,
	result: RunSolutionResponse
): ExerciseAction => ({
	type: EXERCISE_ADD_SUBMISSION,
	courseId,
	slideId,
	result,
});

export const addReviewCommentStartAction = (courseId: string, slideId: string, submissionId: number,
	reviewId: number,
): ExerciseAction => ({
	type: EXERCISE_ADD_REVIEW_COMMENT_START,
	courseId,
	slideId,
	submissionId,
	reviewId,
});

export const addReviewCommentSuccessAction = (
	courseId: string,
	slideId: string,
	submissionId: number,
	reviewId: number,
	comment: ReviewCommentResponse
): ExerciseAction => ({
	type: EXERCISE_ADD_REVIEW_COMMENT_SUCCESS,
	courseId,
	slideId,
	submissionId,
	reviewId,
	comment,
});

export const addReviewCommentFailAction = (
	courseId: string,
	slideId: string,
	submissionId: number,
	reviewId: number,
	error: string
): ExerciseAction => ({
	type: EXERCISE_ADD_REVIEW_COMMENT_FAIL,
	courseId,
	slideId,
	submissionId,
	reviewId,
	error,
});

export const deleteReviewCommentStart = (
	courseId: string,
	slideId: string,
	submissionId: number,
	reviewId: number,
	commentId: number
): ExerciseAction => ({
	type: EXERCISE_DELETE_REVIEW_COMMENT_START,
	courseId,
	slideId,
	submissionId,
	reviewId,
	commentId,
});

export const deleteReviewCommentSuccess = (
	courseId: string,
	slideId: string,
	submissionId: number,
	reviewId: number,
	commentId: number
): ExerciseAction => ({
	type: EXERCISE_DELETE_REVIEW_COMMENT_SUCCESS,
	courseId,
	slideId,
	submissionId,
	reviewId,
	commentId,
});


export const deleteReviewCommentFail = (
	courseId: string,
	slideId: string,
	submissionId: number,
	reviewId: number,
	commentId: number,
	error: string
): ExerciseAction => ({
	type: EXERCISE_DELETE_REVIEW_COMMENT_FAIL,
	courseId,
	slideId,
	submissionId,
	reviewId,
	commentId,
	error,
});

export const sendCode = (
	courseId: string,
	slideId: string,
	code: string,
	language: Language,
): (dispatch: Dispatch) => void => {
	return (dispatch: Dispatch) => {
		submitCode(courseId, slideId, code, language)
			.then(r => {
				dispatch(addSubmissionAction(courseId, slideId, r));
				updateUserProgress(r, dispatch);
			})
			.catch(err => {
				const result: RunSolutionResponse = {
					solutionRunStatus: SolutionRunStatus.InternalServerError,
					message: err.message,
					submission: null,
				};
				dispatch(addSubmissionAction(courseId, slideId, result));
			});
	};

	function updateUserProgress(r: RunSolutionResponse, dispatch: Dispatch) {
		let fieldsToUpdate = {};
		if(r.score != null) {
			fieldsToUpdate = { ...fieldsToUpdate, score: r.score };
		}
		if(r.waitingForManualChecking != null) {
			fieldsToUpdate = { ...fieldsToUpdate, waitingForManualChecking: r.waitingForManualChecking };
		}
		if(r.prohibitFurtherManualChecking != null) {
			fieldsToUpdate = {
				...fieldsToUpdate,
				prohibitFurtherManualChecking: r.prohibitFurtherManualChecking
			};
		}
		if(Object.keys(fieldsToUpdate).length > 0) {
			dispatch(userProgressUpdateAction(courseId, slideId, fieldsToUpdate));
		}
	}
};

export const addReviewComment = (
	courseId: string,
	slideId: string,
	submissionId: number,
	reviewId: number,
	text: string,
) => {
	return (dispatch: Dispatch): void => {
		dispatch(addReviewCommentStartAction(courseId, slideId, submissionId, reviewId));

		sendCodeReviewComment(reviewId, text,)
			.then(r => {
				dispatch(addReviewCommentSuccessAction(courseId, slideId, submissionId, reviewId, r));
			})
			.catch(err => {
				dispatch(addReviewCommentFailAction(courseId, slideId, submissionId, reviewId, err));
			});
	};
};

export const deleteReviewComment = (
	courseId: string,
	slideId: string,
	submissionId: number,
	reviewId: number,
	commentId: number,
) => {
	return (dispatch: Dispatch): void => {
		dispatch(deleteReviewCommentStart(courseId, slideId, submissionId, reviewId, commentId));

		deleteCodeReviewComment(reviewId, commentId,)
			.then(() => {
				dispatch(deleteReviewCommentSuccess(courseId, slideId, submissionId, reviewId, commentId));
			})
			.catch(err => {
				dispatch(deleteReviewCommentFail(courseId, slideId, submissionId, reviewId, commentId, err));
			});
	};
};

