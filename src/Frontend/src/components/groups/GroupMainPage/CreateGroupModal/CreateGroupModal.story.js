import React from 'react';
import { storiesOf } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import CreateGroupModal from './CreateGroupModal';

import './createGroupModal.less';

storiesOf('Group/CreateGroupModal', module)
.add('default', () => (
	<CreateGroupModal onCloseModal={action('onCloseModal')} courseId={'123'} />
));