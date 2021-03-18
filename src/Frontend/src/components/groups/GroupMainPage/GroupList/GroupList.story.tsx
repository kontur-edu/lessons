import React from "react";
import GroupList from "./GroupList";

import "./groupList.less";

export default {
	title: "Group/GroupList",
};

const mock = () => ({});
export const Default = (): React.ReactNode => <GroupList toggleArchived={ mock } deleteGroup={ mock } loading={ false }
														 userId={ '1' } courseId={ '1' } groups={ getGroups() }/>;

Default.storyName = "default";

function getGroups() {
	return [
		{
			id: '17',
			createTime: "2018-10-19T13:46:28.153",
			name: "asdfasdfasdfasdf",
			isArchived: false,
			owner: {
				id: "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
				firstName: "",
				lastName: "",
				visibleName: "paradeeva",
				gender: null,
			},
			inviteHash: "b7638c37-62c6-49a9-898c-38788169987c",
			isInviteLinkEnabled: true,
			isManualCheckingEnabled: false,
			isManualCheckingEnabledForOldSolutions: false,
			defaultProhibitFurtherReview: true,
			canStudentsSeeGroupProgress: true,
			studentsCount: 0,
			accesses: [],
			apiUrl: "/groups/17",
			areYouStudent: false,
		},
		{
			id: '14',
			createTime: null,
			name: "asdfghj",
			isArchived: false,
			owner: {
				id: "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
				firstName: "",
				lastName: "",
				visibleName: "paradeeva",
				gender: null,
			},
			inviteHash: "e22214b7-774c-4f14-b80c-910c6e715301",
			isInviteLinkEnabled: true,
			isManualCheckingEnabled: true,
			isManualCheckingEnabledForOldSolutions: true,
			defaultProhibitFurtherReview: true,
			canStudentsSeeGroupProgress: true,
			studentsCount: 0,
			accesses: [],
			apiUrl: "/groups/14",
			areYouStudent: false,
		},
	];
}
