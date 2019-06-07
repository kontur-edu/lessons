import React from 'react';
import ReactDOM from 'react-dom';
import UlearnApp from "./App";

it('renders without crashing', () => {
	const div = document.createElement('div');
	ReactDOM.render(<UlearnApp />, div);
	ReactDOM.unmountComponentAtNode(div);
});
