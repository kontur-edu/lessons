import React from "react";
import PropTypes from "prop-types";
import Link from "@skbkontur/react-ui/components/Link/Link";

import styles from "./Header.less";

const Author = (props) => (
	<h3 className={styles.author}>{props.name}</h3>
);

export default function Header(props) {
	const { name, children, canViewProfiles, profileUrl } = props;
	return (
		<div className={styles.header}>
			{canViewProfiles ? <Link href={profileUrl}><Author name={name} /></Link> :
				<Author name={name}/>}
			{children}
		</div>
	);
}

Header.propTypes = {
	name: PropTypes.string,
	children: PropTypes.array,
	canViewProfile: PropTypes.bool,
	profileUrl: PropTypes.string
};
