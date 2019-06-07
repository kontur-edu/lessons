import React from 'react';
import { storiesOf } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import { MemoryRouter } from 'react-router';
import GroupInfo from './GroupInfo';

import './groupInfo.less';

storiesOf('Group/GroupInfo', module)
.addDecorator(story => (
	<MemoryRouter initialEntries={['/groups/17']}>{story()}</MemoryRouter>
))
.add('default', () => (
	<GroupInfo group={getGroup()} />
));

function getGroup() {
	return {
		"id": 17,
		"name": "asdfasdfasdfasdf",
		"isArchived": false,
		"owner": {
			"id": "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
			"visibleName": "paradeeva",
			"avatarUrl": null,
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
	}
}