import React, { Component } from "react";
import { YMInitializer } from "react-yandex-metrika";
import ym from 'react-yandex-metrika';
import * as PropTypes from "prop-types";
import withRouter from "react-router-dom/es/withRouter";

class YandexMetrika extends Component {
	componentWillReceiveProps(nextProps, nextContext) {
		ym('hit', nextProps.location.pathname + nextProps.location.search);
	}

	render() {
		return (
			<div>
				<YMInitializer accounts={[25997251]} options={{
					clickmap: true,
					trackLinks: true,
					accurateTrackBounce: true,
					webvisor: true,
				}} />
			</div>
		);
	}

	static propTypes = {
		match: PropTypes.object.isRequired,
		location: PropTypes.object.isRequired,
		history: PropTypes.object.isRequired
	}
}

export default withRouter(YandexMetrika);