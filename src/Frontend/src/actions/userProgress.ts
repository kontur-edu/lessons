import { Dispatch } from "redux";

import { getUserProgressInCourse, updateUserProgressInCourse } from 'src/api/userProgress';

import { SlideUserProgress, UsersProgressResponse } from "src/models/userProgress";
import {
	USER__PROGRESS_LOAD_FAIL,
	USER__PROGRESS_LOAD_START,
	USER__PROGRESS_LOAD_SUCCESS,
	USER__PROGRESS_UPDATE,
	LoadUserProgressFailAction,
	LoadUserProgressStartAction,
	LoadUserProgressSuccessAction,
	UserProgressUpdateAction,
	UserProgressActionTypes,
} from "./userProgress.types";


const loadUserProgressStart = (): LoadUserProgressStartAction => ({
	type: USER__PROGRESS_LOAD_START,
});

const loadUserProgressSuccess = (courseId: string,
	result: { [slideId: string]: SlideUserProgress }
): LoadUserProgressSuccessAction => ({
	type: USER__PROGRESS_LOAD_SUCCESS,
	courseId,
	result,
});

const loadUserProgressFail = (): LoadUserProgressFailAction => ({
	type: USER__PROGRESS_LOAD_FAIL,
});

export const userProgressUpdateAction = (courseId: string, slideId: string, fieldsToUpdate: Partial<SlideUserProgress>
): UserProgressUpdateAction => ({
	type: USER__PROGRESS_UPDATE,
	courseId,
	slideId,
	fieldsToUpdate
});

export const userProgressUpdate = (courseId: string, slideId: string
): (dispatch: Dispatch<UserProgressUpdateAction>) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch<UserProgressUpdateAction>) => {
		dispatch(userProgressUpdateAction(courseId, slideId, { visited: true }));
		updateUserProgressInCourse(courseId, slideId)
			.then(r => {
				const userId = Object.keys(r.userProgress)[0];
				const newTimestamp = r.userProgress[userId].visitedSlides[slideId].timestamp;
				dispatch(userProgressUpdateAction(courseId, slideId,
					{ timestamp: newTimestamp }));
			})
			.catch((err: Error) => {
				console.error(err); // TODO rozentor handle error
			});
	};
};

export const loadUserProgress = (courseId: string, userId: string
): (dispatch: Dispatch<UserProgressActionTypes>) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch<UserProgressActionTypes>) => {
		dispatch(loadUserProgressStart());

		getUserProgressInCourse(courseId)
			.then((result: UsersProgressResponse) => {
				const progress = result.userProgress[userId] ? result.userProgress[userId].visitedSlides : {};
				dispatch(loadUserProgressSuccess(courseId, progress));
			})
			.catch(() => {
				dispatch(loadUserProgressFail());
			});
	};
};
