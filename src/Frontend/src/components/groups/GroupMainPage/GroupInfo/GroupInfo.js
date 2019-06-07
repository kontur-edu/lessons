import React, { Component } from "react";
import PropTypes from 'prop-types';
import { Link } from 'react-router-dom';
import Icon from "@skbkontur/react-icons";
import Kebab from "@skbkontur/react-ui/components/Kebab/Kebab";
import MenuItem from "@skbkontur/react-ui/components/MenuItem/MenuItem";
import Gapped from "@skbkontur/react-ui/components/Gapped/Gapped";
import getPluralForm from "../../../../utils/getPluralForm";

import styles from "./groupInfo.less";
import { Mobile, NotMobile } from "../../../../utils/responsive";

class GroupInfo extends Component {

	render() {
		const {group} = this.props;

		if (!group) {
			return null;
		}

		const studentsCount = group.studentsCount || 0;
		const pluralFormOfStudents = getPluralForm(studentsCount, 'студент', 'студента', 'студентов');
		const isCodeReviewEnabled = group.isManualCheckingEnabled;
		const isProgressEnabled = group.canStudentsSeeGroupProgress;

		return (
			<div className={styles.wrapper}>
				<div className={styles["content-wrapper"]}>
					<Link className={styles["link-to-group-page"]}
						  to={`/${this.props.courseId}/groups/${group.id}/`} />
					<div className={styles["content-block"]}>
						<header className={styles.content}>
							<Link to={`/${this.props.courseId}/groups/${group.id}/`}
								  className={styles.groupLink}>
								<h3 className={styles["group-name"]}>{group.name}</h3>
							</Link>
							<div className={styles["students-count"]}>
								{studentsCount} {pluralFormOfStudents}
							</div>
							{this.renderTeachers()}
						</header>
						<div className={styles["group-settings"]}>
							{this.renderSetting(isProgressEnabled, 'Ведомость включена', 'Ведомость выключена')}
							{this.renderSetting(isCodeReviewEnabled, 'Код-ревью включено', 'Код-ревью выключено')}
						</div>
					</div>
				</div>
				{this.renderActions()}
			</div>
		)
	}

	renderTeachers() {
		const {group} = this.props;
		const teachersList = group.accesses.map(item => item.user.visibleName);
		const shortTeachersList = teachersList.filter((item, index) => index < 2);
		const teachersExcess = teachersList.length - shortTeachersList.length;
		const owner = group.owner.visibleName || 'Неизвестный';
		const teachers = [owner, ...shortTeachersList];
		const teachersCount = teachers.length;
		const pluralFormOfTeachers = getPluralForm(teachersCount, 'Преподаватель', 'Преподаватели');

		return (
			<div className={styles["teachers-list"]}>
				{`${pluralFormOfTeachers}: ${teachers.join(', ')} `}
				{teachersExcess > 0 &&
				<Link className={styles["link-to-group-members"]}
					  to={`/${this.props.courseId}/groups/${group.id}/members`}>
					и ещё {teachersExcess}
				</Link>
				}
			</div>
		)
	}

	renderSetting(enabled, textIfEnabled, textIfDisabled) {
		return (
			<div className={enabled ? styles["settings-on"] : styles["settings-off"]}>
				<Gapped gap={5}>
					{enabled ? <Icon name="Ok" /> : <Icon name="Delete" />}
					{enabled ? textIfEnabled : textIfDisabled}
				</Gapped>
			</div>
		)
	}

	renderActions() {
		const {group} = this.props;

		let menuItems = [
			<MenuItem onClick={() => this.props.toggleArchived(group, !group.isArchived)} key="toggleArchived">
				<Gapped gap={5}>
					<Icon name="ArchiveUnpack" />
					{group.isArchived ? 'Восстановить' : 'Архивировать'}
				</Gapped>
			</MenuItem>,
			<MenuItem onClick={() => this.props.deleteGroup(group, group.isArchived ?
				'archiveGroups' : 'groups')} key="delete">
				<Gapped gap={5}>
					<Icon name="Delete" />
					Удалить
				</Gapped>
			</MenuItem>
		];

		/* TODO (andgein): Change to size="medium" inside of <Mobile> after updating to the new react-ui version */
		return (
			<div className={styles["group-action"]}>
				<Mobile>
					<Kebab size="large" positions={["left top"]} disableAnimations={true}>
						{menuItems}
					</Kebab>
				</Mobile>
				<NotMobile>
					<Kebab size="large" positions={["bottom right"]} disableAnimations={false}>
						{menuItems}
					</Kebab>
				</NotMobile>
			</div>
		)
	}
}

GroupInfo.propTypes = {
	courseId: PropTypes.string.isRequired,
	group: PropTypes.object.isRequired,
	deleteGroup: PropTypes.func,
	toggleArchived: PropTypes.func,
};

export default GroupInfo;