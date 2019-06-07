import React from 'react';
import Responsive from 'react-responsive';

/* See also variables.less for values */

export const Desktop = props => <Responsive {...props} minWidth={992} />;
export const Tablet = props => <Responsive {...props} minWidth={768} maxWidth={991} />;
export const Mobile = props => <Responsive {...props} maxWidth={767} />;
export const NotMobile = props => <Responsive {...props} minWidth={768} />;

