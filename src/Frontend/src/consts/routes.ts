import { buildQuery } from "src/utils";

export const coursePath = "course";
export const coursesPath = "/course/courses";
export const flashcards = "flashcards";
export const flashcardsPreview = "preview";
export const commentsPath = "comments";
export const exerciseStudentSubmissions = 'exercise/studentSubmissions';
export const commentPoliciesPath = "comment-policies";
export const courseStatistics = "/analytics/courseStatistics";
export const slides = "slides";
export const ltiSlide = "ltislide";
export const acceptedAlert = "acceptedalert";
export const acceptedSolutions = "acceptedsolutions";
export const resetStudentsLimits = "/students/reset-limits";
export const signalrWS = 'ws';
export const login = 'login';
export const account = 'account';
export const accountPath = '/' + account + '/manage';
export const register = account + '/register';
export const logoutPath = account + '/logout';
export const rolesPath = account + '/roles';
export const externalLoginConfirmation = login + '/externalLoginConfirmation';
export const externalLoginCallback = login + '/externalLoginCallback';
export const feed = 'feed';
export const notificationsFeed = feed + '/notificationsPartial';

export function constructPathToSlide(courseId: string, slideId: string): string {
	return `/${ coursePath }/${ courseId }/${ slideId }`;
}

export function constructPathToAcceptedSolutions(courseId: string, slideId: string): string {
	return `/${ coursePath }/${ courseId }/${ acceptedSolutions }?slideId=${ slideId }`;
}

export function constructPathToComment(commentId: string, isLike?: boolean): string {
	const url = `${ commentsPath }/${ commentId }`;

	if(isLike) {
		return url + "/like";
	}

	return url;
}

export function constructPathToStudentSubmissions(courseId: string, slideId: string): string {
	return `/${ exerciseStudentSubmissions }?courseId=${ courseId }&slideId=${ slideId }`;
}

export function constructPathToFlashcardsPreview(courseId: string, openUnitId?: string | null): string {
	const unitIdQuery = buildQuery({ unitId: openUnitId });
	const url = `/${ coursePath }/${ courseId }/${ flashcards }/${ flashcardsPreview }`;

	return unitIdQuery ? url + unitIdQuery : url;
}

export function constructLinkWithReturnUrl(link: string, returnUrl?: string): string {
	return `/${ link }${ buildQuery({ returnUrl: returnUrl || window.location.pathname }) }`;
}
