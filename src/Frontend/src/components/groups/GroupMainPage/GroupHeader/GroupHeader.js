import React, { Component } from "react";
import PropTypes from "prop-types";
import Button from "@skbkontur/react-ui/components/Button/Button";
import Tabs from "@skbkontur/react-ui/components/Tabs/Tabs";
import Gapped from "@skbkontur/react-ui/components/Gapped/Gapped";
import CreateGroupModal from "../CreateGroupModal/CreateGroupModal";
import CopyGroupModal from "../CopyGroupModal/CopyGroupModal";

import styles from "./groupHeader.less";
import { Mobile, NotMobile } from "../../../../utils/responsive";

const TABS = {
	active: 'active',
	archived: 'archived',
};

class GroupHeader extends Component {

	state = {
		modalCreateGroup: false,
		modalCopyGroup: false,
	};

	render() {
		return (
			<React.Fragment>
				{this.renderHeader()}
				{this.state.modalCreateGroup &&
				<CreateGroupModal
					courseId={this.props.course.id}
					onCloseModal={this.onCloseModal}
					onSubmit={this.props.addGroup}
				/>
				}
				{this.state.modalCopyGroup &&
				<CopyGroupModal
					course={this.props.course}
					onCloseModal={this.onCloseModal}
					onSubmit={this.props.addGroup}
				/>
				}
			</React.Fragment>
		)
	}

	renderHeader() {
		return (
			<header className={styles["header"]}>
				<div className={styles["header-container"]}>
					<h2 className={styles["header-name"]}>Группы</h2>
					<div className={styles["buttons-container"]}>
						<Mobile>
							<Gapped gap={10}>
								<Button id="create" use="primary" size="small" onClick={this.openCreateGroupModal}>Создать
									группу</Button>
								<Button id="copy" use="default" size="small" onClick={this.openCopyGroupModal}>Скопировать
									из...</Button>
							</Gapped>
						</Mobile>
						<NotMobile>
							<Gapped gap={20}>
								<Button id="create" use="primary" size="medium" onClick={this.openCreateGroupModal}>Создать
									группу</Button>
								<Button id="copy" use="default" size="medium" onClick={this.openCopyGroupModal}>Скопировать
									группу из курса</Button>
							</Gapped>
						</NotMobile>
					</div>
				</div>
				<div className={styles["tabs-container"]}>
					<Tabs value={this.props.filter} onChange={this.onChange}>
						<Tabs.Tab id={TABS.active}>Активные</Tabs.Tab>
						<Tabs.Tab id={TABS.archived}>Архивные</Tabs.Tab>
					</Tabs>
				</div>
			</header>
		)
	}

	openCreateGroupModal = () => {
		this.setState({
			modalCreateGroup: true,
			modalCopyGroup: false,
		})
	};

	openCopyGroupModal = () => {
		this.setState({
			modalCopyGroup: true,
			modalCreateGroup: false,

		})
	};

	onCloseModal = () => {
		this.setState({
			modalCreateGroup: false,
			modalCopyGroup: false,

		})
	};

	onChange = (_, v) => {
		this.props.onTabChange(v);
	};
}

GroupHeader.propTypes = {
	onTabChange: PropTypes.func,
	filter: PropTypes.string,
	course: PropTypes.object,
	addGroup: PropTypes.func,
	groups: PropTypes.array,
};

export default GroupHeader;