import React, {Component} from 'react';
import PropTypes from "prop-types";

import CourseCards from "./CourseCards/CourseCards";
import Guides from "../Guides/Guides";
import Gapped from "@skbkontur/react-ui/Gapped";
import Button from "@skbkontur/react-ui/Button";
import Loader from "@skbkontur/react-ui/Loader";
import Flashcards from "../Flashcards/Flashcards";

import styles from './coursePage.less';
import {guides} from '../consts';

class CoursePage extends Component {
	constructor(props) {
		super(props);

		this.state = {
			showFlashcards: false,
		}
	}

	componentDidMount() {
		const {courseId, loadFlashcardsInfo, flashcardsPack, loadFlashcardsPack, loadStatistics} = this.props;

		document.getElementsByTagName('main')[0].classList.add(styles.pageContainer);

		if (!flashcardsPack) {
			loadFlashcardsPack(courseId);
		}

		loadStatistics(courseId);
		loadFlashcardsInfo(courseId);
	}

	componentWillUnmount() {
		document.getElementsByTagName('main')[0].classList.remove(styles.pageContainer);
	}

	render() {
		const {showFlashcards} = this.state;
		const {flashcardsInfoLoading, flashcardsInfo, courseId, flashcardsPack, unitsInfo, sendFlashcardRate, statistics, totalFlashcardsCount} = this.props;

		return (
			<Loader active={flashcardsInfoLoading} type="big">
				<Gapped gap={15} vertical={true}>

					{this.renderHeader()}

					{flashcardsInfo &&
					<CourseCards
						flashcardsInfo={flashcardsInfo}
						courseId={courseId}
						unitsInfo={unitsInfo}
					/>}

					<Guides guides={guides}/>

					{showFlashcards &&
					<Flashcards
						totalFlashcardsCount={totalFlashcardsCount}
						statistics={statistics}
						flashcards={flashcardsPack}
						courseId={courseId}
						onClose={() => this.hideFlashcards()}
						sendFlashcardRate={sendFlashcardRate}
					/>}
				</Gapped>
			</Loader>
		);
	}

	renderHeader() {
		return (
			<div className={styles.header}>
				<div>
					<h2 className={styles.title}>
						Флеш-карты для самопроверки
					</h2>
					<p className={styles.description}>
						Помогут лучше запомнить материал курса и подготовиться к экзаменам
					</p>
				</div>
				<Button use="primary" size='large' onClick={() => this.showFlashcards()}>
					Проверить себя
				</Button>
			</div>
		);
	}

	showFlashcards = () => {
		this.setState({
			showFlashcards: true,
		});
	};

	hideFlashcards = () => {
		const {loadFlashcardsInfo, courseId} = this.props;

		loadFlashcardsInfo(courseId);

		this.setState({
			showFlashcards: false,
		});
	};
}

CoursePage.propTypes = {
	courseId: PropTypes.string,
	flashcardsInfo: PropTypes.arrayOf(PropTypes.shape({
		unitTitle: PropTypes.string,
		unlocked: PropTypes.bool,
		cardsCount: PropTypes.number,
		unitId: PropTypes.string,
	})),
	flashcardsInfoLoading: PropTypes.bool,
	flashcardsPack: PropTypes.arrayOf(PropTypes.shape({
		id: PropTypes.string,
		question: PropTypes.string,
		answer: PropTypes.string,
		unitTitle: PropTypes.string,
		rate: PropTypes.string,
		unitId: PropTypes.string
	})),
	unitsInfo: PropTypes.arrayOf(PropTypes.shape({
		id: PropTypes.string,
		slides: PropTypes.array,
		title: PropTypes.string,
	})),
	totalFlashcardsCount: PropTypes.number,
	statistics: PropTypes.shape({
		notRated: PropTypes.number,
		rate1: PropTypes.number,
		rate2: PropTypes.number,
		rate3: PropTypes.number,
		rate4: PropTypes.number,
		rate5: PropTypes.number
	}),

	loadFlashcardsInfo: PropTypes.func,
	loadFlashcardsPack: PropTypes.func,
	sendFlashcardRate: PropTypes.func,
	loadStatistics: PropTypes.func,
};

export default CoursePage;
