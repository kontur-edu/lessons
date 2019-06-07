import React from "react";
import PropTypes from "prop-types";
import Link from "@skbkontur/react-ui/components/Link/Link";

import styles from "./Profile.less"

function Profile(props) {
	const {isSysAdmin, systemAccesses, user} = props;
	const canViewProfiles = systemAccesses.includes("viewAllProfiles") || isSysAdmin;
	const profileUrl = `/Account/Profile?userId=${user.id}`;

	return canViewProfiles
		? <Link href={profileUrl}>{user.visibleName}</Link>
		: <div className={styles.name}>{user.visibleName}</div>;
}

Profile.propTypes = {
	user: PropTypes.object,
	systemAccesses: PropTypes.array,
	isSysAdmin: PropTypes.bool,
};

export default Profile;
