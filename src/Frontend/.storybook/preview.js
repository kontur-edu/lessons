import React from "react";
import { Provider } from "react-redux";
import { BrowserRouter } from "react-router-dom";

import configureStore from "src/configureStore";
import theme from "src/uiTheme";
import 'src/common.less';
import 'moment/locale/ru';
import "moment-timezone";

import { ThemeContext } from "ui";

const viewports = {
	desktop: {
		name: 'desktop',
		styles: {
			width: '1920px',
			height: '1080px',
		},
	},
	laptop:{
		name:'laptop',
		styles:{
			width:'1366px',
			height:'768px'
		}
	},
	tablet: {
		name: 'tablet',
		styles: {
			width: '768px',
			height: '1024px',
		},
	},
	mobile: {
		name: 'mobile',
		styles: {
			width: '480px',
			height: '800px',
		},
	},
};

const reduxStore = configureStore();

export const parameters = {
	viewport: {
		viewports: viewports,
		defaultViewport: 'desktop',
	},
}

export const decorators = [
	(Story) => (
		<Provider store={ reduxStore }>
			<ThemeContext.Provider value={ theme }>
				<BrowserRouter>
					<Story/>
				</BrowserRouter>
			</ThemeContext.Provider>
		</Provider>
	),
];
