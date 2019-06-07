import { Component } from "react";
import React from "react";
import styles from "./pages.less"

export class Page extends Component {
	componentDidMount() {
		window.scrollTo(0, 0);
	}

	render() {
		return (
			<div className={styles.wrapper}>
				<div className={styles.contentWrapper}>
					{this.props.children}
				</div>
			</div>
		)
	}
}
