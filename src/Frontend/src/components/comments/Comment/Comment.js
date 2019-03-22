import React, { Component } from 'react';
import PropTypes from "prop-types";
import { userRoles, userType, comment, commentStatus } from "../commonPropTypes";
import moment from "moment";
import Hint from "@skbkontur/react-ui/components/Hint/Hint";
import Link from "@skbkontur/react-ui/components/Link/Link";
import Avatar from "../../common/Avatar/Avatar";
import CommentSendForm from "../CommentSendForm/CommentSendForm";
import Like from "./Like/Like";
import KebabActions from "./Kebab/KebabActions";
import Header from "./Header/Header";
import Marks from "./Marks/Marks";
import CommentActions from "./CommentActions/CommentActions";

import styles from "./Comment.less";

class Comment extends Component {
	ref = React.createRef();

	// componentDidMount() {
	// 	if (this.props.scrollIntoView) {
	// 		this.scrollIntoView();
	// 	}
	// }
	//
	// componentDidUpdate() {
	// 	if (this.props.scrollIntoView) {
	// 		this.scrollIntoView();
	// 	}
	// }

	render() {
		const {actions, children, commentEditing, comment, userRoles,user, slideType, getUserSolutionsUrl} = this.props;
		const canViewProfiles = (user.systemAccesses && user.systemAccesses.includes("viewAllProfiles")) ||
			userRoles.isSystemAdministrator;
		const canViewStudentsGroup = (user.systemAccesses && user.systemAccesses.includes("ViewAllGroupMembers")) ||
			(userRoles.courseAccesses && userRoles.courseAccesses.includes("ViewAllGroupMembers")) ||
			userRoles.isSystemAdministrator;
		const profileUrl = `${window.location.origin}/Account/Profile?userId=${comment.author.id}`;

		return (
			<div className={styles.comment} ref={this.ref}>
				{canViewProfiles ? <Link href={profileUrl}><Avatar user={comment.author} size='big' /></Link> :
					<Avatar user={comment.author} size='big' />}
				<div className={styles.content}>
					<Header
						profileUrl={profileUrl}
						canViewProfiles={canViewProfiles}
						name={comment.author.visibleName}>
						<Like
							checked={comment.isLiked}
							count={comment.likesCount}
							onClick={() => actions.handleLikeClick(comment.id, comment.isLiked)} />
						<Marks
							canViewStudentsGroup={canViewStudentsGroup}
							isApproved={comment.isApproved}
							isCorrectAnswer={comment.isCorrectAnswer}
							isPinnedToTop={comment.isPinnedToTop} />
						<KebabActions
							user={user}
							url={getUserSolutionsUrl(comment.author.id)}
							canModerateComments={this.canModerateComments}
							userRoles={userRoles}
							slideType={slideType}
							comment={comment}
							actions={actions} />
					</Header>
					<Hint pos="bottom" text={comment.publishTime} disableAnimations={false} useWrapper={false}>
						<div className={styles.timeSinceAdded}>
							{moment(comment.publishTime).fromNow()}
						</div>
					</Hint>
					{comment.id === commentEditing.commentId ? this.renderEditCommentForm() : this.renderComment()}
					{children}
				</div>
			</div>
		);
	}

	renderComment() {
		const {comment, user, userRoles, hasReplyAction, actions, slideType} = this.props;
		return (
			<React.Fragment>
				<p className={styles.text}>
					<span dangerouslySetInnerHTML={{__html: comment.renderedText}} />
				</p>
				<CommentActions
					slideType={slideType}
					comment={comment}
					user={user}
					userRoles={userRoles}
					hasReplyAction={hasReplyAction}
					actions={actions}
					canModerateComments={this.canModerateComments} />
			</React.Fragment>
		)
	}

	renderEditCommentForm() {
		const {comment, actions, commentEditing} = this.props;

		return (
			<CommentSendForm
				commentId={comment.id}
				handleSubmit={actions.handleEditComment}
				text={comment.text}
				submitTitle={'Сохранить'}
				sending={commentEditing.sending}
				onCancel={() => actions.handleShowEditForm(null)}
			/>
		)
	}

	canModerateComments = (role, accesses) => {
		return role.isSystemAdministrator || role.courseRole === 'courseAdmin' ||
			(role.courseRole === 'instructor' && role.courseAccesses.includes(accesses))
	};

	scrollIntoView() {
		const headerHeight = 60;
		window.scrollTo({
			left: 0,
			top: this.ref.current.offsetTop - headerHeight,
			behavior: "smooth"
		});
	}
}

Comment.propTypes = {
	user: userType.isRequired,
	userRoles: userRoles.isRequired,
	comment: comment.isRequired,
	actions: PropTypes.objectOf(PropTypes.func),
	children: PropTypes.array,
	getUserSolutionsUrl: PropTypes.func,
	commentEditing: commentStatus,
	hasReplyAction: PropTypes.bool,
	slideType: PropTypes.string,
};

export default Comment;