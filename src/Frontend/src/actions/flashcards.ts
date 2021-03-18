import { Dispatch } from "redux";
import { getFlashcards, putFlashcardStatus } from "src/api/flashcards";

import { RateTypes } from "src/consts/rateTypes";
import {
	FLASHCARDS_RATE_START,
	FLASHCARDS_LOAD_FAIL,
	FLASHCARDS_LOAD_START,
	FLASHCARDS_LOAD_SUCCESS,
	FlashcardsAction,
} from "src/actions/course.types";
import { UnitFlashcards } from "src/models/flashcards";

const loadFlashcardsStartAction = (): FlashcardsAction => ({
	type: FLASHCARDS_LOAD_START,
});

const loadFlashcardsSuccessAction = (courseId: string, result: UnitFlashcards[]): FlashcardsAction => ({
	type: FLASHCARDS_LOAD_SUCCESS,
	courseId,
	result,
});

const loadFlashcardsFailAction = (): FlashcardsAction => ({
	type: FLASHCARDS_LOAD_FAIL,
});

const sendFlashcardResultStartAction = (
	courseId: string,
	unitId: string,
	flashcardId: string,
	rate: RateTypes,
	newTLast: number
): FlashcardsAction => ({
	type: FLASHCARDS_RATE_START,
	courseId,
	unitId,
	flashcardId,
	rate,
	newTLast,
});


export const loadFlashcards = (courseId: string): (dispatch: Dispatch) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch) => {
		dispatch(loadFlashcardsStartAction());

		getFlashcards(courseId)
			.then(result => {
				dispatch(loadFlashcardsSuccessAction(courseId, result.units));
			})
			.catch(err => {
				console.error(err);
				dispatch(loadFlashcardsFailAction());
			});
	};
};

export const sendFlashcardResult = (
	courseId: string,
	unitId: string,
	flashcardId: string,
	rate: RateTypes,
	newTLast: number
): (dispatch: Dispatch) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch) => {
		dispatch(sendFlashcardResultStartAction(courseId, unitId, flashcardId, rate, newTLast));
		putFlashcardStatus(courseId, flashcardId, rate);
	};
};
