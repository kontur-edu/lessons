import React, { Component } from "react";
import PropTypes from "prop-types";
import { userRoles, user } from "../commonPropTypes";
import api from "../../../api";
import { TABS, ROLES } from "../../../consts/general";
import Tabs from "@skbkontur/react-ui/components/Tabs/Tabs";
import CommentsList from "../CommentsList/CommentsList";

import styles from "./CommentsView.less";

class CommentsView extends Component {
	constructor(props) {
		super(props);

		this.state = {
			instructorsComments: [],
			commentPolicy: {},
			activeTab: TABS.allComments,
			openModal: false,
			instructorsCommentCount: 0,
		};

		this.headerRef = React.createRef();
	}

	static defaultProps = {
		commentsApi: api.comments,
	};

	componentDidMount() {
		const {courseId, slideId, userRoles} = this.props;

		this.loadCommentPolicy(courseId);

		if (this.isInstructor(userRoles))
			this.loadComments(courseId, slideId);
	};

	loadCommentPolicy = (courseId) => {
		this.props.commentsApi.getCommentPolicy(courseId)
			.then(commentPolicy => {
				this.setState ({
					commentPolicy: commentPolicy,
				})
			})
			.catch(console.error);
	};

	loadComments = (courseId, slideId) => {
		this.props.commentsApi.getComments(courseId, slideId, true)
		.then(json => {
			let comments = json.topLevelComments;
			this.setState({
				instructorsComments: comments,
				instructorsCommentCount: comments.length,
			});
		})
		.catch(console.error);
	};

	render() {
		const {user, userRoles, courseId, slideId, slideType, commentsApi} = this.props;

		return (
			<div className={styles.wrapper}>
				{this.renderHeader()}
				<div key={this.state.activeTab}>
					<CommentsList
						slideType={slideType}
						handleInstructorsCommentCount={this.handleInstructorsCommentCount}
						handleTabChange={this.handleTabChange}
						headerRef={this.headerRef}
						forInstructors={this.state.activeTab === TABS.instructorsComments}
						commentsApi={commentsApi}
						commentPolicy={this.state.commentPolicy}
						user={user}
						userRoles={userRoles}
						slideId={slideId}
						courseId={courseId}>
					</CommentsList>
				</div>
			</div>
		)
	};

	renderHeader() {
		const {userRoles} = this.props;
		const {activeTab, instructorsCommentCount} = this.state;

		return (
			<header className={styles.header} ref={this.headerRef}>
				<div className={styles.headerRow}>
					<h1 className={styles.headerName}>Комментарии</h1>
				</div>
				{this.isInstructor(userRoles) &&
				<div className={styles.tabs}>
					<Tabs value={activeTab} onChange={(e, id)=> this.handleTabChange(id, true)}>
						<Tabs.Tab id={TABS.allComments}>К слайду</Tabs.Tab>
						<Tabs.Tab id={TABS.instructorsComments}>
							Для преподавателей
							{instructorsCommentCount > 0 &&
							<span className={styles.commentsCount}>{instructorsCommentCount}</span>}
						</Tabs.Tab>
						{activeTab === TABS.instructorsComments &&
						<span className={`${styles.textForInstructors} ${styles["visible-at-least-tablet"]}`}>
							Студенты не видят эти комментарии
						</span>}
					</Tabs>
				</div>}
			</header>
		)
	};

	isCourseAdmin(userRoles) {
		return userRoles.isSystemAdministrator ||
			userRoles.courseRole === ROLES.courseAdmin;
	}

	isInstructor(userRoles) {
		return this.isCourseAdmin(userRoles) ||
			userRoles.courseRole === ROLES.instructor;
	}

	handleTabChange = (id, isUserAction) => {
		if (id !== this.state.activeTab) {
			this.setState({
				activeTab: id,
			});
		}

		if (isUserAction) {
			window.history.pushState("", document.title, window.location.pathname + window.location.search);
		}
	};

	handleInstructorsCommentCount = (action) => {
		if (action === "add") {
			this.setState({
				instructorsCommentCount: this.state.instructorsCommentCount + 1,
			})
		} else {
			this.setState({
				instructorsCommentCount: this.state.instructorsCommentCount - 1,
			})
		}
	}
}

CommentsView.propTypes = {
	user: user.isRequired,
	userRoles: userRoles.isRequired,
	courseId: PropTypes.string.isRequired,
	slideId: PropTypes.string.isRequired,
	slideType: PropTypes.string.isRequired,
};

export default CommentsView;