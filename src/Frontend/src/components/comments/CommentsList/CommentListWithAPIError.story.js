import React from "react";
import { storiesOf } from "@storybook/react";
import CommentsList from "./CommentsList";

const user = {
	"isAuthenticated": true,
	"id": "11",
	"visibleName": "Pavel",
	"avatarUrl": null,
};

const userRoles = {
	"isSystemAdministrator": true,
	"courseRole": "Student",
	"courseAccesses": [],
};

const comments = [
	{
		id: 1999,
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
				isApproved: true,
				isCorrectAnswer: false,
				likesCount: 0,
				isLiked: false,
				parentCommentId: 1999
			},
			{
				id: 2001,
				author: {
					"id": "11",
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

const brokenCommentsApi = {
	getComments: () => {
		throw new Error("Произошла ошибка. Попробуйте обновить страницу");
	},
	addComment: () => Promise.resolve(console.log("API: added comment")),
	deleteComment: () => Promise.resolve(console.log("API: delete comment")),
	updateComment: () => Promise.resolve(console.log("API: update comment")),
	likeComment: () => Promise.resolve(console.log("API: like comment")),
	dislikeComment: () => Promise.resolve(console.log("API: dislike comment")),
};

const brokenMethodsCommentsApi = {
	getComments: () => Promise.resolve({topLevelComments: comments}),
	getComment: () => Promise.reject(new Error("произошла ошибка")),
	addComment: () => Promise.reject(new Error("произошла ошибка")),
	deleteComment: () => Promise.reject(new Error("произошла ошибка")),
	updateComment: () => Promise.reject(new Error("произошла ошибка")),
	likeComment: () => Promise.reject(new Error("произошла ошибка")),
	dislikeComment: () => Promise.reject(new Error("произошла ошибка")),
};

function getUserSolutionsUrl(userId) {
	return `https://dev.ulearn.me/Analytics/UserSolutions?courseId=BasicProgramming&slideId=90bcb61e-57f0-4baa-8bc9-10c9cfd27f58&userId=${userId}`;
}

storiesOf("Comments/CommentsList", module)
.add("api error 500", () => (
	<CommentsList
		slideType={"exercise"}
		comments={comments}
		getUserSolutionsUrl={getUserSolutionsUrl(user.id)}
		user={user}
		userRoles={userRoles}
		courseId={"BasicProgramming"}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		commentsApi={brokenCommentsApi} />
), {viewport: "desktop"})
.add("api request error", () => (
	<CommentsList
		slideType={"exercise"}
		comments={comments}
		getUserSolutionsUrl={getUserSolutionsUrl(user.id)}
		user={user}
		userRoles={userRoles}
		courseId={"BasicProgramming"}
		slideId={"90bcb61e-57f0-4baa-8bc9-10c9cfd27f58"}
		commentsApi={brokenMethodsCommentsApi} />
), {viewport: "desktop"});

