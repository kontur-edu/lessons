import { acceptedAlert, acceptedSolutions, coursePath, ltiSlide } from "src/consts/routes";

import queryString from "query-string";

export interface SlideInfo {
	slideId: string,
	isReview: boolean,
	isLti: boolean,
	isAcceptedAlert: boolean,
	isAcceptedSolutions: boolean,
}

export default function getSlideInfo(location: { pathname: string, search: string },): null | SlideInfo {
	const { pathname, search } = location;
	const isInsideCourse = pathname
		.toLowerCase()
		.startsWith(`/${ coursePath }`);
	if(!isInsideCourse) {
		return null;
	}

	const params = queryString.parse(search);
	const slideIdInQuery = params.slideId;
	const slideSlugOrAction = isInsideCourse ? pathname.split('/').slice(-1)[0].toLowerCase() : undefined;

	let slideId;
	let isLti = false;
	let isAcceptedAlert = false;
	if(slideSlugOrAction) {
		if(slideIdInQuery) {
			const action = slideSlugOrAction;
			slideId = slideIdInQuery;
			isAcceptedAlert = action.toLowerCase() === acceptedAlert;
			isLti = (action.toLowerCase() === ltiSlide || isAcceptedAlert || params.isLti) as boolean;
		} else {
			slideId = slideSlugOrAction.split('_').pop();
		}

		const isReview = params.CheckQueueItemId !== undefined;
		const isAcceptedSolutions = slideSlugOrAction.toLowerCase() === acceptedSolutions;

		return {
			slideId: slideId as string,
			isReview,
			isLti,
			isAcceptedAlert,
			isAcceptedSolutions,
		};
	}

	return null;
}
