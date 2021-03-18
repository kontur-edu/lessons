import { CourseInfo, ScoringGroup, UnitInfo, UnitsInfo } from "src/models/course";
import { SlideUserProgress } from "src/models/userProgress";
import {
	CourseStatistics,
	FlashcardsStatistics,
	SlideProgressStatus,
	StartupSlideInfo,
	UnitProgressWithLastVisit
} from "../Navigation/types";
import { ScoringGroupsIds } from "src/consts/scoringGroup";
import { ShortSlideInfo, SlideType } from "src/models/slide";
import { flashcardsPreview } from "src/consts/routes";

export interface CourseSlideInfo {
	slideType: SlideType;
	previous?: ShortSlideInfo;
	current?: ShortSlideInfo & { firstInModule?: boolean; lastInModule?: boolean; };
	next?: ShortSlideInfo;
}

export function getCourseStatistics(
	units: UnitsInfo | null,
	progress: { [p: string]: SlideUserProgress },
	scoringGroups: ScoringGroup[],
	flashcardsStatisticsByUnits: { [unitId: string]: FlashcardsStatistics }
): CourseStatistics {
	const courseStatistics: CourseStatistics = {
		courseProgress: { current: 0, max: 0 },
		byUnits: {},
		flashcardsStatistics: { count: 0, unratedCount: 0 },
		flashcardsStatisticsByUnits,
	};

	if(!progress || scoringGroups.length === 0 || !units) {
		return courseStatistics;
	}

	for (const unit of Object.values(units)) {
		const unitStatistics = getUnitStatistics(unit, progress, scoringGroups,
			flashcardsStatisticsByUnits[unit.id]);

		courseStatistics.courseProgress.current += unitStatistics.current;
		courseStatistics.courseProgress.max += unitStatistics.max;
		courseStatistics.flashcardsStatistics.count += flashcardsStatisticsByUnits[unit.id]?.count || 0;
		courseStatistics.flashcardsStatistics.unratedCount += flashcardsStatisticsByUnits[unit.id]?.unratedCount || 0;
		courseStatistics.byUnits[unit.id] = unitStatistics;
	}

	return courseStatistics;
}

export const getUnitStatistics = (
	unit: UnitInfo,
	progress: { [p: string]: SlideUserProgress },
	scoringGroups: ScoringGroup[],
	flashcardsStatistics: FlashcardsStatistics,
): UnitProgressWithLastVisit => {
	const visitsGroup = scoringGroups.find(gr => gr.id === ScoringGroupsIds.visits);
	let unitScore = 0, unitMaxScore = 0, unitDoneSlidesCount = 0, unitInProgressSlidesCount = 0;
	const statusesBySlides: { [slideId: string]: SlideProgressStatus } = {};
	let mostPreferablySlideToOpen: StartupSlideInfo | null = null;

	for (const { maxScore, id, scoringGroup, type, quizMaxTriesCount, } of unit.slides) {
		statusesBySlides[id] = SlideProgressStatus.notVisited;
		const slideProgress = progress[id];

		if(slideProgress && slideProgress.visited) {
			const {
				usedAttempts,
				isSkipped,
				score,
				waitingForManualChecking,
				prohibitFurtherManualChecking,
				timestamp,
			} = slideProgress;

			switch (type) {
				case SlideType.Lesson: {
					statusesBySlides[id] = SlideProgressStatus.done;
					break;
				}
				case SlideType.Flashcards:
					statusesBySlides[id] = flashcardsStatistics.unratedCount === 0
						? SlideProgressStatus.done
						: SlideProgressStatus.canBeImproved;
					break;
				case SlideType.CourseFlashcards:
				case SlideType.Quiz: {
					statusesBySlides[id] = (score === maxScore || usedAttempts >= quizMaxTriesCount) && !waitingForManualChecking && !prohibitFurtherManualChecking
						? SlideProgressStatus.done
						: SlideProgressStatus.canBeImproved;
					break;
				}
				case SlideType.Exercise: {
					statusesBySlides[id] = score === maxScore || !waitingForManualChecking && score > 0 || prohibitFurtherManualChecking || isSkipped
						? SlideProgressStatus.done
						: SlideProgressStatus.canBeImproved;
					break;
				}
			}

			const timestampAsDate = new Date(timestamp);
			if(!mostPreferablySlideToOpen) {
				mostPreferablySlideToOpen = {
					id,
					timestamp: new Date(timestamp),
					status: statusesBySlides[id],
				};
			} else if((mostPreferablySlideToOpen.timestamp.getTime() < timestampAsDate.getTime() || statusesBySlides[id] === SlideProgressStatus.canBeImproved)
				&& mostPreferablySlideToOpen.status !== SlideProgressStatus.canBeImproved) {
				mostPreferablySlideToOpen = {
					id,
					timestamp: new Date(timestamp),
					status: statusesBySlides[id],
				};
			}
		}

		if(statusesBySlides[id] === SlideProgressStatus.done) {
			unitDoneSlidesCount++;
		}
		if(statusesBySlides[id] === SlideProgressStatus.canBeImproved) {
			unitInProgressSlidesCount++;
		}

		const group = scoringGroups.find(gr => gr.id === scoringGroup);
		if(visitsGroup) {
			unitMaxScore += visitsGroup.weight;
			if(progress[id] && progress[id].visited) {
				unitScore += visitsGroup.weight;
			}
		}

		if(group && maxScore) {
			unitMaxScore += maxScore * group.weight;
			if(progress[id] && progress[id].score) {
				unitScore += progress[id].score * group.weight;
			}
		}
	}
	return {
		doneSlidesCount: unitDoneSlidesCount,
		inProgressSlidesCount: unitInProgressSlidesCount,
		slidesCount: unit.slides.length,
		statusesBySlides,
		current: unitScore,
		max: unitMaxScore,
		startupSlide: mostPreferablySlideToOpen,
	};
};

export function findUnitIdBySlideId(slideId ?: string, courseInfo ?: CourseInfo): string | null {
	if(!courseInfo || !courseInfo.units) {
		return null;
	}

	const units = courseInfo.units;

	for (const unit of units) {
		for (const slide of unit.slides) {
			if(slideId === slide.id) {
				return unit.id;
			}
		}
	}

	return null;
}

export function getSlideInfoById(
	slideId ?: string,
	courseInfo ?: CourseInfo
): CourseSlideInfo | null {
	if(!courseInfo || !courseInfo.units) {
		return null;
	}

	const units = courseInfo.units;
	let prevSlide, nextSlide;

	for (let i = 0; i < units.length; i++) {
		const { slides } = units[i];
		for (let j = 0; j < slides.length; j++) {
			const slide = slides[j] as ShortSlideInfo & { firstInModule: boolean, lastInModule: boolean };

			if(slide.id === slideId) {
				if(j > 0) {
					prevSlide = slides[j - 1];
				} else if(i > 0) {
					const prevSlides = units[i - 1].slides;
					slide.firstInModule = true;
					prevSlide = prevSlides[prevSlides.length - 1];
				}

				if(j < slides.length - 1) {
					nextSlide = slides[j + 1];
				} else if(i < units.length - 1) {
					const nextSlides = units[i + 1].slides;
					slide.lastInModule = true;
					nextSlide = nextSlides[0];
				}

				return { slideType: slide.type, previous: prevSlide, current: slide, next: nextSlide };
			}
		}
	}
	const slideType = slideId === flashcardsPreview
		? SlideType.PreviewFlashcards
		: SlideType.CourseFlashcards;

	return { slideType };
}

export function findNextUnit(activeUnit: UnitInfo, courseInfo: CourseInfo): UnitInfo | null {
	const units = courseInfo.units;
	const activeUnitId = activeUnit.id;

	const indexOfActiveUnit = units.findIndex(item => item.id === activeUnitId);

	if(indexOfActiveUnit < 0 || indexOfActiveUnit === units.length - 1) {
		return null;
	}

	return units[indexOfActiveUnit + 1];
}
