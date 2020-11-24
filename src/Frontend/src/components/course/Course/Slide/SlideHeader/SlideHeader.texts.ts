import React from "react";
import getPluralForm from "src/utils/getPluralForm.js";

export default {
	getSlideScore: (score: number, maxScore: number, showZeroScore = false) : React.ReactNode =>
		showZeroScore && score === 0
			? `Максимум ${ maxScore } ${ getPluralForm(maxScore, 'балл', 'балла', 'баллов') }`
			: `${ score } ${ getPluralForm(score, 'балл', 'балла', 'баллов') } из ${ maxScore }`,
	skippedHeaderText: "Вы посмотрели чужие решения, поэтому не получите баллы за эту задачу",
	hiddenSlideText: "Студенты не видят этот слайд.",
	//hiddenSlideTextWithStudents: (showedGroupsIds) => `Студенты ${ showedGroupsIds.join(', ') } видят этот слайд.`,
}