import React, { Component } from 'react';
import PropTypes from "prop-types";
import api from "../../../../api";
import Modal from '@skbkontur/react-ui/components/Modal/Modal';
import Select from '@skbkontur/react-ui/components/Select/Select';
import Button from '@skbkontur/react-ui/components/Button/Button';
import Checkbox from '@skbkontur/react-ui/components/Checkbox/Checkbox';
import Loader from "@skbkontur/react-ui/components/Loader/Loader";
import getPluralForm from "../../../../utils/getPluralForm";

import styles from "./copyGroupModal.less";

class CopyGroupModal extends Component {

	state = {
		groupId: null,
		courseId: null,
		changeOwner: true,
		groups: [],
		error: null,
		instructors: [],
		courses: [],
		loadingGroups: false,
		loading: false,
	};

	componentDidMount() {
		const currentCourseId = this.props.course.id;

		this.loadCourses();
		this.loadCourseInstructors(currentCourseId);
	}

	loadCourses = () => {
		api.courses.getUserCourses()
		.then(json => {
			let courses = json.courses;
			this.setState({
				courses,
			});
		}).catch(console.error);
	};

	loadCourseInstructors = (courseId) => {
		api.users.getCourseInstructors(courseId, undefined, 200)
		.then(json => {
			let instructors = json.users.map(item => item.user);
			this.setState({
				instructors,
			});
		}).catch(console.error);
	};

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
		);
	};

	render() {
		const {onCloseModal, course} = this.props;

		return (
			<Modal onClose={onCloseModal} width="100%" alignTop={true}>
				<Modal.Header>Скопировать группу из курса</Modal.Header>
				<form onSubmit={this.onSubmit}>
					<Modal.Body>
						<div className={styles["modal-content"]}>
							<p className={styles["common-info"]}>Новая группа будет создана для
								курса <b>«{course.title}»</b>.
								Скопируются все настройки группы (в том числе владелец),
								в неё автоматически добавятся студенты из копируемой группы.
								Преподаватели тоже будут добавлены в группу, если у них есть права
								на&nbsp;курс <b>«{course.title}»</b>.
							</p>
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
					Вы можете выбрать курс, в котором являетесь преподавателем
				</p>
				<label className={styles["select-course"]}>
					<Select
						autofocus
						required
						items={this.getCourseOptions()}
						onChange={this.onCourseChange}
						width="200"
						placeholder="Выберите курс"
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
					Вам доступны только те группы, в которых вы являетесь преподавателем
				</p>
				<Loader type="normal" active={this.state.loadingGroups}>
					<label className={styles["select-group"]}>
						<Select
							autofocus
							required
							items={this.getGroupOptions()}
							onChange={this.onGroupChange}
							width="200"
							placeholder="Выберите группу"
							value={groupId}
							error={this.hasError()}
							use="default"
							disabled={groups.length === 0}
						/>
					</label>
					{this.state.loadingGroups ? null : (this.checkGroups() && this.renderEmptyGroups())}
					{this.checkOwner() && this.renderChangeOwner()}
				</Loader>
			</React.Fragment>
		);
	};

	renderEmptyGroups() {
		return (
			<p className={styles["empty-group-info"]}>В выбранном вами курсе нет доступных групп</p>
		)
	}

	renderChangeOwner() {
		const {groupId, changeOwner} = this.state;
		const group = this.getGroup(groupId);

		return (
			<div className={styles["change-owner-block"]}>
				<p className={styles["change-owner-info"]}>
					Владелец этой группы <b>{group.owner.visibleName}</b> не является преподавателем
					курса <b>«{this.props.course.title}»</b>.
					Вы можете сделать себя владельцем скопированной группы.
				</p>
				<Checkbox checked={changeOwner} onChange={this.onChangeOwner}>
					Сделать меня владельцем группы
				</Checkbox>
			</div>
		)
	}

	hasError = () => {
		return this.state.error !== null;
	};

	getGroup = (groupId) => {
		const {groups} = this.state;

		return groups.find(g => g.id === groupId);
	};

	getCourseOptions = () => {
		const {courses} = this.state;

		return courses.map(course => [course.id, course.title]);
	};

	onCourseChange = (_, value) => {

		this.setState({
			courseId: value,
			groupId: null
		});

		this.loadGroups(value);
	};

	checkGroups = () => {
		const {courseId, groups} = this.state;
		if (!groups) {
			return false;
		}

		return (courseId && groups.length === 0);
	};

	getGroupOptions = () => {
		const {groups} = this.state;

		return groups.map(group => [group.id, `${group.name}: ${group.studentsCount} 
		${getPluralForm(group.studentsCount, 'студент', 'студента', 'студентов')}`]);
	};

	onGroupChange = (_, value) => {
		this.setState({groupId: value});
	};

	checkOwner = () => {
		const {groupId, instructors} = this.state;
		const group = this.getGroup(groupId);

		if (!group) {
			return false;
		}

		const ownerId = group.owner.id;
		const instructorsId = instructors.map(instructor => instructor.id);

		return !(instructorsId.includes(ownerId));
	};

	onChangeOwner = (_, value) => this.setState({changeOwner: value});

	onSubmit = async (e) => {
		const {groupId, courseId, changeOwner} = this.state;
		const currentCourseId = this.props.course.id;
		const {onCloseModal, onSubmit} = this.props;

		e.preventDefault();

		if (!courseId || !groupId) {
			this.setState({
				error: 'Выберите курс',
			});
			return;
		}

		this.setState({loading: true});
		try {
			const newGroup = await api.groups.copyGroup(groupId, currentCourseId, changeOwner);
			onCloseModal();
			onSubmit(newGroup.id);
		} catch (e) {
			console.error(e);
		} finally {
			this.setState({loading: false});
		}
	};
}

CopyGroupModal.propTypes = {
	course: PropTypes.object,
	onCloseModal: PropTypes.func,
	onSubmit: PropTypes.func,
};

export default CopyGroupModal;