import React from "react";

import { Ok } from "icons";

import styles from "./ProgressBar.less";
import cn from "classnames";

interface Props {
	successValue: number;
	inProgressValue: number;

	active?: boolean;
	big?: boolean;
	startAngle?: number;
	fillDirection?: number;
}


function ProgressBarCircle({
	successValue,
	inProgressValue,
	active,
	big,
	startAngle = -90,
	fillDirection = 1
}: Props): React.ReactElement {
	const radius = big ? 10 : 8;

	if(successValue >= 1) {
		return (
			<span className={ cn(styles.circleFull, { [styles.big]: big }) }>
				<Ok/>
			</span>
		);
	}

	return (
		<svg className={ cn(styles.circleSvgWrapper, { [styles.big]: big }) }
			 viewBox={ `0 0 ${ radius * 2 } ${ radius * 2 }` }>
			<circle cx={ radius } cy={ radius }
					r={ radius / 2 - 0.5 }//-0.5 to prevent appearing of small gray line at the edge of circle
					className={ cn(styles.circleProgressCircle, { [styles.active]: active }, { [styles.big]: big }) }/>
			{ renderParts(successValue, inProgressValue) }
		</svg>
	);

	function renderParts(successValue: number, inProgressValue: number) {
		const renderedParts = [];
		let curStartAngle = startAngle;
		if(successValue) {
			renderedParts.push(
				<path
					key={ 'success' }
					className={ cn(styles.circleProgressValue, { [styles.big]: big }, styles.success) }
					d={ makeSectorPath(radius, radius, radius / 2, curStartAngle, successValue * 360, fillDirection) }
				/>
			);
			curStartAngle += fillDirection * successValue * 360;
		}
		if(inProgressValue) {
			renderedParts.push(
				<path
					key={ 'inProgress' }
					className={ cn(styles.circleProgressValue, { [styles.big]: big }, styles.inProgress) }
					d={ makeSectorPath(radius, radius, radius / 2, curStartAngle, inProgressValue * 360,
						fillDirection) }
				/>
			);
		}

		return renderedParts;
	}
}


//this functions has been copied from https://github.com/tigrr/circle-progress/tree/1ecb3cef5c675b3c9bc1ec6fea56cb89ffe5553a
function makeSectorPath(cx: number, cy: number, r: number, startAngle: number, angle: number, clockwise: number) {
	if(angle > 0 && angle < 0.3) {
		// Tiny angles smaller than ~0.3° can produce weird-looking paths
		angle = 0;
	} else if(angle > 359.999) {
		// If progress is full, notch it back a little, so the path doesn't become 0-length
		angle = 359.999;
	}
	const endAngle = startAngle + angle * (clockwise * 2 - 1),
		startCoords = polarToCartesian(r, startAngle),
		endCoords = polarToCartesian(r, endAngle),
		x1 = cx + startCoords.x,
		x2 = cx + endCoords.x,
		y1 = cy + startCoords.y,
		y2 = cy + endCoords.y;

	return ["M", x1, y1, "A", r, r, 0, +(angle > 180), +clockwise, x2, y2].join(' ');
}

const polarToCartesian = (r: number, angle: number,) => ({
	x: r * Math.cos(angle * Math.PI / 180),
	y: r * Math.sin(angle * Math.PI / 180),
});

export default ProgressBarCircle;
