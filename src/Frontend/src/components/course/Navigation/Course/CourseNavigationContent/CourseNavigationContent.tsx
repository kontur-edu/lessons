import React from "react";

import CourseNavigationItem from '../CourseNavigationItem';

import { CourseMenuItem, } from '../../types';

import styles from './CourseNavigationContent.less';


interface Props {
	items: CourseMenuItem[];
	getRefToActive: React.RefObject<HTMLLIElement>;
}

function CourseNavigationContent({ items, getRefToActive, }: Props): React.ReactElement {
	return (
		<ol className={ styles.root }>
			{ items.map(
				(item) => <CourseNavigationItem getRefToActive={ getRefToActive } key={ item.id } { ...item }/>) }
		</ol>
	);
}

export default CourseNavigationContent;
