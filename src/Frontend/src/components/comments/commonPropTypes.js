import { shape, oneOf, arrayOf, bool, array, number, string, element, objectOf } from "prop-types";

export const userType = shape({
	id: string,
	visibleName: string,
	login: string,
	email: string,
	firstName: string,
	lastName: string,
	avatarUrl: string,
	gender: string,
});

export const user = shape({
	isAuthenticated: bool.isRequired,
	id: string,
	visibleName: string,
	avatarUrl: string,
	systemAccesses: array,
});

export const userRoles = shape({
	isSystemAdministrator: bool,
	courseRoles: oneOf(["courseAdmin", "instructor", "tester", "student"]),
	courseAccesses: arrayOf(oneOf(["editPinAndRemoveComments", "viewAllStudentsSubmissions", "addAndRemoveInstructors",
		"viewAllGroupMembers", "apiViewCodeReviewStatistics"])),
});

export const commentPolicy = shape({
	areCommentsEnabled: bool,
	moderationPolicy: string,
	onlyInstructorsCanReply: bool,
	status: string,
});

export const group = shape({
	id: number,
	name: string,
	apiUrl: string,
	isArchived: bool,
});

export const comment = shape({
	id: number.isRequired,
	author: userType.isRequired,
	text: string,
	authorGroups: arrayOf(group),
	renderedText: string,
	publishTime: string,
	isApproved: bool,
	isCorrectAnswer: bool,
	isPinnedToTop: bool,
	likesCount: number,
	parentCommentId: number,
	courseId: string,
	slideId: string,
});

comment.replies = arrayOf(comment);

export const commentStatus = shape({
	commentId: number,
	sending: bool,
});

export const markdownType = oneOf(['bold', 'italic', 'code']);

export const markdownDescription = shape({
		[markdownType]: shape({
		markup: string,
		description: string,
		hotkey: shape ({
			asText: string,
			ctrl: bool,
			key: string,
		}),
		icon: element,
	}),
});

export const markupOperation = objectOf(markdownDescription);
