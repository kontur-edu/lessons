import React, { Component } from "react";
import PropTypes from "prop-types";
import { userType, userRoles, comment, commentStatus } from "../commonPropTypes";
import { TransitionGroup, CSSTransition } from "react-transition-group";
import Comment from "../Comment/Comment";
import CommentSendForm from "../CommentSendForm/CommentSendForm";

import styles from "./Thread.less";

class Thread extends Component {

	render() {
		const {comment} = this.props;
		const replies = comment.replies || [];
		return this.renderComment(comment, replies.length === 0);
	}

	renderComment(comment, isLastChild) {
		const {user, userRoles, reply, commentEditing, actions, getUserSolutionsUrl, slideType} = this.props;
		const isLastCommentInThread = isLastChild;
		const isParentComment = !comment.parentCommentId;

		return (
			<>
				<Comment
					key={comment.id}
					comment={comment}
					hasReplyAction={isLastCommentInThread}
					commentEditing={commentEditing}
					actions={actions}
					getUserSolutionsUrl={getUserSolutionsUrl}
					slideType={slideType}
					user={user}
					userRoles={userRoles}>
					<div className={styles.visibleOnDesktopAndTablet}>
						{this.renderReplies(comment)}
					</div>
					{(isParentComment && comment.id === this.props.reply.commentId) &&
					<div className={styles.replyForm}>
						<CommentSendForm
							commentId={comment.id}
							sending={reply.sending}
							author={user}
							submitTitle="Отправить"
							handleCancel={() => actions.handleShowReplyForm(null)}
							handleSubmit={actions.handleAddReplyComment} />
					</div>}
				</Comment>
				<div className={styles.visibleOnPhone}>
					{this.renderReplies(comment)}
				</div>
			</>
		)
	}

	renderReplies(comment) {
		const replies = comment.replies || [];
		const transitionStyles = {
			enter: styles.enter,
			exit: styles.exit,
			enterActive: styles.enterActive,
			exitActive: styles.exitActive,
		};

		return (
			<div className={styles.repliesWrapper}>
				<TransitionGroup enter={this.props.animation}>
					{replies.map((reply, index) =>
						<CSSTransition
							key={reply.id}
							mountOnEnter
							unmountOnExit
							in={this.props.animation}
							classNames={transitionStyles}
							timeout={500}>
							<div key={reply.id} className={styles.reply}>
								{this.renderComment(reply, index + 1 === replies.length)}
							</div>
						</CSSTransition>
					)}
				</TransitionGroup>
			</div>
		)
	}
}

Thread.propTypes = {
	user: userType.isRequired,
	userRoles: userRoles.isRequired,
	slideType: PropTypes.string,
	comment: comment.isRequired,
	reply: commentStatus,
	commentEditing: commentStatus,
	actions: PropTypes.objectOf(PropTypes.func),
	animation: PropTypes.bool,
	getUserSolutionsUrl: PropTypes.func,
};

export default Thread;
