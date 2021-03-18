import React from 'react';
import Responsive from 'react-responsive';

/* See also variables.less for values */

type propType = unknown & { children?: React.ReactNode };

export const Desktop = (props: propType): React.ReactElement => <Responsive { ...props } minWidth={ 992 }/>;
export const Tablet = (props: propType): React.ReactElement => <Responsive { ...props } minWidth={ 768 }
																		   maxWidth={ 991 }/>;
export const Mobile = (props: propType): React.ReactElement => <Responsive { ...props } maxWidth={ 767 }/>;
export const NotMobile = (props: propType): React.ReactElement => <Responsive { ...props } minWidth={ 768 }/>;

