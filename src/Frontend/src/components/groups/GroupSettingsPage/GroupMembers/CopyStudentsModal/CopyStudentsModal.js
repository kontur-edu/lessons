import React, { Component } from "react";
import PropTypes from "prop-types";
import api from "../../../../../api/index";
import Select from "@skbkontur/react-ui/components/Select/Select";
import Modal from "@skbkontur/react-ui/components/Modal/Modal";
import Button from "@skbkontur/react-ui/components/Button/Button";
import Toast from "@skbkontur/react-ui/components/Toast/Toast";
import getPluralForm from "../../../../../utils/getPluralForm";

import styles from './copyStudentsModal.less';
import Loader from "@skbkontur/react-ui/components/Loader/Loader";

class CopyStudentsModal extends Component {

	state = {
		groupId: null,
		courseId: null,
		groups: [],
		courses: [],
		error: null,
		loadingGroups: false,
		loading: false,
	};

	componentDidMount() {
		api.courses.getUserCourses()
		.then(json => {
			let courses = json.courses;
			this.setState({
				courses,
			});
		}).catch(console.error);
	}

	loadGroups = (courseId) => {
		this.setState({loadingGroups: true});
		api.groups.getCourseGroups(courseId)
		.then(json => {
			let groups = json.groups;
			this.setState({
				groups,
				loadingGroups: false,
			});
		})
		.catch(console.error)
		.finally(() =>
			this.setState({
				loadingGroups: false,
			})
		)
	};

	render() {
		const {onClose} = this.props;
		return (
			<Modal onClose={onClose} width="100%">
				<Modal.Header>Скопировать студентов</Modal.Header>
				<form onSubmit={this.onSubmit}>
					<Modal.Body>
						<div className={styles["modal-content"]}>
							{this.renderCourseSelect()}
							{this.renderGroupSelect()}
						</div>
					</Modal.Body>
					<Modal.Footer>
						<Button
							use="primary"
							size="medium"
							type="submit"
							disabled={!this.state.groupId}
							loading={this.state.loading}>
							Cкопировать
						</Button>
					</Modal.Footer>
				</form>
			</Modal>
		)
	}

	renderCourseSelect() {
		const {courseId} = this.state;
		return (
			<React.Fragment>
				<p className={styles["course-info"]}>
					Выберите курс, в который надо скопировать студентов
				</p>
				<label className={styles["select-course"]}>
					<Select
						autofocus
						required
						items={this.getCourseOptions()}
						onChange={this.onCourseChange}
						width="200"
						placeholder="Курс"
						value={courseId}
						error={this.hasError()}
					/>
				</label>
			</React.Fragment>
		)
	}

	renderGroupSelect() {
		const {groupId, groups} = this.state;
		return (
			<React.Fragment>
				<p className={styles["group-info"]}>
					Выберите группу
				</p>
				<Loader type="normal" active={this.state.loadingGroups}>
					<label className={styles["select-group"]}>
						<Select
							autofocus
							required
							items={this.getGroupOptions()}
							onChange={this.onGroupChange}
							width="200"
							placeholder="Группа"
							value={groupId}
							error={this.hasError()}
							disabled={groups.length === 0} />
					</label>
					{this.state.loadingGroups ? null : (this.checkGroups() && this.renderEmptyGroups())}
				</Loader>
			</React.Fragment>
		)
	}

	renderEmptyGroups() {
		const {courses, courseId} = this.state;
		return (
			<p className={styles["empty-group-info"]}>
				<b>В курсе {this.getTitle(courses, courseId)} нет доступных вам групп</b>
			</p>
		)
	}

	getCourseOptions = () => {
		const {courses} = this.state;
		return courses.map(course => [course.id, course.title]);
	};

	getTitle = (arr, id) => {
		const item = arr.find(item => item.id === id);
		return item.title || item.name;
	};

	onCourseChange = (_, value) => {
		this.setState({
			courseId: value,
			groupId: null
		});

		this.loadGroups(value);
	};

	getGroupOptions = () => {
		const {groups} = this.state;

		return groups.map(group => [group.id, `${group.name}: ${group.studentsCount} 
		${getPluralForm(group.studentsCount, 'студент', 'студента', 'студентов')}`]);
	};

	onGroupChange = (_, value) => {
		this.setState({groupId: value});
	};

	hasError = () => {
		return this.state.error !== null;
	};

	checkGroups = () => {
		const {courseId, groups} = this.state;
		if (!groups) {
			return false;
		}
		return (courseId && groups.length === 0);
	};

	onSubmit = (e) => {
		const {groupId, courseId, groups} = this.state;
		const {studentIds, onClose} = this.props;

		e.preventDefault();

		if (!courseId || !groupId) {
			this.setState({
				error: 'Выберите курс',
			});
			return;
		}

		const students = [...studentIds];

		this.setState({loading: true});
		api.groups.copyStudents(groupId, students)
		.then(() => {
			Toast.push(`Студенты скопированы в группу ${this.getTitle(groups, groupId)}`);
		})
		.catch((error) => {
			error.showToast();
		})
		.finally(() => this.setState({loading: false}));

		onClose();
	};
}

CopyStudentsModal.propTypes = {
	onCloseModal: PropTypes.func,
	studentIds: PropTypes.object,
};

export default CopyStudentsModal;