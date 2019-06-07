import React from 'react';
import { storiesOf } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import GroupList from './GroupList'
import { MemoryRouter } from 'react-router';

import './groupList.less';

storiesOf('Group/GroupList', module)
.addDecorator(story => (
	<MemoryRouter initialEntries={['/groups/']}>{story()}</MemoryRouter>
))
.add('default', () => (
	<GroupList groups={getGroups()} />
));

function getGroups() {
	return [
		{
			"id": 17,
			"createTime": "2018-10-19T13:46:28.153",
			"name": "asdfasdfasdfasdf",
			"isArchived": false,
			"owner": {
				"id": "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
				"firstName": "",
				"lastName": "",
				"visibleName": "paradeeva",
				"avatarUrl": null,
				"gender": null
			},
			"inviteHash": "b7638c37-62c6-49a9-898c-38788169987c",
			"isInviteLinkEnabled": true,
			"isManualCheckingEnabled": false,
			"isManualCheckingEnabledForOldSolutions": false,
			"defaultProhibitFurtherReview": true,
			"canStudentsSeeGroupProgress": true,
			"studentsCount": 0,
			"accesses": [],
			"apiUrl": "/groups/17"
		},
		{
			"id": 14,
			"createTime": null,
			"name": "asdfghj",
			"isArchived": false,
			"owner": {
				"id": "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
				"firstName": "",
				"lastName": "",
				"visibleName": "paradeeva",
				"avatarUrl": null,
				"gender": null
			},
			"inviteHash": "e22214b7-774c-4f14-b80c-910c6e715301",
			"isInviteLinkEnabled": true,
			"isManualCheckingEnabled": true,
			"isManualCheckingEnabledForOldSolutions": true,
			"defaultProhibitFurtherReview": true,
			"canStudentsSeeGroupProgress": true,
			"studentsCount": 0,
			"accesses": [],
			"apiUrl": "/groups/14"
		},
	]
}
