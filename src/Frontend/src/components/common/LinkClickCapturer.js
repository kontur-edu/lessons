import React, { Component } from "react";
import * as PropTypes from 'prop-types';

/* Component which captures <a> clicks and, if there is a matching route defined, routes them.
   Based on https://mattdistefano.com/blog/2017/11/30/handling-static-html-links-with-react-router
   and https://github.com/STRML/react-router-component/blob/master/lib/CaptureClicks.js
 */

const isModifiedEvent = (e => !!(e.metaKey || e.altKey || e.ctrlKey || e.shiftKey));

const getNearest = (branch, root, tagName) => {
	let candidate = branch;

	while (candidate && candidate.tagName !== tagName) {
		candidate =
			candidate.parentElement === root ? null : candidate.parentElement;
	}

	return candidate;
};

const hasTarget = (anchor) => anchor.target && anchor.target !== '_self';

const isSameDomain = (anchor) =>
	anchor &&
	window &&
	window.location &&
	anchor.protocol === window.location.protocol &&
	anchor.host === window.location.host;

const fileRegex = /\.[a-zA-Z0-9]{2,4}$/;

const isAnchorLink = (anchor) => anchor && anchor.href.indexOf("#") !== -1;

const isProbablyFile = (anchor) => anchor && anchor.pathname && fileRegex.test(anchor.pathname);

const isClientRoutable = (anchor) =>
	anchor &&
	isSameDomain(anchor) &&
	!hasTarget(anchor) &&
	!isProbablyFile(anchor);


class LinkClickCapturer extends Component {
	constructor(props) {
		super(props);
		this.exclude = props.exclude || [];
	}

	onClick = e => {
		// Ignore canceled events, modified clicks, and right clicks.
		if (e.defaultPrevented || e.button !== 0 || isModifiedEvent(e))
			return;

		const anchor = getNearest(e.target, e.currentTarget, 'A');

		if (!isClientRoutable(anchor))
			return;

		if (this.exclude.some(prefix => anchor.pathname.startsWith(prefix)))
			return;

		e.preventDefault();

		if (isAnchorLink(anchor))
			return;

		this.context.router.history.push({
			pathname: anchor.pathname,
			search: anchor.search
		});
	};

	render() {
		return (
			<div onClick={this.onClick}>
				{this.props.children}
			</div>
		);
	}

	static contextTypes = {
		router: PropTypes.shape({
			history: PropTypes.shape({
				push: PropTypes.func.isRequired,
				replace: PropTypes.func.isRequired,
				createHref: PropTypes.func.isRequired
			}).isRequired
		}).isRequired
	};

	static propTypes = {
		exclude: PropTypes.arrayOf(PropTypes.string).isRequired
	}
}

export default LinkClickCapturer;