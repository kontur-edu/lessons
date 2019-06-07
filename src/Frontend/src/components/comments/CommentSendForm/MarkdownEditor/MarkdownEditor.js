import React, { Component } from "react";
import PropTypes from "prop-types";
import BaseTextarea from "@skbkontur/react-ui/components/Textarea/Textarea";
import { boldIcon, codeIcon, italicIcon } from "../../SVGIcons/SVGIcon";
import MarkdownButtons from "../MarkdownButtons/MarkdownButtons";

import styles from "./MarkdownEditor.less";

const markupByOperation = {
	bold: {
		markup: "**",
		description: "жирный",
		hotkey: {
			asText: "Ctrl + B",
			ctrl: true,
			key: ["b", "и"],
		},
		icon: boldIcon,
	},
	italic: {
		markup: "__",
		description: "курсив",
		hotkey: {
			asText: "Ctrl + I",
			ctrl: true,
			key: ["i", "ш"],
		},
		icon: italicIcon,
	},
	code: {
		markup: "```",
		description: "блок кода",
		hotkey: {
			asText: "Alt + Q",
			alt: true,
			key: ["q", "й"],
		},
		icon: codeIcon,
	},
};

// monkey patch
class Textarea extends BaseTextarea {
	get selectionRange() {
		if (!this.node) {
			throw new Error(`Cannot call ${this.selectionRange} on unmounted Input`);
		}

		return {
			start: this.node.selectionStart,
			end: this.node.selectionEnd,
		};
	};
}

class MarkdownEditor extends Component {
	textarea = React.createRef();

	componentDidMount() {
		const {inSendForm, inEditForm, inReplyForm} = this.props.isShowFocus;

		if (inSendForm || inEditForm || inReplyForm) {
			this.textarea.current.focus();
		}
	}

	render() {
		const {hasError, text, children, isForInstructors} = this.props;

		return (
			<>
				<Textarea
					ref={this.textarea}
					value={text}
					width={"100%"}
					error={hasError}
					maxRows={15}
					rows={4}
					onChange={this.handleChange}
					onKeyDown={this.handleKeyDown}
					autoResize
					placeholder={isForInstructors ? "Комментарий для преподавателей" : "Комментарий"} />
				<div className={styles.formFooter}>
					{children}
					<MarkdownButtons
						markupByOperation={markupByOperation}
						onClick={this.handleClick} />
				</div>
			</>
		)
	}

	handleChange = (e, value) => {
		const {handleChange} = this.props;

		handleChange(value);
	};

	handleKeyDown = (e) => {
		for (let operation of Object.values(markupByOperation)) {
			if (this.isKeyFromMarkdownOperation(e, operation)) {
				this.transformTextToMarkdown(operation);
				return;
			}
		}

		if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
			this.handleChange();
			this.props.handleSubmit(window.event);
		}
	};

	isKeyFromMarkdownOperation = (event, operation) => {
		return operation.hotkey.key.includes(event.key) &&
			(event.ctrlKey || event.metaKey) === !!operation.hotkey.ctrl &&
			event.altKey === !!operation.hotkey.alt;
	};

	handleClick = (operation) => {
		this.transformTextToMarkdown(operation);
	};

	transformTextToMarkdown = (operation) => {
		const {handleChange, text} = this.props;
		const range = this.textarea.current.selectionRange;

		let {finalText, finalSelectionRange} = MarkdownEditor.wrapRangeWithMarkdown(text, range, operation);

		handleChange(finalText, () => {
			this.textarea.current.setSelectionRange(
				finalSelectionRange.start,
				finalSelectionRange.end,
			);
		});
	};

	static wrapRangeWithMarkdown(text, range, operation) {
		const isErrorInRangeType = typeof range !== "object" || range === null;
		const isErrorInStartEndAvailable = !("start" in range) || !("end" in range);
		const isErrorInStartEndType = typeof range.start !== "number" || typeof range.end !== "number";
		const isErrorInRangeLength = range.start < 0 || range.end < range.start || range.end > text.length;

		if (typeof text !== "string") {
			throw new TypeError("text should be a string");
		}

		if (isErrorInRangeType|| isErrorInStartEndAvailable || isErrorInStartEndType) {
			throw new TypeError("range should be an object with `start` and `end` properties of type `number`");
		}

		if (isErrorInRangeLength) {
			throw new RangeError("range should be within 0 and text.length");
		}

		const before = text.substr(0, range.start);
		const target = text.substr(range.start, range.end - range.start);
		const after = text.substr(range.end);
		const formatted = operation.markup + target + operation.markup;
		const finalText = before + formatted + after;
		const finalSelectionRange = {};

		if (target.length === 0) {
			finalSelectionRange.start = range.start + operation.markup.length;
			finalSelectionRange.end = range.end + operation.markup.length;
		} else {
			finalSelectionRange.start = range.start;
			finalSelectionRange.end = range.start + formatted.length;
		}

		return {finalText, finalSelectionRange};
	}
}

MarkdownEditor.propTypes = {
	hasError: PropTypes.bool,
	isShowFocus: PropTypes.objectOf(PropTypes.bool),
	isForInstructors: PropTypes.bool,
	text: PropTypes.string,
	handleChange: PropTypes.func,
	children: PropTypes.element,
};

export default MarkdownEditor;