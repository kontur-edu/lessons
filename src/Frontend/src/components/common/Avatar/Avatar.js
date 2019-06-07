import React, { Component } from "react";
import PropTypes from "prop-types";
import colorHash from "../../../utils/colorHash";

import styles from "./avatar.less";

class Avatar extends Component {

	render() {
		const {user, size} = this.props;
		const imageUrl = user.avatarUrl;
		let className = `${styles["photo-avatar"]} ${styles[size] || "big"}`;

		if (imageUrl) {
			return this.renderImage(imageUrl, className)
		}

		return this.renderCircle(className)
	}

	renderImage(url, className) {
		return (
			<img
				alt="Аватарка"
				className={className}
				src={url}
			/>
		);
	}

	renderCircle(className) {
		const userName = this.props.user.visibleName;
		const firstLetterIndex = userName.search(/[a-zа-яё]/i);
		const userFirstLetter = firstLetterIndex !== -1 ? userName[firstLetterIndex].toUpperCase() : "?";
		let divStyle = {
			backgroundColor: `${colorHash(userName)}`,
		};

		return (
			<div style={divStyle} className={`${className} ${styles["color-avatar"]}`}>{userFirstLetter}</div>
		)
	}
}

Avatar.propTypes = {
	size: PropTypes.string,
	user: PropTypes.shape({
		id: PropTypes.string.isRequired,
		avatarUrl: PropTypes.string,
		visibleName: PropTypes.string.isRequired,
	}),
};

export default Avatar;
