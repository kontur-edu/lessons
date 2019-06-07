import { configure } from '@storybook/react';
import { configureViewport } from '@storybook/addon-viewport';

const viewports = {
	desktop: {
		name: 'desktop',
		styles: {
			width: '1280px',
			height: '1024px',
		},
	},
	tablet: {
		name: 'tablet',
		styles: {
			width: '800px',
			height: '600px',
		},
	},
	mobile: {
		name: 'mobile',
		styles: {
			width: '320px',
			height: '240px',
		},
	},
};

configureViewport({
	defaultViewport: 'responsive',
	viewports,
});

function loadStories() {
	const req = require.context('../src', true, /\.story\.js$/);
	req.keys().forEach(filename => req(filename));
}

configure(loadStories, module);