import React from "react";
import PropTypes from "prop-types";
import { comment, userRoles, userType } from "../../commonPropTypes";
import Link from "@skbkontur/react-ui/components/Link/Link";
import Button from "@skbkontur/react-ui/components/Button/Button";
import Icon from "@skbkontur/react-icons";
import { ACCESSES, SLIDETYPE } from "../../../../consts/general";

import styles from "./CommentActions.less";

const ActionButton = ({onClick, icon, children}) => (
	<div className={styles.action}>
		<Button use="link" onClick={onClick} icon={<Icon name={icon} />}>
			{children}
		</Button>
	</div>
);

const ActionLink = ({url, icon, children}) => (
	<div className={styles.action}>
		<Link href={url} icon={<Icon name={icon} />}>
			{children}
		</Link>
	</div>
);

export default function CommentActions(props) {
	const {user, comment, userRoles, url, hasReplyAction, canModerateComments,
		actions, slideType, canReply} = props;

	const commentActions = [];

	if (canReply && hasReplyAction) {
		const commentId = comment.parentCommentId ? comment.parentCommentId : comment.id;

		commentActions.push(
			<ActionButton
				key="Ответить"
				onClick={() => actions.handleShowReplyForm(commentId)}
				icon="ArrowCorner1">
				Ответить
			</ActionButton>);
	}

	if (user.id === comment.author.id || canModerateComments(userRoles, ACCESSES.editPinAndRemoveComments)) {
		commentActions.push(
		<div className={styles.visibleOnDesktopAndTablet} key="Редактировать">
			<ActionButton
				onClick={() => actions.handleShowEditForm(comment.id)}
				icon="Edit">
				Редактировать
			</ActionButton>
		</div>);
	}

	if (slideType === SLIDETYPE.exercise && canModerateComments(userRoles, ACCESSES.viewAllStudentsSubmissions)) {
		commentActions.push(
			<div className={styles.visibleOnDesktopAndTablet}  key="Решения">
				<ActionLink
					url={url}
					icon="DocumentLite">
					Посмотреть решения
				</ActionLink>
			</div>);
	}

	return (
		commentActions.length > 0 &&
		<div className={styles.actions}>
			{commentActions}
		</div>
	)
};

CommentActions.propTypes = {
	user: userType.isRequired,
	userRoles: userRoles.isRequired,
	comment: comment.isRequired,
	actions: PropTypes.objectOf(PropTypes.func),
	url: PropTypes.string,
	hasReplyAction: PropTypes.bool,
	canModerateComments: PropTypes.func,
	slideType: PropTypes.string,
	getUserSolutionsUrl: PropTypes.func,
};

