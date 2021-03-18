import React from "react";
import NavigationHeader from "./NavigationHeader";

const _CourseNavigationHeader = (): React.ReactNode => (
	<div>
		<NavigationHeader
			title="Основы программирования"
			courseProgress={ { current: 0, max: 0 } }
			groupsAsStudent={ [] }
		/>
		<NavigationHeader
			title="Основы программирования"
			courseProgress={ { current: 56, max: 100 } }
			groupsAsStudent={ [] }
		/>
		<NavigationHeader
			title="Основы программирования"
			courseProgress={ { current: 0, max: 0 } }
			groupsAsStudent={ [] }
		/>
		<NavigationHeader
			title="Основы программирования"
			courseProgress={ { current: 1, max: 1 } }
			groupsAsStudent={ [] }
		/>
	</div>
);

export default {
	title: "CourseNavigation",
};

export { _CourseNavigationHeader };
_CourseNavigationHeader.storyName = 'Headers';
