import React from "react";
import NextUnit from "./NextUnit";
import { SlideType } from "src/models/slide";


const _NextUnit = (): React.ReactNode =>
	<NextUnit
		onClick={ () => ({}) }
		unit={ {
			additionalScores: [],
			id: '123',
			title: 'Следующий модуль',
			slides: [{
				maxScore: 0,
				scoringGroup: null,
				slug: '123-213-slug',
				id: '1',
				title: "123-21",
				hide: false,
				type: SlideType.Lesson,
				apiUrl: '123',
				questionsCount: 0,
			}],
		} }
	/>;

export default {
	title: "ModuleNavigation",
};
export { _NextUnit };
_NextUnit.storyName = "Следующий модуль";
