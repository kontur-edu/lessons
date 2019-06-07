import React, { Component } from "react";
import Error404 from "./Error404";


export class UrlError extends Error {
	constructor(message, response) {
		super();
		this.response = response;
	}
}

class NotFoundErrorBoundary extends Component {
	state = {
		error: null,
	};

	componentDidCatch(error, errorInfo) {
		if (error instanceof UrlError) {
			this.setState({error})
		} else {
			Error();
		}
	}

	render() {
		if (this.state.error) {
			return (
				<Error404 />
			);
		}
		return this.props.children;
	}
}

export default NotFoundErrorBoundary;