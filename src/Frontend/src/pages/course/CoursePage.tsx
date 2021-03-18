import { connect } from "react-redux";
import { Dispatch } from "redux";
import { RouteComponentProps, withRouter } from "react-router-dom";

import Course from 'src/components/course/Course';

import { loadCourse, loadCourseErrors, changeCurrentCourseAction } from "src/actions/course";
import { loadUserProgress, userProgressUpdate } from "src/actions/userProgress";
import { loadFlashcards } from "src/actions/flashcards";

import getSlideInfo from "src/utils/getSlideInfo";

import { RootState } from "src/redux/reducers";
import { MatchParams } from "src/models/router";
import { CourseInfo, UnitInfo } from "src/models/course";
import { FlashcardsStatistics } from "src/components/course/Navigation/types";

const mapStateToProps = (state: RootState, { match, location, }: RouteComponentProps<MatchParams>) => {
	const slideInfo = getSlideInfo(location);
	const courseId = match.params.courseId.toLowerCase();

	const courseInfo = state.courses.fullCoursesInfo[courseId];
	const flashcardsByUnit = state.courses.flashcardsInfoByCourseByUnits[courseId];
	const flashcardsStatisticsByUnits: { [unitId: string]: FlashcardsStatistics } | undefined = flashcardsByUnit ? {} : undefined;
	if(flashcardsStatisticsByUnits) {
		for (const unitId in flashcardsByUnit) {
			flashcardsStatisticsByUnits[unitId] = {
				count: flashcardsByUnit[unitId].cardsCount,
				unratedCount: flashcardsByUnit[unitId].unratedFlashcardsCount,
			};
		}
	}

	const loadedCourseIds: { [courseId: string]: boolean } = {};
	for (const courseId of Object.keys(state.courses.fullCoursesInfo)) {
		loadedCourseIds[courseId] = true;
	}
	const isNavigationVisible = slideInfo && !slideInfo.isLti && !slideInfo.isReview && (courseInfo == null || courseInfo.tempCourseError == null);
	const pageInfo = {
		isNavigationVisible: isNavigationVisible || false,
		isReview: slideInfo?.isReview || false,
		isLti: slideInfo?.isLti || false,
		isAcceptedSolutions: slideInfo?.isAcceptedSolutions || false,
		isAcceptedAlert: slideInfo?.isAcceptedAlert || false,
	};

	return {
		courseId,
		slideId: slideInfo?.slideId,
		courseInfo,
		loadedCourseIds,
		pageInfo: pageInfo,
		units: mapCourseInfoToUnits(courseInfo),
		user: state.account,
		progress: state.userProgress.progress[courseId],
		courseLoadingErrorStatus: state.courses.courseLoadingErrorStatus,
		flashcardsStatisticsByUnits,
		flashcardsLoading: state.courses.flashcardsLoading,

		navigationOpened: state.navigation.opened,
		isSlideReady: state.slides.isSlideReady,
		isHijacked: state.account.isHijacked,
		isStudentMode: state.instructor.isStudentMode,
	};
};
const mapDispatchToProps = (dispatch: Dispatch) => ({
	enterToCourse: (courseId: string) => dispatch(changeCurrentCourseAction(courseId)),
	loadCourse: (courseId: string) => loadCourse(courseId)(dispatch),
	loadFlashcards: (courseId: string) => loadFlashcards(courseId)(dispatch),
	loadCourseErrors: (courseId: string) => loadCourseErrors(courseId)(dispatch),
	loadUserProgress: (courseId: string, userId: string) => loadUserProgress(courseId, userId)(dispatch),
	updateVisitedSlide: (courseId: string, slideId: string) => userProgressUpdate(courseId, slideId)(dispatch),
});


const connected = connect(mapStateToProps, mapDispatchToProps)(Course);
export default withRouter(connected);


function mapCourseInfoToUnits(courseInfo: CourseInfo) {
	if(!courseInfo || !courseInfo.units) {
		return null;
	}
	return courseInfo.units.reduce((acc: { [unitId: string]: UnitInfo }, item) => {
		acc[item.id] = item;
		return acc;
	}, {});
}
