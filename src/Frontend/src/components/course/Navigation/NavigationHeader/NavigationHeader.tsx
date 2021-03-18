import React from "react";
import cn from "classnames";

import { Link } from 'ui';
import { ArrowChevron2Left } from "icons";
import ProgressBar from "../ProgressBar";
import ProgressBarCircle from "../ProgressBar/ProgressBarCircle";

import { buildQuery } from "src/utils";

import { Progress, UnitProgress } from '../types';
import { GroupAsStudentInfo } from "src/models/groups";
import { DeviceType } from "src/consts/deviceType";
import { courseStatistics } from "src/consts/routes";

import styles from './NavigationHeader.less';

interface Props {
	className?: string;
	title: string;
	courseProgress: Progress;
	groupsAsStudent: GroupAsStudentInfo[];

	unitTitle?: string;
	unitProgress?: UnitProgress;

	isInsideCourse: boolean;

	deviceType: DeviceType;

	returnToCourseNavigationClicked: () => void;

	createRef: React.RefObject<HTMLDivElement>;
}

function NavigationHeader({
	title,
	unitProgress,
	unitTitle,
	className,
	groupsAsStudent,
	courseProgress,
	returnToCourseNavigationClicked,
	createRef,
	isInsideCourse,
	deviceType,
}: Props): React.ReactElement {
	return (
		<header ref={ createRef }
				className={ cn(styles.root, styles.sticky, className) }>
			{ isInsideCourse
				? <h1 className={ styles.courseTitle } title={ title }>{ title }</h1>
				: renderReturnToCourseNavigationLink()
			}
			<>
				{ groupsAsStudent.length > 0 && renderLinkToGroupsStatements(groupsAsStudent) }
				{ renderCourseProgress() }
			</>
			{
				unitTitle && renderUnitSection(unitProgress)
			}
		</header>
	);

	function renderCourseProgress() {
		const percentage = courseProgress.current / courseProgress.max;

		if(percentage) {
			return (
				<div className={ styles.progressBarWrapper }>
					<ProgressBar value={ percentage }/>
				</div>
			);
		}
	}

	function renderReturnToCourseNavigationLink() {
		return (
			<nav className={ styles.returnToCourseLink }>
				<Link
					icon={ <ArrowChevron2Left className={ styles.returnToCourseLinkIcon }/> }
					onClick={ returnToCourseNavigationClicked }>
					<span className={ styles.returnToCourseLinkText }>{ title }</span>
				</Link>
			</nav>
		);
	}

	function renderLinkToGroupsStatements(groupsAsStudent: GroupAsStudentInfo[]) {
		const groupsLinks = [];

		for (let i = 0; i < groupsAsStudent.length; i++) {
			const { id, courseId, name, } = groupsAsStudent[i];
			const courseIdInLowerCase = courseId.toLowerCase();

			groupsLinks.push(
				<Link
					key={ id }
					href={ courseStatistics + buildQuery({ courseId: courseIdInLowerCase, group: id }) }>
					{ name }
				</Link>
			);

			if(i < groupsAsStudent.length - 1) {
				groupsLinks.push(', ');
			}
		}

		return <p className={ cn(
			styles.linkToGroupsStatementsWrapper,
			{ [styles.insideModule]: !isInsideCourse }) }>Ведомость { groupsLinks }</p>;
	}

	function renderUnitSection(unitProgress?: UnitProgress) {
		return (
			<div className={ styles.unitRoot }>
				<h2 className={ styles.unitTitle } title={ unitTitle }>
					{ unitTitle }
				</h2>
				{ unitProgress && renderUnitProgress(unitProgress) }
			</div>
		);
	}

	function renderUnitProgress(unitProgress: UnitProgress) {
		if(unitProgress.inProgressSlidesCount > 0 || unitProgress.doneSlidesCount > 0) {
			return (
				<span className={ styles.circleProgressBarWrapper }>
					<ProgressBarCircle
						big={ deviceType !== DeviceType.mobile }
						successValue={ unitProgress.doneSlidesCount / unitProgress.slidesCount }
						inProgressValue={ unitProgress.inProgressSlidesCount / unitProgress.slidesCount }
					/>
				</span>
			);
		}
	}
}

export default NavigationHeader;
