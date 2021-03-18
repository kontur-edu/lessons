import React from "react";
import ProgressBar from "./ProgressBar";
import ProgressBarCircle from "./ProgressBarCircle";
import { Gapped } from "ui";

function getProgressBar(value: number, small?: boolean) {
	return <>
		<b>value={ value }:</b>
		<ProgressBar value={ value } small={ small }/>
	</>;
}

function getCircleProgressBar(inProgressValue: number, successValue: number, big?: boolean) {
	return <>
		<b>success={ successValue } | inProgress={ inProgressValue }:</b>
		<br/>
		<ProgressBarCircle inProgressValue={ inProgressValue } successValue={ successValue } big={ big }/>
	</>;
}

const values = [0, 0.25, 0.36, 0.45, 0.5, 0.67, 0.85, 1, 5,];
const moduleValues = [
	[0, 0],
	[0, 0.1],
	[0, 0.25],
	[0, 0.33],
	[0, 0.5],
	[0, 0.75],
	[0, 0.88],
	[0, 0.99],
	[0, 1],
	[0.1, 0],
	[0.1, 0.1],
	[0.25, 0.25],
	[0.5, 0.5],
	[0.75, 0.25],
	[1, 0],
	[0.95, 0.05],
];

const ProgressBars = (values: React.ReactNode[]): React.ReactNode => (
	<div style={ { width: 300 } }>
		<Gapped vertical>
			{ values }
		</Gapped>
	</div>
);

export default {
	title: "ProgressBar",
};

const CourseBars = (): React.ReactNode => ProgressBars(values.map(v => getProgressBar(v)));
export { CourseBars };

const CourseBarsInModule = (): React.ReactNode => ProgressBars(values.map(v => getProgressBar(v, true)));
export { CourseBarsInModule };

const ModuleBars = (): React.ReactNode => ProgressBars(moduleValues.map(v => getCircleProgressBar(v[0], v[1])));
export { ModuleBars };

const ModuleBarsInHeader = (): React.ReactNode => ProgressBars(
	moduleValues.map(v => getCircleProgressBar(v[0], v[1], true)));
export { ModuleBarsInHeader };
