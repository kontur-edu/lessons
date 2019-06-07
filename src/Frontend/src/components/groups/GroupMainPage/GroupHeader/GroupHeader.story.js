import React from 'react';
import { storiesOf } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import GroupHeader from './GroupHeader';

import './groupHeader.less';

storiesOf('Group/GroupHeader', module)
.add('default', () => (
	<GroupHeader onTabChange={action('click')} filter="hello" />
));