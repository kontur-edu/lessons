import React, { useState } from "react";
import { Button, Tabs, Gapped } from "ui";

import { Mobile, NotMobile } from "src/utils/responsive";

import CreateGroupModal from "../CreateGroupModal/CreateGroupModal.js";
import CopyGroupModal from "../CopyGroupModal/CopyGroupModal.js";

import { CourseInfo } from "src/models/course";

import styles from "./groupHeader.less";

const TABS = {
	active: 'active',
	archived: 'archived',
};

interface Props {
	filter: string;
	course: CourseInfo;

	onTabChange: (tab: string) => void;
	addGroup: (groupId: string) => void;
}

function GroupHeader({ filter, course, onTabChange, addGroup, }: Props): React.ReactElement {
	const [{ modalCreateGroup, modalCopyGroup, }, setState] = useState({
		modalCreateGroup: false,
		modalCopyGroup: false,
	});

	return (
		<React.Fragment>
			{ renderHeader() }
			{ modalCreateGroup &&
			<CreateGroupModal
				courseId={ course.id }
				onCloseModal={ onCloseModal }
				onSubmit={ addGroup }
			/>
			}
			{ modalCopyGroup &&
			<CopyGroupModal
				course={ course }
				onCloseModal={ onCloseModal }
				onSubmit={ addGroup }
			/>
			}
		</React.Fragment>
	);

	function renderHeader() {
		return (
			<header className={ styles["header"] }>
				<div className={ styles["header-container"] }>
					<h2 className={ styles["header-name"] }>Группы</h2>
					<div className={ styles["buttons-container"] }>
						<Mobile>
							<Gapped gap={ 10 }>
								<Button use="primary" size="small" onClick={ openCreateGroupModal }>Создать
									группу</Button>
								<Button use="default" size="small" onClick={ openCopyGroupModal }>Скопировать
									из...</Button>
							</Gapped>
						</Mobile>
						<NotMobile>
							<Gapped gap={ 20 }>
								<Button use="primary" size="medium" onClick={ openCreateGroupModal }>Создать
									группу</Button>
								<Button use="default" size="medium" onClick={ openCopyGroupModal }>Скопировать
									группу из курса</Button>
							</Gapped>
						</NotMobile>
					</div>
				</div>
				<div className={ styles["tabs-container"] }>
					<Tabs value={ filter } onValueChange={ onChange }>
						<Tabs.Tab id={ TABS.active }>Активные</Tabs.Tab>
						<Tabs.Tab id={ TABS.archived }>Архивные</Tabs.Tab>
					</Tabs>
				</div>
			</header>
		);
	}

	function openCreateGroupModal() {
		setState({
			modalCreateGroup: true,
			modalCopyGroup: false,
		});
	}

	function openCopyGroupModal() {
		setState({
			modalCopyGroup: true,
			modalCreateGroup: false,
		});
	}

	function onCloseModal() {
		setState({
			modalCreateGroup: false,
			modalCopyGroup: false,
		});
	}

	function onChange(tab: string) {
		onTabChange(tab);
	}
}

export default GroupHeader;
