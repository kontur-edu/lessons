import React, { Component, createRef } from "react";
import { withRouter } from "react-router-dom";
import { Helmet } from "react-helmet";
import connect from "react-redux/es/connect/connect";
import PropTypes from 'prop-types';
import api from "../../../api";
import { Page } from "../../index";
import GroupList from "../../../components/groups/GroupMainPage/GroupList/GroupList";
import GroupHeader from "../../../components/groups/GroupMainPage/GroupHeader/GroupHeader";
import Error404 from "../../../components/common/Error/Error404";
import Toast from "@skbkontur/react-ui/components/Toast/Toast";

class GroupListPage extends Component {
	constructor(props) {
		super(props);
		this.headerRef = createRef();
		this.state = {
			groups: [],
			archiveGroups: [],
			filter: "active",
			loadingArchived: false,
			loadingActive: false,
			loadedArchived: false,
			loadedActive: false,
			status: '',
		}
	};

	get courseId() {
		return this.props.match.params.courseId.toLowerCase();
	}

	componentDidMount() {
		this.loadActiveGroups(this.courseId);
		this.props.enterToCourse(this.courseId);
	};

	loadActiveGroups = (courseId) => {
		const {loadingActive, loadedActive} = this.state;

		if (loadedActive || loadingActive) {
			return;
		}

		this.setState({
			loadingActive: true,
		});

		api.groups.getCourseGroups(courseId)
		.then(json => {
			let groups = json.groups;
			this.setState({
				loadedActive: true,
				groups,
			});
		})
		.catch(() => {
			this.setState({
				status: 'error',
			});
		})
		.finally(() =>
			this.setState({
				loadingActive: false,
			})
		)
	};

	loadArchivedGroups = (courseId) => {
		const {loadingArchived, loadedArchived} = this.state;

		if (loadedArchived || loadingArchived) {
			return;
		}

		this.setState({
			loadingArchived: true,
		});

		api.groups.getCourseArchivedGroups(courseId)
		.then(json => {
			let archiveGroups = json.groups;
			this.setState({
				loadedArchived: true,
				archiveGroups,
			});
		})
		.catch(console.error)
		.finally(() =>
			this.setState({
				loadingArchived: false,
			})
		)
	};

	render() {
		const courseById = this.props.courses.courseById;
		const course = courseById[this.courseId];

		if (this.state.status === "error") {
			return <Error404 />;
		}

		if (!course) {
			return null;
		}

		return (
			<Page>
				<Helmet defer={false}>
					<title>{`Группы в курсе ${course.title.toLowerCase()}`}</title>
				</Helmet>
				<GroupHeader
					onTabChange={this.onTabChange}
					filter={this.state.filter}
					course={course}
					addGroup={this.addGroup}
					groups={this.state.groups}
					ref={this.headerRef}
				/>
				<GroupList
					courseId={this.courseId}
					groups={this.filteredGroups}
					deleteGroup={this.deleteGroup}
					toggleArchived={this.toggleArchived}
					loading={this.loading}
				>
					{this.state.filter === "archived" && <div>
						У вас нет архивных групп. Когда какая-нибудь группа станет вам больше не&nbsp;нужна,
						заархивируйте её.
						Архивные группы будут жить здесь вечно и не&nbsp;помешают вам в&nbsp;текущей работе. Однако если
						понадобится, вы всегда
						сможете вернуться к&nbsp;ним.
					</div>}
					{this.state.filter === "active" && <div>
						У вас нет активных групп. Создайте группу и пригласите в неё студентов, чтобы видеть их
						прогресс, проверять их тесты и делать код-ревью их решений.
					</div>}
				</GroupList>
			</Page>
		)
	};

	onTabChange = (id) => {
		this.setState({
			filter: id,
		});

		if (id === "active") {
			this.loadActiveGroups(this.courseId);
		} else {
			this.loadArchivedGroups(this.courseId);
		}
	};

	addGroup = async (groupId) => {
		const groups = this.filteredGroups;
		const newGroup = await api.groups.getGroup(groupId);

		this.setState({
			groups: [newGroup, ...groups],
		});

		this.props.history.push(`/${this.courseId}/groups/${groupId}`);
	};

	get filteredGroups() {
		if (this.state.filter === "archived") {
			return this.state.archiveGroups;
		} else {
			return this.state.groups;
		}
	};

	deleteGroup = (group, groupsName) => {
		api.groups.deleteGroup(group.id)
		.then(() => {
			Toast.push(`Группа «${group.name}» удалена`);

			const updateGroups = this.state[groupsName].filter(g => group.id !== g.id);

			this.setState({
				[groupsName]: updateGroups,
			});
		})
		.catch((error) => {
			error.showToast();
		});
	};

	toggleArchived = (group, isArchived) => {
		const newSettings = {
			isArchived
		};

		api.groups.saveGroupSettings(group.id, newSettings)
		.then(() => {
			Toast.push(isArchived ? `Группа «${group.name}» заархивирована` : `Группа «${group.name}» восстановлена`);

			group = {...group, ...newSettings};

			if (isArchived) {
				this.moveGroup(group, 'groups', 'archiveGroups');
			} else {
				this.moveGroup(group, 'archiveGroups', 'groups');
			}
		})
		.catch((error) => {
			error.showToast();
		});
	};

	moveGroup = (group, moveFrom, moveTo) => {
		const groupsMoveFrom = this.state[moveFrom].filter(g => group.id !== g.id);
		const groupsMoveTo = [group, ...this.state[moveTo]].sort((a, b) => a.name.localeCompare(b.name));

		this.setState({
			[moveFrom]: groupsMoveFrom,
			[moveTo]: groupsMoveTo
		});
	};

	get loading() {
		if (this.state.filter === "archived") {
			return this.state.loadingArchived;
		} else {
			return this.state.loadingActive;
		}
	};

	static mapStateToProps(state) {
		return {
			courses: state.courses,
		}
	}

	static mapDispatchToProps(dispatch) {
		return {
			enterToCourse: (courseId) => dispatch({
				type: 'COURSES__COURSE_ENTERED',
				courseId: courseId
			}),
		}
	}
}

GroupListPage.propTypes = {
	history: PropTypes.object,
	match: PropTypes.object,
	courses: PropTypes.object,
	enterToCourse: PropTypes.func,
};

GroupListPage = connect(GroupListPage.mapStateToProps, GroupListPage.mapDispatchToProps)(GroupListPage);

export default withRouter(GroupListPage);