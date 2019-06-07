import React, { Component } from "react";
import PropTypes from "prop-types";
import Checkbox from "@skbkontur/react-ui/components/Checkbox/Checkbox";

import styles from './groupScores.less';

const mapToServerName = {
	groupScores: 'areAdditionalScoresEnabledInThisGroup',
	allGroupScores: 'areAdditionalScoresEnabledForAllGroups',
	unitScores: 'canInstructorSetAdditionalScoreInSomeUnit',
};

class GroupScores extends Component {

	render() {
		return (
			<label className={styles["settings-checkbox"]}>
				{this.renderCheckbox()}
			</label>
		);
	}

	renderCheckbox() {
		const {score} = this.props;
		const isChecked = score[mapToServerName.groupScores] || score[mapToServerName.allGroupScores] || false;
		const isDisabled = score[mapToServerName.allGroupScores] || !score[mapToServerName.unitScores];

		return (
			<React.Fragment>
				<Checkbox checked={isChecked} disabled={isDisabled} onChange={this.onChange}>
					{score.name}
				</Checkbox>

				<p className={styles["settings-comment"]}>
					{score.description}
				</p>

				<div className={styles["settings-comment"]}>
					<p className={styles["exercise-comment"]}>
						{
							score[mapToServerName.allGroupScores] &&
							'Автор курса включил возможность получать баллы этого вида абсолютно для всех. ' +
							'Вы как преподаватель не можете отдельно включить или выключить эти баллы только в своей группе.'
						}
					</p>
					<p className={styles["exercise-comment"]}>
						{
							!score[mapToServerName.allGroupScores] && !score[mapToServerName.unitScores] &&
							'В курсе нет ни одного модуля, в котором вы могли бы дополнительно выставлять баллы этого вида.'
						}
					</p>
				</div>
			</React.Fragment>
		);
	}

	onChange = (_, value) => {
		const id = this.props.score.id;
		this.props.onChangeScores(id, mapToServerName.groupScores, value);
	};
}

GroupScores.propTypes = {
	score: PropTypes.object,
	onChangeScores: PropTypes.func,
};

export default GroupScores;