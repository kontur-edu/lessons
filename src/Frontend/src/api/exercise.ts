import api from "src/api/index";
import { buildQuery } from "src/utils";

import { Language } from "src/consts/languages";
import { ReviewCommentResponse, RunSolutionResponse } from "src/models/exercise";

export function submitCode(courseId: string, slideId: string, code: string,
	language: Language
): Promise<RunSolutionResponse> {
	const query = buildQuery({ language }) || '';
	return api.post<RunSolutionResponse>(
		`slides/${ courseId }/${ slideId }/exercise/submit` + query,
		api.createRequestParams({ solution: code }));
}

export function sendCodeReviewComment(reviewId: number, text: string): Promise<ReviewCommentResponse> {
	return api.post(`review/${ reviewId }/comments`,
		api.createRequestParams({ text }));
}

export function deleteCodeReviewComment(reviewId: number, commentId: number): Promise<Response> {
	return api.delete(`review/${ reviewId }/comments/${ commentId }`);
}
