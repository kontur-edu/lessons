import React from "react";
import classnames from 'classnames';

import { Link } from "react-router-dom";
import { flashcards, constructPathToSlide } from 'src/consts/routes';

import { FlashcardsStatistics } from "../../types";

import styles from './Flashcards.less';
import ProgressBarCircle from "../../ProgressBar/ProgressBarCircle";


interface Props {
	courseId: string;
	statistics: FlashcardsStatistics;

	isActive: boolean;

	toggleNavigation: () => void;
}


function Flashcards({ courseId, isActive, toggleNavigation, statistics, }: Props): React.ReactElement {
	return (
		<Link to={ constructPathToSlide(courseId, flashcards) }
			  className={ classnames(styles.root, { [styles.isActive]: isActive }) }
			  onClick={ toggleNavigation }
		>
			<h5 className={ styles.header }>Карточки</h5>
			<span className={ styles.text }>
				Вопросы для самопроверки
				{ statistics.count > 0 &&
				<ProgressBarCircle
					active={ isActive }
					successValue={ 1 - statistics.unratedCount / statistics.count }
					inProgressValue={ 0 }/>
				}
			</span>
		</Link>
	);
}

export default Flashcards;

