import React, {Component} from "react";
import PropTypes from "prop-types";
import {withRouter} from "react-router-dom";
import moment from "moment";
import api from "../../../../api";
import Icon from "@skbkontur/react-icons";
import Kebab from "@skbkontur/react-ui/components/Kebab/Kebab";
import MenuItem from "@skbkontur/react-ui/components/MenuItem/MenuItem";
import Gapped from "@skbkontur/react-ui/components/Gapped/Gapped";
import Loader from "@skbkontur/react-ui/components/Loader/Loader";
import ComboboxInstructorsSearch from "./Combobox/ComboboxInstructorsSearch";
import Avatar from "../../../common/Avatar/Avatar";
import GroupStudents from "./GroupStudents/GroupStudents";
import InviteBlock from "./InviteBlock/InviteBlock";
import Profile from './Profile';
import getGenderForm from "../../../../utils/getGenderForm";
import styles from './groupMembers.less';
import Toast from "@skbkontur/react-ui/components/Toast/Toast";
import {Mobile, NotMobile} from "../../../../utils/responsive";

class GroupMembers extends Component {

	state = {
		accesses: [],
		selected: null,
		students: [],
		loadingTeachers: false,
		loadingStudents: false,
	};

	componentDidMount() {
		const { groupId } = this.props.match.params;

		this.loadGroupAccesses(groupId);
		this.loadStudents(groupId);
	}

	loadGroupAccesses = (groupId) => {
		this.setState({
			loadingTeachers: true,
		});

		api.groups.getGroupAccesses(groupId)
			.then(json => {
			let accesses = json.accesses;
			this.setState({
				accesses,
				loadingTeachers: false,
			});
			})
			.catch(console.error)
			.finally(() =>
				this.setState({
					loadingTeachers: false,
				})
			)
	};

	loadStudents = (groupId) => {
		this.setState({
			loadingStudents: true,
		});

		api.groups.getStudents(groupId)
			.then(json => {
				let students = json.students;
				this.setState({
					students,
					loadingStudents: false,
				});
			})
			.catch(console.error)
			.finally(() =>
				this.setState({
					loadingStudents: false,
				})
			)
	};

	render() {
		const { accesses, students, loadingStudents, loadingTeachers } = this.state;
		const { group } = this.props;
		const owner = group.owner;

		if (!owner) {
			return null;
		}

		const { systemAccesses, isSysAdmin } = this.props;

		return (
			<div className={styles.wrapper}>
				<div className={styles.teachers}>
					<h4 className={styles["teachers-header"]}>Преподаватели</h4>
					<p className={styles["teachers-info"]}>
						Преподаватели могут видеть список участников группы, проводить код-ревью
						и проверку тестов, выставлять баллы и смотреть ведомость.
					</p>
					<Loader type="big" active={loadingTeachers}>
						<div className={styles["teacher-block"]}>
							<Avatar user={owner} size='big' />
							<div className={styles["teacher-name"]}>
								<Profile
									user={owner}
									systemAccesses={systemAccesses}
									isSysAdmin={isSysAdmin} /> <span className={styles["teacher-status"]}>Владелец</span>
							</div>
						</div>
						{ (accesses.length > 0) && this.renderTeachers() }
					</Loader>
					{ this.renderTeachersSearch() }
				</div>
				<div className={styles["students-block"]}>
					<h4 className={styles["students-header"]}>Студенты</h4>
					<InviteBlock group={group} />
					<Loader type="big" active={loadingStudents}>
						<div className={styles["students-list"]}>
							{(students.length >0) &&
							<GroupStudents
								isSysAdmin={isSysAdmin}
								systemAccesses={systemAccesses}
								students={students}
								group={group}
								onDeleteStudents={this.onDeleteStudents}/>}
						</div>
					</Loader>
				</div>
			</div>
		)
	}

	renderTeachers() {
		const { accesses } = this.state;
		const { group, role, userId } = this.props;
		const owner = group.owner;

		const grantTime = (grantTime) => moment(grantTime).format();

		const { systemAccesses, isSysAdmin } = this.props;

		return (accesses
			.sort((a, b) => a.user.visibleName.localeCompare(b.user.visibleName))
			.map(item =>
				<React.Fragment
					key={item.user.id}>
					<div className={styles["teacher-block"]}>
						<Avatar user={item.user} size='big' />
						<div className={styles["teacher-name"]}>
							<Profile
								user={item.user}
								systemAccesses={systemAccesses}
								isSysAdmin={isSysAdmin} /> <span className={styles["teacher-status"]}>
								Полный доступ { `${getGenderForm(owner.gender, 'предоставила', 'предоставил') }
								${item.grantedBy.visibleName} ${moment(grantTime(item.grantTime)).fromNow()}` }
							</span>
						</div>
						<div className={styles["teacher-action"]}>
							{((group.owner.id === userId) || (role === 'courseAdmin')) && this.renderKebab(item)}
						</div>
					</div>
				</React.Fragment>
				)
		)
	};

	renderKebab(item) {
		const { group, role, userId } = this.props;

		let menuItems = [
			<MenuItem onClick={() => this.onChangeOwner(item.user)} key="changeOwner">
				<Gapped gap={5}>
					<Icon name="User" />
					Сделать владельцем
				</Gapped>
			</MenuItem>
		];
		if (group.owner.id === userId || role === 'courseAdmin' || item.grantedBy.id === userId){
			menuItems.push(
				<MenuItem onClick={() => this.onRemoveTeacher(item.user)} key="removeTeacher">
					<Gapped gap={5}>
						<Icon name="Delete"/>
						Забрать доступ
					</Gapped>
				</MenuItem>
			);
		}

		/* TODO (andgein): Change to size="medium" inside of <Mobile> after updating to the new react-ui version */
		return (
			<React.Fragment>
				<Mobile>
					<Kebab size="large" positions={["left top"]} disableAnimations={true}>
						{ menuItems }
					</Kebab>
				</Mobile>
				<NotMobile>
					<Kebab size="large" positions={["bottom right"]} disableAnimations={false}>
						{ menuItems }
					</Kebab>
				</NotMobile>
			</React.Fragment>
		)
	}

	renderTeachersSearch() {
		const { group, courseId } = this.props;
		const { accesses, selected } = this.state;

		return (
			<label className={styles["teacher-search"]}>
				<p>Добавить преподавателя:</p>
				<ComboboxInstructorsSearch
					selected={selected}
					courseId={courseId}
					accesses={accesses}
					owner={group.owner}
					onAddTeacher={this.onAddTeacher}/>
			</label>
		)
	}

	onChangeOwner = (user) => {
		const { accesses } = this.state;
		const { group } = this.props;

		api.groups.changeGroupOwner(group.id, user.id)
			.then(() => {
				const updatedAccesses = accesses.map(item =>
					item.user.id === user.id ? {...item, user: group.owner, grantTime: new Date()} : item);
				this.setState({
					accesses: updatedAccesses,
				});

				this.props.onChangeGroupOwner(user, updatedAccesses);
			})
			.catch((error) => {
				error.showToast();
			});
	};

	onRemoveTeacher = (user) => {
		const { accesses } = this.state;

		api.groups.removeAccess(this.props.group.id, user.id)
			.then(() => {
				const updatedAccesses = accesses
				.filter(item => item.user.id !== user.id);
				this.setState({
					accesses: updatedAccesses,
				});
			})
			.catch((error) => {
				error.showToast();
			});
	};

	onAddTeacher = (item) => {
		this.setState({
			selected: item,
			});

		this.onLoadTeacher(item);
	};

	onLoadTeacher = (item) => {
		const { accesses } = this.state;
		const { group } = this.props;

		api.groups.addGroupAccesses(group.id, item.value)
			.then(() => {
				const updatedAccesses = accesses
					.filter(i => i.user.id !== item.value)
					.concat({
						user: item,
						grantedBy: group.owner,
						grantTime: new Date(),
					});

				this.setState({
					accesses: updatedAccesses,
					selected: null,
				});
			})
			.catch((error) => {
				error.showToast();
			});
	};

	onDeleteStudents = (students) => {
		const { group } = this.props;

		api.groups.deleteStudents(group.id, students)
			.then(() => {
				const updatedStudents = this.state.students.filter((item) => !students.includes(item.user.id));

				this.setState({
					students: updatedStudents,
				});

				Toast.push("Студенты исключены из группы");
			})
			.catch((error) => {
				error.showToast();
			});
	};
}

GroupMembers.propTypes = {
	courseId: PropTypes.string,
	group: PropTypes.object,
	match: PropTypes.object,
	onChangeGroupOwner: PropTypes.func,
	role: PropTypes.string,
	userId: PropTypes.string,
	isSysAdmin: PropTypes.bool,
	systemAccesses: PropTypes.array,
};

export default withRouter(GroupMembers);
