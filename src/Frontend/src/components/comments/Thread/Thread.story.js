import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Thread from "./Thread";

const user = {
	"is_authenticated": true,
	"id": "11",
	"visibleName": "Pavel",
	"avatarUrl": null,
};

const userRoles = {
	"isSystemAdministrator": true,
	"courseRole": "Student",
	"courseAccesses": [{
		accesses: null,
	}],
};

const commentEditing = {
	commentId: null,
	sending: false
};

const reply = {
	commentId: null,
	sending: false
};

const actions = {
	handleLikeClick: action("likeComment"),
	handleCorrectAnswerMark: action("correctMark"),
	handleApprovedMark: action("approvedMark"),
	handlePinnedToTopMark: action("pinnedToTopMark"),
	handleEditComment: action("editComment"),
	handleAddReplyComment: action("addReplyComment"),
	handleDeleteComment: action("deleteComment"),
	handleShowEditForm: action("showEditForm"),
	handleShowReplyForm: action("showReplyForm"),
};

const comment = {
	id: 1,
	text: "Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
	renderedText: "Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
	author: {
		"id": "11",
		"visibleName": "Louisa",
		"avatarUrl": null,
	},
	publishTime: "2019-01-18T14:12:41.947",
	isApproved: false,
	isPinnedToTop: false,
	isLiked: true,
	likesCount: 10,
	replies: [
		{
			id: 2000,
			author: {
				"id": "10",
				"visibleName": "Maria",
				"avatarUrl": "https://staff.skbkontur.ru/content/images/default-user-woman.png",
			},
			text: "Я **не согласна**",
			replies: [],
			renderedText: "Я <b>не согласна</b>",
			publishTime: "2019-02-18T14:12:41.947",
			isApproved: false,
			isCorrectAnswer: false,
			likesCount: 0,
			parentCommentId: 1999
		}
	],
};

function getUserSolutionUrl(userId) {
	return `https://dev.ulearn.me/Analytics/UserSolutions?courseId=BasicProgramming&slideId=90bcb61e-57f0-4baa-8bc9-10c9cfd27f58&userId=${userId}`;
}

storiesOf("Comments/Thread", module)
.add("comment with replies", () => (
	<Thread
		comment={comment}
		user={user}
		getUserSolutionsUrl={getUserSolutionUrl}
		userRoles={userRoles}
		commentEditing={commentEditing}
		reply={reply}
		actions={actions} />
), {viewport: "desktop"});