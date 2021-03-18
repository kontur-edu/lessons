import { ShortSlideInfo } from "./slide";
import { ScoringGroupsIds } from "src/consts/scoringGroup";

interface CourseInfo {
	id: string;
	title: string;
	timestamp?: string;
	units: UnitInfo[];
	nextUnitPublishTime: string | null;
	scoring: ScoringInfo;
	containsFlashcards: boolean;
	isTempCourse: boolean;
	tempCourseError: string | null;
}

interface ScoringInfo {
	id: string;
	name: string;
	abbr: string | null;
	description: string | null;
	weight: number; // decimal
	groups: ScoringGroup[];
}

interface ScoringGroup {
	id: ScoringGroupsIds;
	weight: number;
}

interface UnitInfo {
	id: string;
	title: string;
	isNotPublished?: boolean;
	publicationDate?: string;
	slides: ShortSlideInfo[];
	additionalScores: UnitScoringGroupInfo[];
}

interface InfoByUnit {
	unitId: string,
	unitTitle: string,
	unlocked: boolean,
	flashcardsIds: string[],
	unratedFlashcardsCount: number,
	cardsCount: number,
	flashcardsSlideSlug: string,
}

interface AbstractScoringGroupInfo {
	id: string;
	name: string;
	abbreviation: string;
	description: string;
	weight: number; // decimal
}

interface UnitScoringGroupInfo extends AbstractScoringGroupInfo {
	canInstructorSetAdditionalScore: boolean;
	maxAdditionalScore: number;
}

export interface TempCourseErrorsResponse extends Response {
	tempCourseError: string | null,
}

export interface CoursesListResponse extends Response {
	courses: ShortCourseInfo[],
}

export interface ShortCourseInfo {
	id: string,
	title: string,
	apiUrl: string,
	isTempCourse: boolean,
}

interface UnitsInfo {
	[p: string]: UnitInfo;
}

interface PageInfo {
	isLti: boolean;
	isReview: boolean;
	isAcceptedSolutions: boolean;
	isNavigationVisible: boolean;
	isAcceptedAlert: boolean;
}

export {
	CourseInfo,
	ScoringInfo,
	UnitInfo,
	UnitScoringGroupInfo,
	InfoByUnit,
	ScoringGroup,
	UnitsInfo,
	PageInfo,
};
