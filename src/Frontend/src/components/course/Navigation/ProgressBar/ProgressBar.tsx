import React from "react";
import classnames from 'classnames';
import styles from "./ProgressBar.less";

interface Props {
	value: number;
	small?: boolean;
	active?: boolean;
}

function ProgressBar({ value, small, active, }: Props): React.ReactElement {
	return (
		<div className={ classnames(styles.wrapper, { [styles.small]: small }, { [styles.active]: active }) }>
			<div
				className={ styles.value }
				style={ { width: `${ value * 100 }%` } }/>
		</div>
	);
}

export default ProgressBar;
