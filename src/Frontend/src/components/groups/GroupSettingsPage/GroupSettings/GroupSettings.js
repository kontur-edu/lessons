import React, { Component } from "react";
import PropTypes from "prop-types";
import Input from "@skbkontur/react-ui/components/Input/Input";
import Loader from "@skbkontur/react-ui/components/Loader/Loader";
import GroupScores from "./GroupScores/GroupScores";
import GroupSettingsCheckbox from "./GroupSettingsCheckbox/GroupSettingsCheckbox";

import styles from './groupSettings.less';

class GroupSettings extends Component {
	render() {
		const {group, scores, onChangeSettings, loading, onChangeScores} = this.props;

		return (
			<Loader type="big" active={loading}>
				<div className={styles.wrapper}>
					{this.renderChangeGroupName()}
					<div className={`${styles["checkbox-block"]} ${styles.settings}`}>
						<h4 className={styles["settings-header"]}>Код-ревью и проверка тестов</h4>
						<GroupSettingsCheckbox
							group={group}
							onChangeSettings={onChangeSettings} />
					</div>
					{scores.length > 0 &&
					<div className={styles.settings}>
						<h4 className={styles["settings-header"]}>Баллы</h4>
						<p className={styles["settings-text"]}>Преподаватели могут выставлять студентам группы
							следующие
							баллы:</p>
						{scores.map(score =>
							<GroupScores
								key={score.id}
								score={score}
								onChangeScores={onChangeScores} />
						)}
					</div>
					}
				</div>
			</Loader>
		)
	}

	renderChangeGroupName() {
		return (
			<div className={styles["change-name"]}>
				<header className={styles["change-name-header"]}>
					<h4 className={styles["change-name-label"]}>Название группы</h4>
				</header>
				<div className={styles["change-name-input"]}>
					<Input
						type="text"
						required
						size="small"
						error={this.props.error}
						value={this.inputValue}
						placeholder="Здесь вы можете изменить название группы"
						onChange={this.onChangeName}
						width="100%" />
				</div>
			</div>
		)
	}

	get inputValue() {
		const {name} = this.props;

		return name || '';
	}

	onChangeName = (_, value) => {
		this.props.onChangeName(value);
	};
}

GroupSettings.propTypes = {
	name: PropTypes.string,
	error: PropTypes.bool,
	loading: PropTypes.bool,
	group: PropTypes.object,
	scores: PropTypes.array,
	onChangeSettings: PropTypes.func,
	onChangeName: PropTypes.func,
	onChangeScores: PropTypes.func,
};

export default GroupSettings;