import React from "react";
import { Link } from "react-router-dom";
import classnames from 'classnames';

import { DocumentLite, EyeClosed, } from "icons";
import { Hint } from "ui";

import { SlideType } from 'src/models/slide';
import { MenuItem, SlideProgressStatus } from "../../types";

import styles from './NavigationItem.less';


interface Props extends MenuItem<SlideType> {
	metro: {
		isFirstItem: boolean;
		isLastItem: boolean;
		connectToPrev: boolean;
		connectToNext: boolean;
	},
	onClick: () => void;
	getRefToActive?: React.RefObject<HTMLLIElement>;
}

function NavigationItem({
	title,
	url,
	isActive,
	metro,
	onClick,
	hide,
	score,
	maxScore,
	type,
	status,
	containsVideo,
	getRefToActive,
}: Props): React.ReactElement {
	const classes = {
		[styles.itemLink]: true,
		[styles.active]: isActive,
	};

	return (
		<li className={ styles.root } ref={ isActive ? getRefToActive : undefined }>
			<Link to={ url } className={ classnames(classes) } onClick={ onClick }>
				{ metro && renderMetro() }
				<div className={ styles.firstLine }>
					<span className={ styles.icon }>
						{ renderIcon() }
					</span>
					<span className={ styles.text }>
						{ title }
						{ hide && <span className={ styles.isHiddenIcon }>
							<Hint text={ `Этот слайд скрыт` }>
								<EyeClosed/>
							</Hint>
						</span> }
					</span>
					{ renderScore() }
				</div>
			</Link>
		</li>
	);

	function renderIcon() {
		switch (type) {
			case SlideType.Exercise:
				return <svg className={ styles.svg } viewBox={ "0 0 10 10" } fill={ "none" }
							xmlns={ "http://www.w3.org/2000/svg" }>
					<path
						d="M2.77778 0C2.48309 0 2.20048 0.117063 1.9921 0.325437C1.78373 0.533811 1.66667 0.816426 1.66667 1.11111V3.33333C1.66667 3.62802 1.5496 3.91063 1.34123 4.11901C1.13286 4.32738 0.850241 4.44444 0.555556 4.44444H0V5.55556H0.555556C0.850241 5.55556 1.13286 5.67262 1.34123 5.88099C1.5496 6.08937 1.66667 6.37198 1.66667 6.66667V8.88889C1.66667 9.18357 1.78373 9.46619 1.9921 9.67456C2.20048 9.88294 2.48309 10 2.77778 10H3.88889V8.88889H2.77778V6.11111C2.77778 5.81643 2.66071 5.53381 2.45234 5.32544C2.24397 5.11706 1.96135 5 1.66667 5C1.96135 5 2.24397 4.88294 2.45234 4.67456C2.66071 4.46619 2.77778 4.18357 2.77778 3.88889V1.11111H3.88889V0H2.77778ZM7.22222 0C7.51691 0 7.79952 0.117063 8.0079 0.325437C8.21627 0.533811 8.33333 0.816426 8.33333 1.11111V3.33333C8.33333 3.62802 8.4504 3.91063 8.65877 4.11901C8.86714 4.32738 9.14976 4.44444 9.44444 4.44444H10V5.55556H9.44444C9.14976 5.55556 8.86714 5.67262 8.65877 5.88099C8.4504 6.08937 8.33333 6.37198 8.33333 6.66667V8.88889C8.33333 9.18357 8.21627 9.46619 8.0079 9.67456C7.79952 9.88294 7.51691 10 7.22222 10H6.11111V8.88889H7.22222V6.11111C7.22222 5.81643 7.33929 5.53381 7.54766 5.32544C7.75603 5.11706 8.03865 5 8.33333 5C8.03865 5 7.75603 4.88294 7.54766 4.67456C7.33929 4.46619 7.22222 4.18357 7.22222 3.88889V1.11111H6.11111V0H7.22222Z"
						fill="#A0A0A0"/>
				</svg>;
			case SlideType.Lesson:
				if(containsVideo) {
					return <svg
						className={ styles.svg }
						viewBox={ "-2 -2 14 14" }
						fill={ "none" }
						xmlns={ "http://www.w3.org/2000/svg" }>
						<path
							d={ "M 9.5 4.5 L 9.4472 5.3944 L 11.2361 4.5 L 9.4472 3.6056 L 9.5 4.5 Z M 0 -0 L 0.4472 -0.8944 L -1 -1.618 V -0 H 0 Z M 0 9 H -1 V 10.618 L 0.4472 9.8944 L 0 9 Z M 9.4472 3.6056 L 0.4472 -0.8944 L 0 0 l 9.5 4.5 L 9.4472 3.6056 Z M -1 -0 V 9 H 0 V 0 H -1 Z M 0.4472 9.8944 L 9.4472 5.3944 L 9.5 4.5 L 0 9 L 0.4472 9.8944 Z" }
							fill={ "currentColor" }/>
					</svg>;
				}
				return <DocumentLite/>;
			case SlideType.Quiz:
			case SlideType.Flashcards:
				return <svg className={ styles.svg } viewBox={ "0 0 11 11" }
							fill={ 'none' }
							xmlns="http://www.w3.org/2000/svg">
					<path fillRule={ 'evenodd' }
						  clipRule={ "evenodd" }
						  d={ "M5.5 0.912195C2.96623 0.912195 0.912195 2.96623 0.912195 5.5C0.912195 8.03378 2.96623 10.0878 5.5 10.0878C8.03377 10.0878 10.0878 8.03378 10.0878 5.5C10.0878 2.96623 8.03377 0.912195 5.5 0.912195ZM0 5.5C0 2.46243 2.46243 0 5.5 0C8.53757 0 11 2.46243 11 5.5C11 8.53757 8.53757 11 5.5 11C2.46243 11 0 8.53757 0 5.5Z" }
						  fill={ "currentColor" }/>
					<path
						d={ "M4.1273 3.05353C4.47683 2.68146 4.95665 2.49538 5.56675 2.49538C6.13113 2.49538 6.58304 2.65453 6.92238 2.97283C7.26182 3.29124 7.4316 3.69797 7.4316 4.19313C7.4316 4.49308 7.3701 4.73637 7.2469 4.92321C7.12392 5.10994 6.87162 5.38446 6.49021 5.74665C6.2128 6.0098 6.03218 6.23259 5.94847 6.41513C5.86477 6.59757 5.82291 6.86715 5.82291 7.22366H5.0674C5.0674 6.81897 5.11548 6.49294 5.21185 6.24536C5.30801 5.99767 5.5221 5.71414 5.85393 5.39433L6.20035 5.05897C6.30423 4.96281 6.38847 4.86236 6.45308 4.75751C6.5707 4.57368 6.6294 4.38265 6.6294 4.18455C6.6294 3.90724 6.54494 3.66674 6.37592 3.46295C6.207 3.25926 5.92744 3.15731 5.53745 3.15731C5.05495 3.15731 4.72141 3.33277 4.5364 3.68359C4.43241 3.8788 4.37317 4.1604 4.35879 4.52828H3.60327C3.60306 3.91733 3.77777 3.4256 4.1273 3.05353ZM5.05044 7.9283H5.89514V8.8112H5.05044V7.9283Z" }
						fill={ 'currentColor' }/>
				</svg>;
		}
	}

	function renderScore() {
		if(!maxScore) {
			return;
		}

		if(type === SlideType.Exercise || type === SlideType.Quiz) {
			return (
				<span className={ styles.score }>{ score || 0 }/{ maxScore }</span>
			);
		}
	}

	function renderMetro() {
		if(!metro) {
			return null;
		}

		const { isFirstItem, isLastItem, connectToPrev, connectToNext } = metro;

		const classes = {
			[styles.metroWrapper]: true,
			[styles.withoutBottomLine]: isLastItem,
			[styles.noTopLine]: isFirstItem,
			[styles.completeTop]: connectToPrev,
			[styles.completeBottom]: connectToNext,
		};

		return (
			<div className={ classnames(classes) }>
				<span className={ classnames(styles.pointer,
					{ [styles.canBeImproved]: status === SlideProgressStatus.canBeImproved },
					{ [styles.complete]: status === SlideProgressStatus.done }) }/>
			</div>
		);
	}
}

export default NavigationItem;

