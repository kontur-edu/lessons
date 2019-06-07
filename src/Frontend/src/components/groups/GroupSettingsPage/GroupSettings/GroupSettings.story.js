import React from 'react';
import { storiesOf } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import GroupSettings from './GroupSettings';

import './groupSettings.less';

storiesOf('Settings/GroupSettings', module)
.add('default', () => (
	<GroupSettings group={{test: "test"}} updatedFields={{name: 'maria'}}
				   onChangeSettings={action('change')} />
));