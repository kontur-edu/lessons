import React from "react";
import ReactDOM from "react-dom";

/* This module allows to insert react components inside legacy layout.
 * Use following syntax to render react component:
 *
 * <div class="react-render" data-component="comment" data-props="@(new { first = "First props", second="Second props"}.JsonSerialize())"></div>
 *
 * Here `class="react-render"` is a trigger for work,
 * 		`data-component` is a component name (use should define it in Frontend/src/externalComponentRenderer.js), and
 * 		`data-props` are json-serialized props for the component.
 *
 * 	Child components are not supported now.
 *
 * 	See Ulearn.Web/Scripts/react-renderer.js for the second part of this module.
 */

/* Import all components you want to insert into legacy (cshtml+jquery) layout */

import CommentsView from "./components/comments/CommentsView/CommentsView";
import CommentsList from "./components/comments/CommentsList/CommentsList";
import Comment from "./components/comments/Comment/Comment";
import CommentSendForm from "./components/comments/CommentSendForm/CommentSendForm";

/* Define names for all components you want to use */
const components = {
	"CommentsView": CommentsView,
	"CommentsList": CommentsList,
	"Comment": Comment,
	"CommentSendForm": CommentSendForm,
};

window.renderReactComponent = function (componentType, element, props) {
	if (components[componentType] === undefined) {
		throw new Error("Unknown component type: " + componentType + ". Allowed types are: [" + Object.keys(components).join(", ") + "]");
	}

	let Component = components[componentType];

	ReactDOM.render(<Component {...props} />, element);
};