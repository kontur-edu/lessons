import React, { Component } from 'react';
import * as PropTypes from "prop-types";
import { withRouter } from "react-router-dom";

import DownloadedHtmlContent from '../components/common/DownloadedHtmlContent'
import LinkClickCapturer from "../components/common/LinkClickCapturer";


class AnyPage extends Component {
	constructor(props) {
		super(props);
		this.state = {
			href: props.location.pathname + props.location.search,
		};
	}

	componentWillReceiveProps(nextProps) {
		this.setState({
			href: nextProps.location.pathname + nextProps.location.search,
		});
	}

	render() {
		let url = this.state.href;
		if (url === "" || url === "/") {
			url = "/CourseList"
		}

		return (
			<LinkClickCapturer exclude={["/Certificate/", "/elmah/", "/Courses/"]}>
				<DownloadedHtmlContent url={url} />
			</LinkClickCapturer>
		)
	}

	static propTypes = {
		match: PropTypes.object.isRequired,
		location: PropTypes.object.isRequired,
		history: PropTypes.object.isRequired
	}
}

export default withRouter(AnyPage);