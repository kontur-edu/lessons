import React, { Component } from 'react';
import PropTypes from "prop-types";
import connect from "react-redux/es/connect/connect";
import { Redirect } from 'react-router-dom';
import { withRouter } from "react-router-dom";
import { Helmet } from "react-helmet";
import api from "src/api";
import { Tabs, Button, Toast, Link } from "ui";
import GroupMembers from "src/components/groups/GroupSettingsPage/GroupMembers/GroupMembers";
import GroupSettings from "src/components/groups/GroupSettingsPage/GroupSettings/GroupSettings";
import Error404 from "src/components/common/Error/Error404.tsx";
import styles from "./groupPage.less";
import Page from "../../index";
import { changeCurrentCourseAction } from "src/actions/course";

class GroupPage extends Component {

	state = {
		group: {},
		updatedFields: {},
		error: false,
		loadingAllSettings: false,
		loadingGroup: true,
		loadingScores: true,
		loading: false,
		scores: [],
		checkedScoresSettingsIds: [],
		status: '',
	};

	componentDidMount() {
		let groupId = this.props.match.params.groupId;
		let courseId = this.props.match.params.courseId.toLowerCase();

		this.props.enterToCourse(courseId);

		this.loadGroupScores(groupId);
		this.loadGroup(groupId);
	};

	loadGroup = (groupId) => {
		api.groups.getGroup(groupId)
		.then(group => {
			this.setState({
				group,
			});
		})
		.catch(() => {
			this.setState({
				status: 'error',
			});
		})
		.finally(() => {
				this.setState({
					loadingGroup: false,
				})
			}
		);
	};

	loadGroupScores = (groupId) => {
		api.groups.getGroupScores(groupId)
		.then(json => {
			const scores = json.scores;
			const checkedScoresSettingsIds = scores
				.filter(score=> score.areAdditionalScoresEnabledInThisGroup || score.areAdditionalScoresEnabledForAllGroups || false)
				.map(score=> score.id);
			this.setState({
				scores,
				checkedScoresSettingsIds,
			});
		})
		.catch(console.error)
		.finally(() =>
			this.setState({
				loadingScores: false,
			})
		);
	};

	render() {
		const {group} = this.state;
		let courseId = this.props.match.params.courseId.toLowerCase();
		const {groupId, groupPage} = this.props.match.params;

		if (this.state.status === "error") {
			return <Error404 />;
		}

		if (!groupPage) {
			return <Redirect to={`/${courseId}/groups/${groupId}/settings`} />
		}

		let rolesByCourse = this.props.account.roleByCourse;
		let systemAccesses = this.props.account.systemAccesses;
		let courseRole = '';

		if (this.props.account.isSystemAdministrator) {
			courseRole = 'courseAdmin';
		} else {
			courseRole = rolesByCourse[courseId];
		}

		return (
			<Page>
				<Helmet defer={true}>
					<title>{`Группа ${group.name}`}</title>
				</Helmet>
				{this.renderHeader()}
				<div className={styles.content}>
					{groupPage === "settings" &&
					this.renderSettings()}
					{groupPage === "members" &&
					<GroupMembers
						courseId={courseId}
						account={this.props.account}
						role={courseRole}
						isSysAdmin={this.props.account.isSystemAdministrator}
						systemAccesses={systemAccesses}
						group={group}
						onChangeGroupOwner={this.onChangeGroupOwner} />
					}
				</div>
			</Page>
		)
	}

	renderHeader() {
		const {group} = this.state;
		const {groupId, groupPage} = this.props.match.params;
		let courseId = this.props.match.params.courseId.toLowerCase();

		if (!['settings', 'members'].includes(groupPage)) {
			return <Redirect to={`/${courseId}/groups/${groupId}/settings`} />
		}

		return (
			<header className={styles["group-header"]}>
				<div className={styles["link-to-prev-page-block"]}>
					<div className={styles["link-to-prev-page"]}>
						<Link onClick={this.goToPrevPage}>
							← Все группы
						</Link>
					</div>
				</div>
				<h2 className={styles["group-name"]}>{group.name ? group.name : " "}</h2>
				<div className={styles["tabs-container"]}>
					<Tabs value={groupPage} onValueChange={this.onChangeTab}>
						<Tabs.Tab id="settings">Настройки</Tabs.Tab>
						<Tabs.Tab id="members">Преподаватели и студенты</Tabs.Tab>
					</Tabs>
				</div>
			</header>
		)
	}

	renderSettings() {
		const {group, loadingAllSettings, scores, updatedFields, error} = this.state;
		return (
			<form onSubmit={this.sendSettings}>
				<GroupSettings
					loading={this.state.loadingScores && this.state.loadingGroup}
					name={updatedFields.name !== undefined ? updatedFields.name : group.name}
					group={group}
					scores={scores}
					error={error}
					onChangeName={this.onChangeName}
					onChangeSettings={this.onChangeSettings}
					onChangeScores={this.onChangeScores} />
				<Button
					size="medium"
					use="primary"
					type="submit"
					loading={loadingAllSettings}>
					Сохранить
				</Button>
			</form>
		)
	}

	onChangeTab = (value) => {
		const {courseId, groupId} = this.props.match.params;

		this.props.history.push(`/${courseId}/groups/${groupId}/${value}`);
	};

	onChangeName = (value) => {
		const {updatedFields} = this.state.updatedFields;

		this.setState({
			updatedFields: {
				...updatedFields,
				name: value,
			}
		});
	};

	onChangeSettings = (field, value) => {
		const {group, updatedFields} = this.state;

		this.setState({
			group: {
				...group,
				[field]: value
			},
			updatedFields: {
				...updatedFields,
				[field]: value,
			}
		});
	};

	onChangeGroupOwner = (user, updatedGroupAccesses) => {
		const {group} = this.state;
		const updatedGroup = {...group, owner: user, accesses: updatedGroupAccesses};
		this.setState({
			group: updatedGroup,
		});
	};

	onChangeScores = (key, field, value) => {
		const {scores} = this.state;
		const updatedScores = scores
		.map(item => item.id === key ? {...item, [field]: value} : item);

		const checkedScoresSettingsIds = updatedScores
		.filter(item => item[field] === true)
		.map(item => item.id);

		this.setState({
			scores: updatedScores,
			checkedScoresSettingsIds,
		});
	};

	goToPrevPage = () => {
		let courseId = this.props.match.params.courseId.toLowerCase();

		this.props.history.push(`/${courseId}/groups/`);
	};

	sendSettings = (e) => {
		const {group, updatedFields, checkedScoresSettingsIds} = this.state;
		e.preventDefault();

		const saveGroup = api.groups.saveGroupSettings(group.id, updatedFields);
		const saveScores = api.groups.saveScoresSettings(group.id, checkedScoresSettingsIds);

		Promise
		.all([saveGroup, saveScores])
		.then(([group, scores]) => {
			this.setState({
				loadingAllSettings: true,
				group: {
					...group,
					name: updatedFields.name === undefined ? group.name : updatedFields.name,
				}
			});
			Toast.push('Настройки группы сохранены');
		})
		.catch((error) => {
			error.showToast();
		})
		.finally(() => {
			this.setState({loadingAllSettings: false});
		});
	};

	static mapStateToProps(state) {
		return {
			courses: state.courses,
			account: state.account,
		}
	}

	static mapDispatchToProps(dispatch) {
		return {
			enterToCourse: (courseId) => dispatch(changeCurrentCourseAction(courseId)),
		}
	}
}

GroupPage.propTypes = {
	history: PropTypes.object,
	account: PropTypes.object,
	courses: PropTypes.object,
	match: PropTypes.object,
	enterToCourse: PropTypes.func,
};

GroupPage = connect(GroupPage.mapStateToProps, GroupPage.mapDispatchToProps)(GroupPage);

export default withRouter(GroupPage);
