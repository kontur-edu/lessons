import React, { Component } from "react";
import PropTypes from "prop-types";
import Loader from "@skbkontur/react-ui/components/Loader/Loader";
import GroupInfo from "../GroupInfo/GroupInfo";

import styles from "./groupList.less";

class GroupList extends Component {
	render() {
		return (
			<section className={styles.wrapper}>
				{this.props.loading &&
				<div className={styles.loaderWrapper}>
					<Loader type="big" active={true} />
				</div>
				}
				{!this.props.loading &&
				<div className={styles.content}>
					{this.props.groups && this.props.groups
					.sort((a, b) => a.name.localeCompare(b.name))
					.map(group =>
						<GroupInfo
							key={group.id}
							courseId={this.props.courseId}
							group={group}
							deleteGroup={this.props.deleteGroup}
							toggleArchived={this.props.toggleArchived}
						/>)
					}
				</div>
				}
				{!this.props.loading && this.props.groups && this.props.groups.length === 0 &&
				<div className={styles.noGroups}>
					{this.props.children}
				</div>
				}
			</section>
		);
	}
}

GroupList.propTypes = {
	courseId: PropTypes.string.isRequired,
	groups: PropTypes.array,
	loading: PropTypes.bool,
	deleteGroup: PropTypes.func,
	toggleArchived: PropTypes.func,
};

export default GroupList;