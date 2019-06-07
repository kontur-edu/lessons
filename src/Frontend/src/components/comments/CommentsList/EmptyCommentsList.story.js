import React from "react";
import { storiesOf } from "@storybook/react";
import CommentsList from "./CommentsList";

const user = {
	"isAuthenticated": true,
	"id": "11",
	"visibleName": "Pavel",
	"avatarUrl": null,
};

const userIsNotAuthenticated = {
	"isAuthenticated": false,
	"id": "11",
	"visibleName": "Pavel",
	"avatarUrl": null,
	"systemAccesses": ["viewAllProfiles"],
};

const userIsStudent = {
	"isSystemAdministrator": false,
	"courseRole": "Student",
	"courseAccesses": [],
};

const userIsInstructor = {
	"isSystemAdministrator": false,
	"courseRole": "instructor",
	"courseAccesses": [],
};

function getUserSolutionsUrl(userId) {
	return `https://dev.ulearn.me/Analytics/UserSolutions?courseId=BasicProgramming&slideId=90bcb61e-57f0-4baa-8bc9-10c9cfd27f58&userId=${userId}`;
}

const comments = [];

const fakeCommentsApi = {
	getComments: () => Promise.resolve({topLevelComments: comments}),
	getComment: () => Promise.resolve(console.log("API: get comment")),
	addComment: () => Promise.resolve(console.log("API: added comment")),
	deleteComment: () => Promise.resolve(console.log("API: delete comment")),
	updateComment: () => Promise.resolve(console.log("API: update comment")),
	likeComment: () => Promise.resolve(console.log("API: like comment")),
	dislikeComment: () => Promise.resolve(console.log("API: dislike comment")),
};

storiesOf("Comments/CommentsList", module)
.add("empty comments list for unauthorized user", () => (
	<CommentsList
		slideType={"exercise"}
		comments={comments}
		getUserSolutionsUrl={getUserSolutionsUrl(user.id)}
		user={userIsNotAuthenticated}
		userRoles={userIsStudent}
		courseId={"BasicProgramming"}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.add("empty comments list for student", () => (
	<CommentsList
		slideType={"exercise"}
		comments={comments}
		getUserSolutionsUrl={getUserSolutionsUrl(user.id)}
		user={user}
		userRoles={userIsStudent}
		courseId={"BasicProgramming"}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.add("empty comments list for instructor", () => (
	<CommentsList
		slideType={"exercise"}
		comments={comments}
		getUserSolutionsUrl={getUserSolutionsUrl(user.id)}
		user={user}
		userRoles={userIsInstructor}
		courseId={"BasicProgramming"}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"});