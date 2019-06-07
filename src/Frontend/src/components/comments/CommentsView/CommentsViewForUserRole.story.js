import React from "react";
import { storiesOf } from "@storybook/react";
import { withViewport } from "@storybook/addon-viewport";
import CommentsView from "./CommentsView";

const comments = [
	{
		id: 1999,
		text: "Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		renderedText: "Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		author: {
			"id": "11",
			"visibleName": "Maria",
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
					"visibleName": "Pavel",
					"avatarUrl": "https://staff.skbkontur.ru/content/images/default-user-woman.png",
				},
				text: "Я **не согласна**",
				replies: [],
				renderedText: "Я <b>не согласна</b>",
				publishTime: "2019-02-18T14:12:41.947",
				isApproved: true,
				isCorrectAnswer: false,
				likesCount: 0,
				isLiked: false,
				parentCommentId: 1999
			},
			{
				id: 2001,
				author: {
					"id": "13",
					"visibleName": "Kate",
					"avatarUrl": "https://staff.skbkontur.ru/content/images/default-user-woman.png",
				},
				text: "Я **согласна**",
				replies: [],
				renderedText: "Я <b>согласна</b>",
				publishTime: "2019-02-18T14:12:41.947",
				isApproved: false,
				isCorrectAnswer: true,
				likesCount: 5,
				isLiked: true,
				parentCommentId: 1999
			}
		],
	},
	{
		id: 2002,
		text: "Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		renderedText: "Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		author: {
			"id": "13",
			"visibleName": "Henry",
			"avatarUrl": null,
		},
		publishTime: "2019-01-18T14:12:41.947",
		isApproved: false,
		isPinnedToTop: false,
		isLiked: true,
		likesCount: 8,
		replies: [],
	}
];

const userIsNotAuthenticated = {
	"isAuthenticated": false,
	"id": "11",
	"visibleName": "Pavel",
	"avatarUrl": null,
	"systemAccesses": ["viewAllProfiles"],
};

const userWithSystemAccesses = {
	"isAuthenticated": true,
	"id": "11",
	"visibleName": "Maria",
	"avatarUrl": null,
	"systemAccesses": ["viewAllProfiles"],
};

const user = {
	"isAuthenticated": true,
	"id": "11",
	"visibleName": "Maria",
	"avatarUrl": null,
	"systemAccesses": [],
};

const userIsStudent = {
	"isSystemAdministrator": false,
	"courseRole": "student",
	"courseAccesses": [],
};

const userIsInstructor = {
	"isSystemAdministrator": false,
	"courseRole": "instructor",
	"courseAccesses": [],
};

const userIsInstructorWithModerateAccesses = {
	"isSystemAdministrator": false,
	"courseRole": "instructor",
	"courseAccesses": ["editPinAndRemoveComments"],
};

const userIsInstructorWithViewSubmission = {
	"isSystemAdministrator": false,
	"courseRole": "instructor",
	"courseAccesses": ["viewAllStudentsSubmissions"],
};

const userIsCourseAdmin = {
	"isSystemAdministrator": true,
	"courseRole": "courseAdmin",
	"courseAccesses": [],
};

const userIsSysAdmin = {
	"isSystemAdministrator": true,
	"courseRole": "instructor",
	"courseAccesses": [],
};

const fakeCommentsApi = {
	getComments: () => Promise.resolve({topLevelComments: comments}),
	addComment: () => Promise.resolve(console.log("API: added comment")),
	deleteComment: () => Promise.resolve(console.log("API: delete comment")),
	updateComment: () => Promise.resolve(console.log("API: update comment")),
	likeComment: () => Promise.resolve(console.log("API: like comment")),
	dislikeComment: () => Promise.resolve(console.log("API: dislike comment")),
};

storiesOf("Comments/CommentsView", module)
.addDecorator(withViewport())
.add("unauthorized user", () => (
	<CommentsView
		slideType={"exercise"}
		user={userIsNotAuthenticated}
		userRoles={userIsStudent}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.addDecorator(withViewport())
.add("user is student", () => (
	<CommentsView
		slideType={"exercise"}
		user={user}
		userRoles={userIsStudent}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.addDecorator(withViewport())
.add("user is instructor", () => (
	<CommentsView
		slideType={"exercise"}
		user={user}
		userRoles={userIsInstructor}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.addDecorator(withViewport())
.add("user is instructor with accesses to see profiles", () => (
	<CommentsView
		slideType={"exercise"}
		user={userWithSystemAccesses}
		userRoles={userIsInstructor}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.addDecorator(withViewport())
.add("user is instructor with moderate accesses", () => (
	<CommentsView
		slideType={"exercise"}
		user={user}
		userRoles={userIsInstructorWithModerateAccesses}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.addDecorator(withViewport())
.add("user is instructor with accesses view submission", () => (
	<CommentsView
		slideType={"exercise"}
		user={user}
		userRoles={userIsInstructorWithViewSubmission}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.addDecorator(withViewport())
.add("user is course admin", () => (
	<CommentsView
		slideType={"exercise"}
		user={user}
		userRoles={userIsCourseAdmin}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"})
.addDecorator(withViewport())
.add("user is sysadmin", () => (
	<CommentsView
		slideType={"exercise"}
		user={user}
		userRoles={userIsSysAdmin}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		courseId={"BasicProgramming"}
		commentsApi={fakeCommentsApi} />
), {viewport: "desktop"});
