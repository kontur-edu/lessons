import React, { Component } from "react";
import PropTypes from "prop-types";
import moment from "moment";
import 'moment/locale/ru';
import "moment-timezone";
import Checkbox from "@skbkontur/react-ui/components/Checkbox/Checkbox";
import Gapped from "@skbkontur/react-ui/components/Gapped/Gapped";
import Icon from "@skbkontur/react-icons";
import Avatar from "../../../../common/Avatar/Avatar";
import CopyStudentsModal from "../CopyStudentsModal/CopyStudentsModal";
import Profile from '../Profile';
import getGenderForm from "../../../../../utils/getGenderForm";

import styles from './groupStudents.less';

class GroupStudents extends Component {

	state = {
		studentIds: new Set(),
		modalOpen: false,
	};

	render() {
		const {students} = this.props;
		const {studentIds, modalOpen} = this.state;
		const studentsArrayOfIds = students.map(item => item.user.id);

		return (
			<React.Fragment>
				<div className={styles["actions-block"]}>
					<Checkbox
						checked={studentIds.size === studentsArrayOfIds.length || false}
						onChange={this.onCheckAllStudents}>
						Выбрать всех
					</Checkbox>
					{this.renderStudentActions()}
				</div>
				{this.renderStudents()}
				{modalOpen &&
				<CopyStudentsModal
					studentIds={studentIds}
					onClose={this.onCloseModal} />}
			</React.Fragment>
		)
	}

	renderStudentActions() {
		const {studentIds} = this.state;
		let buttonClass = `${styles.action}`;

		return (
			<div className={styles["action-buttons"]}>
				<button
					className={studentIds.size > 0 ? `${buttonClass} ${styles["button-copy"]}` : buttonClass}
					disabled={studentIds.size === 0}
					onClick={this.onOpenModal}>
					<Gapped gap={3}>
						<Icon name="Copy" />
						<span className={styles["action-text"]}>Скопировать в группу...</span>
					</Gapped>
				</button>
				<button
					className={studentIds.size > 0 ? `${buttonClass} ${styles["button-delete"]}` : buttonClass}
					disabled={studentIds.size === 0}
					onClick={this.onDeleteStudents}
				>
					<Gapped gap={3}>
						<Icon name="Trash" />
						<span className={styles["action-text"]}>Исключить из группы</span>
					</Gapped>
				</button>
			</div>
		);
	}

	renderStudents() {
		const {students, systemAccesses, isSysAdmin} = this.props;
		const {studentIds} = this.state;
		const grantTime = (grantTime) => moment(grantTime).format();

		return (
			<div>
				{students
				.sort((a, b) => a.user.visibleName.localeCompare(b.user.visibleName))
				.map(item =>
					<div className={styles["student-block"]}
						 key={item.user.id}>
						<Checkbox
							checked={studentIds.has(item.user.id) || false}
							onChange={(_, value) => this.onCheckStudent(item.user.id, _, value)}>
							<Avatar user={item.user} size='small' />
							<Profile
								user={item.user}
								systemAccesses={systemAccesses}
								isSysAdmin={isSysAdmin} /> {item.addingTime && <span className={styles.addingTime}>
									{`${getGenderForm(item.user.gender, 'вступила', 'вступил')}
									${moment(grantTime(item.addingTime)).fromNow()}`}</span>}
						</Checkbox>
					</div>
				)
				}
			</div>
		)
	}

	onOpenModal = () => {
		this.setState({
			modalOpen: true,
		})
	};

	onCloseModal = () => {
		this.setState({
			modalOpen: false,
		})
	};

	onCheckAllStudents = (_, value) => {
		const {students} = this.props;
		const studentsToArrayOfIds = students.map(item => item.user.id);

		if (value) {
			this.setState({
				studentIds: new Set(studentsToArrayOfIds),
			});
		} else {
			this.setState({
				studentIds: new Set(),
			});
		}
	};

	onCheckStudent = (id, _, value) => {
		const {studentIds} = this.state;
		const studentsCopy = new Set(studentIds);

		if (value) {
			studentsCopy.add(id);
		} else {
			studentsCopy.delete(id);
		}

		this.setState({
			studentIds: studentsCopy,
		});
	};

	onDeleteStudents = () => {
		const students = [...this.state.studentIds];
		this.props.onDeleteStudents(students);
	};
}

GroupStudents.propTypes = {
	students: PropTypes.array,
	onDeleteStudents: PropTypes.func,
	isSysAdmin: PropTypes.bool,
	systemAccesses: PropTypes.array,
};

export default GroupStudents;
