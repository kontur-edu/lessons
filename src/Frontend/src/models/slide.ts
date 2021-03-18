import { ReactNode } from "react";
import { Language } from "src/consts/languages";
import { SubmissionInfoRedux } from "src/models/reduxState";
import { AttemptsStatistics, SubmissionInfo } from "src/models/exercise";

interface ShortSlideInfo {
	id: string;
	title: string;
	hide: boolean | undefined;
	slug: string; // Человекочитаемый фрагмент url для слайда
	maxScore: number;
	scoringGroup: string | null;
	containsVideo: boolean;
	type: SlideType;
	apiUrl: string;
	questionsCount: number; // Количество вопросов в quiz
	quizMaxTriesCount: number; // Макс число попыток для quiz
	gitEditLink?: string;
}

enum SlideType {
	Lesson = "lesson",
	Quiz = "quiz",
	Exercise = "exercise",
	Flashcards = "flashcards",
	CourseFlashcards = "courseFlashcards",
	PreviewFlashcards = "previewFlashcards",
}

export enum BlockTypes {
	video = "youtube",
	code = "code",
	text = "html",
	image = "imageGallery",
	spoiler = "spoiler",
	tex = 'tex',
	exercise = 'exercise',
}

interface Block<T extends BlockTypes> {
	$type: T,
	key: number,
	hide: boolean,
}

interface SpoilerBlock extends Block<BlockTypes.spoiler> {
	blocks: Block<BlockTypes>[],
	blocksId: string,
	isPreviousBlockHidden: boolean,
	renderedBlocks: ReactNode[],
}

interface TexBlock extends Block<BlockTypes.tex> {
	content: string,
	lines: string[],
}

interface VideoBlock extends Block<BlockTypes.video> {
	autoplay: boolean,
	openAnnotation: boolean,
	annotationWithoutBottomPaddings: boolean,
}

interface ExerciseBlock extends Block<BlockTypes.exercise> {
	slideId: string,
	courseId: string,
	forceInitialCode: boolean,
	maxScore?: number,
	submissions?: SubmissionInfo[],//we're moving this field to other state in redux reducer
	isLti: boolean,
}

interface ExerciseBlockProps {
	languages: Language[],
	languageInfo: EnumDictionary<Language, LanguageLaunchInfo> | null,
	defaultLanguage: Language | null,
	renderedHints: string[],
	exerciseInitialCode: string,
	hideSolutions: boolean,
	expectedOutput: string,
	submissions: SubmissionInfoRedux[],
	attemptsStatistics: AttemptsStatistics | null,
}

interface LanguageLaunchInfo {
	compiler: string;
	compileCommand: string;
	runCommand: string;
}

export {
	ShortSlideInfo,
	SlideType,
	Block,
	SpoilerBlock,
	TexBlock,
	VideoBlock,
	ExerciseBlock,
	ExerciseBlockProps,
	LanguageLaunchInfo,
};
