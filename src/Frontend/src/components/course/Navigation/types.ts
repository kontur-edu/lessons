import { SlideType } from 'src/models/slide';
import { SlideInfo } from "src/utils/getSlideInfo";

export interface MenuItem<T extends SlideType> {
	type: T;

	id: string;
	title: string;
	url: string;
	score: number;
	maxScore: number;
	videosCount: number;
	questionsCount: number,
	quizMaxTriesCount: number,

	isActive: boolean;
	visited: boolean;
	hide?: boolean;

	status: SlideProgressStatus;
}

export interface QuizMenuItem extends MenuItem<SlideType.Quiz> {
	questionsCount: number;
}

export interface Progress {
	current: number;
	max: number;
}

export interface UnitProgress extends Progress {
	doneSlidesCount: number;
	inProgressSlidesCount: number;
	slidesCount: number;
	statusesBySlides: { [slideId: string]: SlideProgressStatus };
}

export interface StartupSlideInfo {
	id: string;
	timestamp: Date;
	status: SlideProgressStatus;
}

export interface UnitProgressWithLastVisit extends UnitProgress {
	startupSlide: StartupSlideInfo | null;
}

export enum SlideProgressStatus {
	'notVisited',
	'canBeImproved',
	'done',
}

export interface CourseMenuItem {
	id: string;
	title: string;
	progress?: UnitProgress;
	isActive: boolean;
	isNotPublished?: boolean;
	publicationDate?: string;
	onClick?: (id: string) => void;
}

export interface FlashcardsStatistics {
	count: number;
	unratedCount: number;
}

export interface CourseStatistics {
	courseProgress: Progress;
	byUnits: { [unitId: string]: UnitProgressWithLastVisit };

	flashcardsStatistics: FlashcardsStatistics;
	flashcardsStatisticsByUnits: { [unitId: string]: FlashcardsStatistics };
}
