import React, { createRef, RefObject } from 'react';

import { Controlled, } from "react-codemirror2";
import { Checkbox, FLAT_THEME, Modal, Select, ThemeContext, Toast, Tooltip, } from "ui";
import { Review } from "./Review/Review";
import { CongratsModal } from "./CongratsModal/CongratsModal";
import { ExerciseOutput, HasOutput } from "./ExerciseOutput/ExerciseOutput";
import { ExerciseFormHeader } from "./ExerciseFormHeader/ExerciseFormHeader";
import Controls from "./Controls/Controls";
import LoginForContinue from "src/components/notificationModal/LoginForContinue";
import DownloadedHtmlContent from "src/components/common/DownloadedHtmlContent.js";
import { Info } from 'icons';

import { darkFlat } from "src/uiTheme";

import classNames from 'classnames';
import moment from "moment";

import { exerciseSolutions, loadFromCache, saveToCache, } from "src/utils/localStorageManager";
import { convertDefaultTimezoneToLocal } from "src/utils/momentUtils";
import {
	GetLastSuccessSubmission,
	GetSubmissionColor,
	HasSuccessSubmission,
	IsFirstRightAnswer,
	SubmissionColor,
	SubmissionIsLast,
} from "./ExerciseUtils";

import { Language, } from "src/consts/languages";
import { constructPathToAcceptedSolutions, } from "src/consts/routes";
import { DeviceType } from "src/consts/deviceType";
import {
	AutomaticExerciseCheckingResult as CheckingResult,
	AutomaticExerciseCheckingResult,
	ReviewInfo,
	RunSolutionResponse,
	SolutionRunStatus,
} from "src/models/exercise";
import { ReviewInfoRedux, SubmissionInfoRedux } from "src/models/reduxState";
import { SlideUserProgress } from "src/models/userProgress";
import { ExerciseBlockProps } from "src/models/slide";

import CodeMirror, { Doc, Editor, EditorChange, EditorConfiguration, MarkerRange, TextMarker } from "codemirror";
import 'codemirror/lib/codemirror.css';
import 'codemirror/addon/edit/matchbrackets';
import 'codemirror/addon/hint/show-hint';
import 'codemirror/addon/hint/show-hint.css';
import 'codemirror/addon/hint/javascript-hint';
import 'codemirror/addon/hint/anyword-hint';
import 'codemirror/theme/darcula.css';
import registerCodeMirrorHelpers from "./CodeMirrorAutocompleteExtension";

import styles from './Exercise.less';

import texts from './Exercise.texts';


interface DispatchFunctionsProps {
	sendCode: (courseId: string, slideId: string, value: string, language: Language,) => unknown;
	addReviewComment: (courseId: string, slideId: string, submissionId: number, reviewId: number,
		text: string
	) => unknown;
	deleteReviewComment:
		(courseId: string, slideId: string, submissionId: number, reviewId: number, commentId: number) => unknown;
	visitAcceptedSolutions: (courseId: string, slideId: string) => unknown;
}

interface FromSlideProps {
	courseId: string,
	slideId: string,
	maxScore: number,
	forceInitialCode: boolean,
}

interface FromMapStateToProps {
	isAuthenticated: boolean,
	lastCheckingResponse: RunSolutionResponse | null,
	userId?: string | null,
	slideProgress: SlideUserProgress,
	submissionError: string | null,
	deviceType: DeviceType,
}

interface Props extends ExerciseBlockProps, DispatchFunctionsProps, FromSlideProps, FromMapStateToProps {
	className?: string,
}

enum ModalType {
	congrats,
	loginForContinue,
	acceptedSolutions,
	studentsSubmissions,
}

interface ModalData<T extends ModalType> {
	type: T,
}

interface CongratsModalData extends ModalData<ModalType.congrats> {
	score: number | null,
	waitingForManualChecking: boolean | null,
}

interface SelfCheckup {
	text: string,
	checked: boolean,
	onClick: () => void,
}

interface ReviewInfoWithMarker extends ReviewInfoRedux {
	marker: TextMarker,
}

interface State {
	value: string,
	valueChanged: boolean,

	isEditable: boolean,

	language: Language,

	modalData: ModalData<ModalType> | null,

	submissionLoading: boolean,
	isAllHintsShowed: boolean,
	visibleCheckingResponse?: RunSolutionResponse, // Не null только если только что сделанная посылка не содержит submission
	currentSubmission: null | SubmissionInfoRedux,
	currentReviews: ReviewInfoWithMarker[],
	selectedReviewId: number,
	showOutput: boolean,

	editor: null | Editor,
	exerciseCodeDoc: null | Doc,
	savedPositionOfExercise: DOMRect | undefined,

	selfChecks: SelfCheckup[],
}

interface ExerciseCode {
	value: string,
	time: string,
	language: Language,
}

function saveExerciseCodeToCache(id: string, value: string, time: string, language: Language): void {
	saveToCache<ExerciseCode>(exerciseSolutions, id, { value, time, language });
}

function loadExerciseCodeFromCache(id: string): ExerciseCode | undefined {
	return loadFromCache<ExerciseCode>(exerciseSolutions, id);
}

class Exercise extends React.Component<Props, State> {
	private readonly editThemeName = 'darcula';
	private readonly defaultThemeName = 'default';
	private readonly newTry = { id: -1 };
	private readonly lastSubmissionIndex = 0;
	private wrapper: RefObject<HTMLDivElement> = createRef();

	constructor(props: Props) {
		super(props);
		const { exerciseInitialCode, submissions, languages, renderedHints, defaultLanguage } = props;

		this.state = {
			value: exerciseInitialCode,
			valueChanged: false,

			isEditable: submissions.length === 0,

			language: defaultLanguage ?? [...languages].sort()[0],

			modalData: null,

			submissionLoading: false,
			isAllHintsShowed: renderedHints.length === 0,
			visibleCheckingResponse: undefined,
			currentSubmission: null,
			currentReviews: [],
			selectedReviewId: -1,
			showOutput: false,

			editor: null,
			exerciseCodeDoc: null,
			savedPositionOfExercise: undefined,

			selfChecks: texts.checkups.self.checks.map((ch, i) => ({
				text: ch,
				checked: false,
				onClick: () => this.onSelfCheckBoxClick(i)
			})),
		};
	}

	componentDidMount(): void {
		const { forceInitialCode, } = this.props;
		this.overrideCodeMirrorAutocomplete();

		if(forceInitialCode) {
			this.resetCode();
		} else {
			this.loadSlideSubmission();
		}

		window.addEventListener("beforeunload", this.saveCodeDraftToCacheEvent);
	}

	saveCodeDraftToCacheEvent = (): void => {
		this.saveCodeDraftToCache();
	};

	loadSlideSubmission = (): void => {
		const { slideId, submissions, } = this.props;

		if(submissions.length > 0) {
			this.loadSubmissionToState(submissions[this.lastSubmissionIndex]);
		} else {
			this.loadLatestCode(slideId);
		}
	};

	componentDidUpdate(prevProps: Props): void {
		const {
			lastCheckingResponse,
			courseId,
			slideId,
			submissions,
			forceInitialCode,
			submissionError,
			slideProgress,
			isAuthenticated,
		} = this.props;
		const { currentSubmission, submissionLoading, selectedReviewId, value, } = this.state;

		if(submissionError && submissionError !== prevProps.submissionError) {
			Toast.push("При добавлении или удалении комментария произошла ошибка");
		}

		if(forceInitialCode !== prevProps.forceInitialCode) {
			if(forceInitialCode) {
				this.saveCodeDraftToCache();
				this.resetCode();
			} else {
				this.loadSlideSubmission();
			}
			return;
		}

		if(courseId !== prevProps.courseId || slideId !== prevProps.slideId || isAuthenticated && isAuthenticated !== prevProps.isAuthenticated) {
			this.setState({
				submissionLoading: false,
			});
			this.saveCodeDraftToCache(prevProps.slideId, value);
			this.loadSlideSubmission();
			return;
		}

		const hasNewLastCheckingResponse = lastCheckingResponse
			&& lastCheckingResponse !== prevProps.lastCheckingResponse; // Сравнение по ссылкам
		if(hasNewLastCheckingResponse && lastCheckingResponse) {
			const { submission, solutionRunStatus, } = lastCheckingResponse;

			if(submission && submission.automaticChecking?.result !== AutomaticExerciseCheckingResult.WrongAnswer
				&& submission.automaticChecking?.result !== AutomaticExerciseCheckingResult.CompilationError) {
				this.loadSubmissionToState(submissions.find(s => s.id === submission.id));

				if(solutionRunStatus === SolutionRunStatus.Success) {
					const { automaticChecking } = submission;
					if((!automaticChecking || automaticChecking.result === CheckingResult.RightAnswer)
						&& !slideProgress.isSkipped
						&& IsFirstRightAnswer(submissions, submission)
					) {
						this.openModal({
							type: ModalType.congrats,
							score: lastCheckingResponse.score,
							waitingForManualChecking: lastCheckingResponse.waitingForManualChecking,
						});
					}
				}
			} else {
				this.setState({
					visibleCheckingResponse: lastCheckingResponse,
				});
			}

			if(solutionRunStatus === SolutionRunStatus.CompilationError
				|| submission?.automaticChecking?.result === AutomaticExerciseCheckingResult.WrongAnswer
				|| submission?.automaticChecking?.result === AutomaticExerciseCheckingResult.CompilationError) {
				this.setState({
					isEditable: true,
					showOutput: true,
				});
			}

			if(submissionLoading) {
				this.setState({
					submissionLoading: false,
				});
			}
		} else if(currentSubmission) {
			const submission = submissions.find(s => s.id === currentSubmission.id);

			if(submission && currentSubmission !== submission) { // Сравнение по ссылке. Отличаться должны только в случае изменения комментериев
				this.setCurrentSubmission(submission,
					() => this.highlightReview(selectedReviewId)); //Сохраняем выделение выбранного ревью
			}
		}
	}

	overrideCodeMirrorAutocomplete = (): void => {
		registerCodeMirrorHelpers();
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore because autocomplete will be added by js addon script
		CodeMirror.commands.autocomplete = (cm: Editor) => {
			const { language, } = this.state;
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			const hint = CodeMirror.hint[language.toLowerCase()];
			if(hint) {
				cm.showHint({ hint: hint });
			}
		};
	};

	saveCodeDraftToCache = (slideId?: string, value?: string, language?: Language): void => {
		const { forceInitialCode, isAuthenticated } = this.props;
		const { valueChanged, } = this.state;

		if(valueChanged && !forceInitialCode && isAuthenticated) {
			saveExerciseCodeToCache(
				slideId || this.props.slideId,
				value || this.state.value,
				moment().format(),
				language || this.state.language);
		}
	};

	componentWillUnmount(): void {
		this.saveCodeDraftToCache();
		window.removeEventListener("beforeunload", this.saveCodeDraftToCacheEvent);
	}

	render(): React.ReactElement {
		const { className, } = this.props;

		const opts = this.codeMirrorOptions;

		return (
			<div className={ classNames(styles.wrapper, className) } ref={ this.wrapper }>
				{ this.renderControlledCodeMirror(opts) }
			</div>
		);
	}

	get codeMirrorOptions(): EditorConfiguration {
		const { isAuthenticated, } = this.props;
		const { isEditable, language } = this.state;

		return {
			mode: Exercise.loadLanguageStyles(language),
			lineNumbers: true,
			scrollbarStyle: 'null',
			lineWrapping: true,
			theme: isEditable ? this.editThemeName : this.defaultThemeName,
			readOnly: !isEditable || !isAuthenticated,
			matchBrackets: true,
			tabSize: 4,
			indentUnit: 4,
			indentWithTabs: true,
			extraKeys: {
				ctrlSpace: "autocomplete",
				".": function (cm: Editor) {
					setTimeout(function () {
						const cursorPosition = cm.getCursor();
						const char = cm.getLine(cursorPosition.line).substr(Math.max(cursorPosition.ch - 1, 0), 1);

						if(char === '.') {
							cm.execCommand("autocomplete");
						}
					}, 100);
					return CodeMirror.Pass;
				}
			},
		};
	}

	renderControlledCodeMirror = (opts: EditorConfiguration): React.ReactElement => {
		const {
			expectedOutput, submissions, userId,
			slideProgress, maxScore, languages,
			courseId, slideId, hideSolutions, renderedHints,
			attemptsStatistics, isAuthenticated
		} = this.props;
		const {
			value, currentSubmission,
			isEditable, exerciseCodeDoc, modalData,
			currentReviews, showOutput, selectedReviewId, visibleCheckingResponse,
			submissionLoading, isAllHintsShowed,
		} = this.state;

		const isReview = !isEditable && currentReviews.length > 0;
		const automaticChecking = currentSubmission?.automaticChecking ?? visibleCheckingResponse?.automaticChecking ?? visibleCheckingResponse?.submission?.automaticChecking;
		const selectedSubmissionIsLast = SubmissionIsLast(submissions, currentSubmission);
		const selectedSubmissionIsLastSuccess = GetLastSuccessSubmission(submissions) === currentSubmission;
		const isMaxScore = slideProgress.score === maxScore;
		const submissionColor = GetSubmissionColor(visibleCheckingResponse?.solutionRunStatus,
			automaticChecking?.result,
			HasSuccessSubmission(submissions), selectedSubmissionIsLast, selectedSubmissionIsLastSuccess,
			slideProgress.prohibitFurtherManualChecking, slideProgress.isSkipped, isMaxScore);

		const wrapperClassName = classNames(
			styles.exercise,
			{ [styles.reviewWrapper]: isReview },
		);
		const editorClassName = classNames(
			styles.editor,
			{ [styles.editorWithoutBorder]: isEditable },
			{ [styles.editorInReview]: isReview },
		);

		const hasOutput = currentSubmission
			&& HasOutput(visibleCheckingResponse?.message, currentSubmission.automaticChecking,
				expectedOutput);
		const isAcceptedSolutionsWillNotDiscardScore = submissions.filter(
			s => s.automaticChecking?.result === AutomaticExerciseCheckingResult.RightAnswer).length > 0 || slideProgress.isSkipped;
		const outputMessage = visibleCheckingResponse?.message || visibleCheckingResponse?.submission?.automaticChecking?.output;

		return (
			<React.Fragment>
				{ submissions.length !== 0 && this.renderSubmissionsSelect() }
				{ languages.length > 1 && (submissions.length > 0 || isEditable) && this.renderLanguageSelect() }
				{ languages.length > 1 && (submissions.length > 0 || isEditable) && this.renderLanguageLaunchInfoTooltip() }
				{ !isEditable && this.renderHeader(submissionColor, selectedSubmissionIsLast,
					selectedSubmissionIsLastSuccess) }
				{ modalData && this.renderModal(modalData) }
				<div className={ wrapperClassName } onClick={ this.openModalForUnauthenticatedUser }>
					<Controlled
						onBeforeChange={ this.onBeforeChange }
						editorDidMount={ this.onEditorMount }
						onCursorActivity={ this.onCursorActivity }
						onUpdate={ this.scrollToBottomBorderIfNeeded }
						className={ editorClassName }
						options={ opts }
						value={ value }
					/>
					{ exerciseCodeDoc && isReview &&
					<Review
						userId={ userId }
						addReviewComment={ this.addReviewComment }
						deleteReviewComment={ this.deleteReviewComment }
						selectedReviewId={ selectedReviewId }
						onSelectComment={ this.selectComment }
						reviews={ this.getReviewsWithoutDeleted(currentReviews) }
						getReviewAnchorTop={ this.getReviewAnchorTop }
					/>
					}
				</div>
				{/* TODO not included in current release !isEditable && currentSubmission && this.renderOverview(currentSubmission)*/ }
				{ isAuthenticated && <Controls>
					<Controls.SubmitButton
						isLoading={ submissionLoading }
						onClick={ isEditable ? this.sendExercise : this.loadNewTry }
						text={ isEditable ? texts.controls.submitCode.text : texts.controls.submitCode.redactor }
					/>
					{ renderedHints.length !== 0 &&
					<Controls.ShowHintButton
						onAllHintsShowed={ this.onAllHintsShowed }
						renderedHints={ renderedHints }
					/> }
					{ isEditable && <Controls.ResetButton onResetButtonClicked={ this.resetCodeAndCache }/> }
					{ (!isEditable && hasOutput) && <Controls.OutputButton
						showOutput={ showOutput }
						onShowOutputButtonClicked={ this.toggleOutput }
					/> }
					{ attemptsStatistics && <Controls.StatisticsHint attemptsStatistics={ attemptsStatistics }/> }
					{ (!hideSolutions && (isAllHintsShowed || isAcceptedSolutionsWillNotDiscardScore))
					&& <Controls.AcceptedSolutionsButton
						acceptedSolutionsUrl={ constructPathToAcceptedSolutions(courseId, slideId) }
						onVisitAcceptedSolutions={ this.openAcceptedSolutionsModal }
						isShowAcceptedSolutionsAvailable={ isAcceptedSolutionsWillNotDiscardScore }
					/> }
				</Controls>
				}
				{ showOutput && HasOutput(outputMessage, automaticChecking, expectedOutput) &&
				<ExerciseOutput
					solutionRunStatus={ visibleCheckingResponse?.solutionRunStatus ?? SolutionRunStatus.Success }
					message={ outputMessage }
					expectedOutput={ expectedOutput }
					automaticChecking={ automaticChecking }
					submissionColor={ submissionColor }
				/>
				}
			</React.Fragment>
		);
	};

	onAllHintsShowed = (): void => {
		this.setState({
			isAllHintsShowed: true
		});
	};

	openModalForUnauthenticatedUser = (): void => {
		const { isAuthenticated } = this.props;
		if(!isAuthenticated) {
			this.openModal({ type: ModalType.loginForContinue });
		}
	};

	getReviewsWithoutDeleted = (reviews: ReviewInfoWithMarker[]): ReviewInfoWithMarker[] => {
		return reviews.map(r => ({ ...r, comments: r.comments.filter(c => !c.isDeleted && !c.isLoading) }));
	};

	getReviewAnchorTop = (review: ReviewInfo): number => {
		const { editor, } = this.state;

		if(editor) {
			return editor.charCoords({
				line: review.startLine,
				ch: review.startPosition,
			}, 'local').top;
		}
		return -1;
	};

	renderSubmissionsSelect = (): React.ReactElement => {
		const { currentSubmission } = this.state;
		const { submissions, } = this.props;
		const { waitingForManualChecking } = this.props.slideProgress;

		const lastSuccessSubmission = GetLastSuccessSubmission(submissions);
		const items = [[this.newTry.id, texts.submissions.newTry], ...submissions.map((submission) => {
			const caption = texts.submissions
				.getSubmissionCaption(submission, lastSuccessSubmission === submission, waitingForManualChecking);
			return [submission.id, caption];
		})];

		return (
			<div className={ styles.select }>
				<ThemeContext.Provider value={ FLAT_THEME }>
					<Select
						width={ '100%' }
						items={ items }
						value={ currentSubmission?.id || this.newTry.id }
						onValueChange={ this.onSubmissionsSelectValueChange }
					/>
				</ThemeContext.Provider>
			</div>
		);
	};

	onSubmissionsSelectValueChange = (id: unknown): void => {
		const { submissions, } = this.props;

		this.saveCodeDraftToCache();
		if(id === this.newTry.id) {
			this.loadNewTry();
		}
		this.loadSubmissionToState(submissions.find(s => s.id === id));
	};

	renderLanguageSelect = (): React.ReactElement => {
		const { language, isEditable, } = this.state;
		const { languages, languageInfo } = this.props;
		const items = [...languages].sort().map((l) => {
			return [l, texts.getLanguageLaunchInfo(l, languageInfo).compiler];
		});
		return (
			<div className={ styles.select }>
				<ThemeContext.Provider value={ FLAT_THEME }>
					<Select
						disabled={ !isEditable }
						width={ '100%' }
						items={ items }
						value={ language }
						onValueChange={ this.onLanguageSelectValueChange }
					/>
				</ThemeContext.Provider>
			</div>
		);
	};

	renderLanguageLaunchInfoTooltip = (): React.ReactElement => {
		const { deviceType, } = this.props;
		return (
			<ThemeContext.Provider value={ darkFlat }>
				<Tooltip trigger={ "hover" } render={ this.renderLanguageLaunchInfoTooltipContent }>
					<span className={ styles.launchInfoHelpIcon }>
						{ (deviceType !== DeviceType.mobile && deviceType !== DeviceType.tablet)
							? texts.compilationText
							: <Info/> }
					</span>
				</Tooltip>
			</ThemeContext.Provider>
		);
	};

	renderLanguageLaunchInfoTooltipContent = (): React.ReactNode => {
		const { language } = this.state;
		const { languageInfo } = this.props;

		return texts.getLanguageLaunchMarkup(texts.getLanguageLaunchInfo(language, languageInfo));
	};

	onLanguageSelectValueChange = (l: unknown): void => {
		this.setState({ language: l as Language });
	};

	renderHeader = (submissionColor: SubmissionColor, selectedSubmissionIsLast: boolean,
		selectedSubmissionIsLastSuccess: boolean
	): React.ReactNode => {
		const { currentSubmission, visibleCheckingResponse } = this.state;
		const { waitingForManualChecking, prohibitFurtherManualChecking, score } = this.props.slideProgress;
		if(!currentSubmission && !visibleCheckingResponse) {
			return null;
		}
		return (
			<ExerciseFormHeader
				solutionRunStatus={ visibleCheckingResponse ? visibleCheckingResponse.solutionRunStatus : null }
				selectedSubmission={ currentSubmission }
				waitingForManualChecking={ waitingForManualChecking }
				prohibitFurtherManualChecking={ prohibitFurtherManualChecking }
				selectedSubmissionIsLast={ selectedSubmissionIsLast }
				selectedSubmissionIsLastSuccess={ selectedSubmissionIsLastSuccess }
				score={ score }
				submissionColor={ submissionColor }
			/>
		);
	};

	loadSubmissionToState = (submission?: SubmissionInfoRedux): void => {
		this.clearAllTextMarkers();

		// Firstly we updating code in code mirror
		// when code is rendered we attaching reviewMarkers and loading reviews
		// after all is done we refreshing editor to refresh layout and sizes depends on reviews sizes
		if(submission) {
			this.setState({
					value: submission.code,
					language: submission.language,
					isEditable: false,
					valueChanged: false,
					showOutput: false,
					visibleCheckingResponse: undefined,
					currentReviews: [],
				}, () =>
					this.setCurrentSubmission(submission)
			);
		}
	};

	setCurrentSubmission = (submission: SubmissionInfoRedux, callback?: () => void): void => {
		this.clearAllTextMarkers();
		this.setState({
			currentSubmission: submission,
			currentReviews: this.getReviewsWithTextMarkers(submission),
		}, () => {
			const { editor } = this.state;
			if(editor) {
				editor.refresh();
			}
			if(callback) {
				callback();
			}
		});
	};

	openModal = <T extends ModalData<ModalType>>(data: T | null): void => {
		this.setState({
			modalData: data,
		});
	};

	openAcceptedSolutionsModal = (): void => {
		const { courseId, slideId, visitAcceptedSolutions, submissions, } = this.props;

		if(!HasSuccessSubmission(submissions)) {
			visitAcceptedSolutions(courseId, slideId);
		}
		this.openModal({ type: ModalType.acceptedSolutions });
	};

	getReviewsWithTextMarkers = (submission: SubmissionInfoRedux): ReviewInfoWithMarker[] => {
		const { exerciseCodeDoc } = this.state;
		const reviews = this.getAllReviewsFromSubmission(submission);

		const reviewsWithTextMarkers: ReviewInfoWithMarker[] = [];

		if(!exerciseCodeDoc) {
			return reviewsWithTextMarkers;
		}

		for (const review of reviews) {
			const { finishLine, finishPosition, startLine, startPosition } = review;
			const textMarker = this.highlightLine(finishLine, finishPosition, startLine, startPosition,
				styles.reviewCode,
				exerciseCodeDoc);

			reviewsWithTextMarkers.push({
				marker: textMarker,
				...review
			});
		}

		return reviewsWithTextMarkers;
	};

	getAllReviewsFromSubmission = (submission: SubmissionInfoRedux): ReviewInfoRedux[] => {
		if(!submission) {
			return [];
		}

		const manual = submission.manualCheckingReviews || [];
		const auto = submission.automaticChecking && submission.automaticChecking.reviews ? submission.automaticChecking.reviews : [];
		return manual.concat(auto);
	};

	renderOverview = (submission: SubmissionInfoRedux): React.ReactElement => {
		const { selfChecks } = this.state;
		const checkups = [
			{
				title: texts.checkups.self.title,
				content:
					<React.Fragment>
						<span className={ styles.overviewSelfCheckComment }>
							{ texts.checkups.self.text }
						</span>
						<ul className={ styles.overviewSelfCheckList }>
							{ this.renderSelfCheckBoxes(selfChecks) }
						</ul>
					</React.Fragment>
			},
		];

		const reviewsLength = submission?.automaticChecking?.reviews?.length || 0;
		if(reviewsLength !== 0) {
			checkups.unshift(
				{
					title: texts.checkups.bot.title,
					content:
						<span className={ styles.overviewComment }>
						{ texts.checkups.bot.countBotComments(reviewsLength) }
							<a onClick={ this.showFirstBotComment }>{ texts.checkups.showReview }</a>
					</span>
				});
		}

		if(submission.manualCheckingReviews.length !== 0) {
			const reviewsCount = submission?.manualCheckingReviews?.length || 0;

			checkups.unshift({
				title: texts.checkups.teacher.title,
				content:
					<span className={ styles.overviewComment }>
						{ texts.checkups.teacher.countTeacherReviews(reviewsCount) }
						<a onClick={ this.showFirstComment }>{ texts.checkups.showReview }</a>
					</span>
			});
		}

		return (
			<ul className={ styles.overview }>
				{ checkups.map(({ title, content }) =>
					<li key={ title } className={ styles.overviewLine } title={ title }>
						<h3>{ title }</h3>
						{ content }
					</li>
				) }
			</ul>
		);
	};

	renderSelfCheckBoxes = (selfChecks: SelfCheckup[]): React.ReactNode => {
		return (
			selfChecks.map(({ text, checked, onClick, }, i) =>
				<li key={ i }>
					<Checkbox checked={ checked } onClick={ onClick }/> <span
					className={ styles.selfCheckText }>{ text }</span>
				</li>
			)
		);
	};

	onSelfCheckBoxClick = (i: number): void => {
		const { selfChecks } = this.state;
		const newSelfChecks = [...selfChecks];

		newSelfChecks[i].checked = !newSelfChecks[i].checked;

		this.setState({
			selfChecks: newSelfChecks,
		});
	};

	renderModal = (modalData: ModalData<ModalType>): React.ReactNode => {
		const { hideSolutions, courseId, slideId, } = this.props;

		switch (modalData.type) {
			case ModalType.congrats: {
				const { score, waitingForManualChecking, } = modalData as CongratsModalData;
				const showAcceptedSolutions = !waitingForManualChecking && !hideSolutions;

				return (
					score && score > 0 &&
					<CongratsModal
						showAcceptedSolutions={ showAcceptedSolutions ? this.openAcceptedSolutionsModal : undefined }
						score={ score }
						waitingForManualChecking={ waitingForManualChecking || false }
						onClose={ this.closeModal }
					/>
				);
			}
			case ModalType.loginForContinue: {
				return (
					<LoginForContinue
						onClose={ this.closeModal }
					/>
				);
			}
			case ModalType.studentsSubmissions:
				break;
			case ModalType.acceptedSolutions: {
				return (
					<DownloadedHtmlContent
						url={ constructPathToAcceptedSolutions(courseId, slideId) }
						injectInWrapperAfterContentReady={ (html: React.ReactNode) =>
							<Modal onClose={ this.closeModal }>
								<Modal.Header>
									{ texts.acceptedSolutions.title }
								</Modal.Header>
								<Modal.Body>
									{ texts.acceptedSolutions.content }
									{ html }
								</Modal.Body>
							</Modal> }
					/>);
			}
		}
	};

	showFirstComment = (): void => {
		//TODO
	};

	showFirstBotComment = (): void => {
		//TODO
	};

	selectComment = (e: React.MouseEvent<Element, MouseEvent> | React.FocusEvent, id: number,): void => {
		const { isEditable, selectedReviewId, } = this.state;
		e.stopPropagation();

		if(!isEditable && selectedReviewId !== id) {
			this.highlightReview(id);
		}
	};

	highlightReview = (id: number): void => {
		const { currentReviews, selectedReviewId, editor, exerciseCodeDoc, } = this.state;
		const newCurrentReviews = [...currentReviews];

		if(!exerciseCodeDoc) {
			return;
		}

		if(selectedReviewId >= 0) {
			const selectedReview = newCurrentReviews.find(r => r.id === selectedReviewId);
			if(selectedReview) {
				const { from, to, } = selectedReview.marker.find() as MarkerRange;
				selectedReview.marker.clear();
				selectedReview.marker =
					this.highlightLine(to.line, to.ch, from.line, from.ch, styles.reviewCode, exerciseCodeDoc);
			}
		}

		let line = 0;
		if(id >= 0) {
			const review = newCurrentReviews.find(r => r.id === id);
			if(review) {
				const { from, to, } = review.marker.find() as MarkerRange;
				review.marker.clear();
				review.marker =
					this.highlightLine(to.line, to.ch, from.line, from.ch, styles.selectedReviewCode, exerciseCodeDoc);

				line = from.line;
			}
		}

		this.setState({
			currentReviews: newCurrentReviews,
			selectedReviewId: id,
		}, () => {
			if(id >= 0) {
				if(editor) {
					editor.scrollIntoView({ ch: 0, line, }, 200);
				}
			}
		});
	};

	highlightLine = (finishLine: number, finishPosition: number, startLine: number, startPosition: number,
		className: string, exerciseCodeDoc: Doc,
	): TextMarker => exerciseCodeDoc.markText({
		line: startLine,
		ch: startPosition
	}, {
		line: finishLine,
		ch: finishPosition
	}, {
		className,
	});

	resetCodeAndCache = (): void => {
		const { slideId, exerciseInitialCode, } = this.props;

		this.resetCode();
		this.saveCodeDraftToCache(slideId, exerciseInitialCode);
	};

	resetCode = (): void => {
		const { exerciseInitialCode, } = this.props;
		const savedPositionOfExercise = this.wrapper.current?.getBoundingClientRect();

		this.clearAllTextMarkers();
		this.setState({
			value: exerciseInitialCode,
			valueChanged: true,
			isEditable: true,
			currentSubmission: null,
			visibleCheckingResponse: undefined,
			currentReviews: [],
			showOutput: false,
			savedPositionOfExercise,
		});
	};

	scrollToBottomBorderIfNeeded = (): void => {
		const { savedPositionOfExercise } = this.state;

		const newPositionOfExercise = this.wrapper.current?.getBoundingClientRect();
		if(savedPositionOfExercise && newPositionOfExercise) {
			if(savedPositionOfExercise.top < 0 && savedPositionOfExercise.bottom > newPositionOfExercise.bottom) {
				window.scrollTo({
					left: 0,
					top: window.pageYOffset - (savedPositionOfExercise.bottom - newPositionOfExercise.bottom),
					behavior: "auto",
				});

				this.setState({
					savedPositionOfExercise: undefined,
				});
			}
		}
	};

	clearAllTextMarkers = (): void => {
		const { currentReviews, } = this.state;

		currentReviews.forEach(({ marker }) => marker.clear());

		this.setState({
			selectedReviewId: -1,
		});
	};

	loadNewTry = (): void => {
		const { slideId } = this.props;
		this.resetCode();
		this.loadLatestCode(slideId);
	};

	toggleOutput = (): void => {
		const { showOutput, } = this.state;

		this.setState({
			showOutput: !showOutput
		});
	};

	closeModal = (): void => {
		this.setState({
			modalData: null,
		});
	};

	sendExercise = (): void => {
		const { value, language } = this.state;
		const { courseId, slideId, sendCode, } = this.props;

		this.setState({
			submissionLoading: true,
			showOutput: false,
		});

		sendCode(courseId, slideId, value, language);
	};

	addReviewComment = (reviewId: number, text: string): void => {
		const { addReviewComment, courseId, slideId, } = this.props;
		const { currentSubmission, } = this.state;

		if(currentSubmission) {
			addReviewComment(courseId, slideId, currentSubmission.id, reviewId, text);
		}
	};

	deleteReviewComment = (reviewId: number, commentId: number,): void => {
		const { deleteReviewComment, courseId, slideId, } = this.props;
		const { currentSubmission, } = this.state;

		if(currentSubmission) {
			deleteReviewComment(courseId, slideId, currentSubmission.id, reviewId, commentId,);
		}
	};

	onBeforeChange = (editor: Editor, data: EditorChange, value: string): void => {
		this.setState({
			value,
			valueChanged: true,
		});
	};

	onEditorMount = (editor: Editor): void => {
		editor.setSize('auto', '100%');
		this.setState({
			exerciseCodeDoc: editor.getDoc(),
			editor,
		});
	};

	onCursorActivity = (): void => {
		const { currentReviews, exerciseCodeDoc, isEditable, } = this.state;
		if(exerciseCodeDoc) {
			const cursor = exerciseCodeDoc.getCursor();

			if(!isEditable && currentReviews.length > 0) {
				const reviewId = Exercise.getSelectedReviewIdByCursor(currentReviews, exerciseCodeDoc, cursor);
				this.highlightReview(reviewId);
			}
		}
	};

	static getSelectedReviewIdByCursor = (
		reviews: ReviewInfoWithMarker[],
		exerciseCodeDoc: Doc,
		cursor: CodeMirror.Position
	): number => {
		const { line, ch } = cursor;
		const reviewsUnderCursor = reviews.filter(r =>
			r.startLine <= line && r.finishLine >= line
			&& !(r.startLine === line && ch < r.startPosition)
			&& !(r.finishLine === line && r.finishPosition < ch)
		);

		if(reviewsUnderCursor.length === 0) {
			return -1;
		}

		reviewsUnderCursor.sort((a, b) => {
			const aLength = Exercise.getReviewSelectionLength(a, exerciseCodeDoc);
			const bLength = Exercise.getReviewSelectionLength(b, exerciseCodeDoc);
			if(aLength !== bLength) {
				return aLength - bLength;
			}

			return a.startLine !== b.startLine
				? a.startLine - b.startLine
				: a.startPosition !== b.startPosition
					? a.startPosition - b.startPosition
					: new Date(a.addingTime ?? Math.random() * 10).getTime()
					- new Date(b.addingTime ?? Math.random() * 10).getTime();
		});

		return reviewsUnderCursor[0].id;
	};

	static getReviewSelectionLength = (review: ReviewInfoWithMarker, exerciseCodeDoc: Doc): number =>
		exerciseCodeDoc.indexFromPos({ line: review.finishLine, ch: review.finishPosition })
		- exerciseCodeDoc.indexFromPos({ line: review.startLine, ch: review.startPosition });

	static loadLanguageStyles = (language: Language): string => {
		switch (language.toLowerCase()) {
			case Language.cSharp:
				require('codemirror/mode/clike/clike');
				return 'text/x-csharp';
			case Language.python2:
				require('codemirror/mode/python/python');
				return 'text/x-python';
			case Language.python3:
				require('codemirror/mode/python/python');
				return 'text/x-python';
			case Language.java:
				require('codemirror/mode/clike/clike');
				return 'text/x-java';
			case Language.javaScript:
				require('codemirror/mode/javascript/javascript');
				return 'text/javascript';
			case Language.html:
				require('codemirror/mode/xml/xml');
				return 'text/html';
			case Language.typeScript:
				require('codemirror/mode/javascript/javascript');
				return 'text/typescript';
			case Language.css:
				require('codemirror/mode/css/css');
				return 'text/css';
			case Language.haskell:
				require('codemirror/mode/haskell/haskell');
				return 'text/x-haskell';
			case Language.cpp:
				require('codemirror/mode/clike/clike');
				return 'text/x-c++src';
			case Language.c:
				require('codemirror/mode/clike/clike');
				return 'text/x-c';
			case Language.pgsql:
				require('codemirror/mode/sql/sql');
				return 'text/x-pgsql';
			case Language.mikrokosmos:
				return 'text/plain';

			case Language.text:
				return 'text/plain';

			case Language.jsx:
				require('codemirror/mode/jsx/jsx');
				return 'text/jsx';

			default:
				require('codemirror/mode/xml/xml');
				return 'text/html';
		}
	};

	loadLatestCode = (slideId: string): void => {
		const { submissions, } = this.props;
		const { language, } = this.state;

		const code = loadExerciseCodeFromCache(slideId);
		this.resetCode();

		if(submissions.length > 0 && code) {
			let newValue = code.value;

			const lastSubmission = submissions[this.lastSubmissionIndex];
			const lastSubmissionTime = convertDefaultTimezoneToLocal(lastSubmission.timestamp);
			const codeFromCacheTime = moment(code.time);

			if(lastSubmissionTime.diff(codeFromCacheTime, 'seconds') >= 0) { //if last submission is newer then last saved
				this.saveCodeDraftToCache(slideId, lastSubmission.code);
				newValue = lastSubmission.code;
			}

			this.setState({
				value: newValue,
				language: code.language ? code.language : language,
			});
			return;
		}

		if(submissions.length > 0) {
			const lastSubmission = submissions[this.lastSubmissionIndex];
			this.saveCodeDraftToCache(slideId, lastSubmission.code);
			this.setState({
				value: lastSubmission.code,
				language: lastSubmission.language,
			});
			return;
		}

		if(code) {
			this.setState({
				value: code.value,
				language: code.language ? code.language : language,
			});
		}
	};
}

export default Exercise;
