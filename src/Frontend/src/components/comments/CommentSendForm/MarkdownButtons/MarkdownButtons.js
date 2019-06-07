import React, { Component } from "react";
import PropTypes from "prop-types";
import { markupOperation } from "../../commonPropTypes";
import Hint from "@skbkontur/react-ui/components/Hint/Hint";

import styles from "./MarkdownButtons.less";

class MarkdownButtons extends Component {
	render() {
		const {markupByOperation} = this.props;

		return (
			<div className={styles.markdownButtons}>
				<span className={styles.markdownText}>Поддерживаем Markdown</span>
				{Object.entries(markupByOperation)
				.map(([name, operation]) =>
					<div key={name} className={styles.buttonBlock}>
						{!window.matchMedia("(max-width: 767px)").matches ?
						<Hint
							pos="bottom"
							text={this.renderHint(operation)}>
							{this.renderMarkdownButton(name, operation)}
						</Hint> : this.renderMarkdownButton(name, operation)}
					</div>)}
			</div>
		);
	}

	renderMarkdownButton(name, operation) {
		return (
			<button
				className={styles.button}
				onClick={this.handleClick(operation)} type="button">
				<svg
					width={20}
					height={18}
					xmlns="http://www.w3.org/2000/svg"
					fill="none">
					{operation.icon}
				</svg>
			</button>
		)
	}

	renderHint({description, markup, hotkey}) {
		return (
			<span className={styles.lightYellow}>
				{markup}
				<span className={styles.white}>{description}</span>
				{markup}<br />
				{!window.matchMedia("(max-width: 767px)").matches &&
				<span className={styles.lightYellow}>{hotkey.asText}</span>}
			</span>)
	};

	handleClick = (operation) => () => {
		this.props.onClick(operation);
	};
}

MarkdownButtons.propTypes = {
	markupByOperation: markupOperation,
	onClick: PropTypes.func,
};

export default MarkdownButtons;