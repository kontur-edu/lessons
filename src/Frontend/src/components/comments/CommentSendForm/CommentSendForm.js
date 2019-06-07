import React, { Component } from "react";
import PropTypes from "prop-types";
import { userType } from "../commonPropTypes";
import Button from "@skbkontur/react-ui/components/Button/Button";
import Avatar from "../../common/Avatar/Avatar";
import MarkdownEditor from "./MarkdownEditor/MarkdownEditor";

import styles from "./CommentSendForm.less";

class CommentSendForm extends Component {
	constructor(props) {
		super(props);

		this.state = {
			text: props.text || "",
			error: null,
			commentId: props.commentId,
			status: props.sendStatus,
		};

		this.editor = React.createRef();
	}

	static getDerivedStateFromProps(props, state) {
		if (props.commentId !== state.commentId) {
			return {
				text: props.text || "",
				commentId: props.commentId,
			};
		}
		return null;
	}

	render() {
		const {author, isForInstructors, isShowFocus} = this.props;
		const {error, text} = this.state;

		return (
			<div className={styles.formContainer}>
				{author && (
					<div className={`${styles.avatar} ${styles.visibleOnDesktopAndTablet}`}>
						<Avatar user={author} size="big" />
					</div>
				)}
				<form className={styles.form} onSubmit={this.handleSubmit}>
					<MarkdownEditor
						isShowFocus={isShowFocus}
						ref={this.editor}
						isForInstructors={isForInstructors}
						hasError={error !== null}
						text={text}
						handleChange={this.handleChange}
						handleSubmit={this.handleSubmit}>
						<div className={styles.buttons}>
							{this.renderSubmitButton()}
							{this.renderCancelButton()}
						</div>
					</MarkdownEditor>
				</form>
			</div>
		)
	}

	renderSubmitButton() {
		const {submitTitle = "Оставить комментарий", sending} = this.props;

		return (
			<Button
				use="primary"
				size={window.matchMedia("(max-width: 767px)").matches ? "small" : "medium"}
				type="submit"
				loading={sending}>
				{submitTitle}
			</Button>
		);
	}

	renderCancelButton() {
		const {handleCancel, cancelTitle = "Отменить"} = this.props;

		if (!handleCancel) {
			return null;
		}

		return (
			<Button
				use="secondary"
				size={window.matchMedia("(max-width: 767px)").matches ? "small" : "medium"}
				type="button"
				onClick={handleCancel}>
				{cancelTitle}
			</Button>
		);
	}

	handleSubmit = (event) => {
		event.preventDefault();

		const {text} = this.state;

		if (!text) {
			this.setState({
				error: "Заполните поле комментария",
			});
			return;
		}

		const {commentId, handleSubmit} = this.props;

		handleSubmit(commentId, text);
	};

	handleChange = (text, callback) => {
		this.setState({text, error: null}, callback);
	};
}

CommentSendForm.propTypes = {
	author: userType,
	commentId: PropTypes.number,
	handleSubmit: PropTypes.func,
	sendStatus: PropTypes.string,
	isForInstructors: PropTypes.bool,
	isShowFocus: PropTypes.objectOf(PropTypes.bool),
	sending: PropTypes.bool,
	submitTitle: PropTypes.string,
	handleCancel: PropTypes.func,
	cancelTitle: PropTypes.string,
};

export default CommentSendForm;