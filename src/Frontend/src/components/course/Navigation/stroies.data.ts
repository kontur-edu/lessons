import { CourseMenuItem, MenuItem, SlideProgressStatus } from "./types";
import { SlideType } from "src/models/slide";

const standardSlide: MenuItem<SlideType> = {
	containsVideo: false,
	id: "1",
	isActive: false,
	maxScore: 0,
	questionsCount: 0,
	quizMaxTriesCount: 0,
	score: 0,
	status: SlideProgressStatus.notVisited,
	title: "Title",
	type: SlideType.Lesson,
	url: "",
	visited: false,
};

export function getModuleNavigationProps(): MenuItem<SlideType>[] {
	return [
		{
			type: SlideType.Lesson,
			title: "Урок без видео",
			containsVideo: false,
		},
		{
			type: SlideType.Lesson,
			title: "Урок с видео",
			containsVideo: true,
		},
		{
			type: SlideType.Lesson,
			title: "Урок без видео",
			containsVideo: false,
		},
		{
			type: SlideType.Quiz,
			title: "Квиз",
		},
		{
			type: SlideType.Exercise,
			title: "Первый шаг",
		},
		{
			type: SlideType.Lesson,
			title: "Источники информации",
		},
		{
			type: SlideType.Lesson,
			title: "Числовые типы данных",
		},
		{
			type: SlideType.Exercise,
			title: "Неверный тип данных",
		},
		{
			type: SlideType.Exercise,
			title: "Ошибки преобразования типов",
		},
		{
			type: SlideType.Exercise,
			title: "Биткоины в массы!",
		},
		{
			type: SlideType.Lesson,
			title: "Строки",
		},
		{
			type: SlideType.Exercise,
			title: "Преобразование строки в число",
		},
		{
			type: SlideType.Lesson,
			title: "Арифметические операции и var",
		},
		{
			type: SlideType.Exercise,
			title: "Использование var",
		},
		{
			type: SlideType.Lesson,
			title: "Методы",
		},
		{
			type: SlideType.Exercise,
			title: "Добрый работодатель",
		},
		{
			type: SlideType.Exercise,
			title: "Главный вопрос Вселенной",
		},
		{
			type: SlideType.Exercise,
			title: "Разыскиваются методы!",
		},
		{
			type: SlideType.Lesson,
			title: "Переменные",
		},
		{
			type: SlideType.Quiz,
			title: "Области видимости",
		},
		{
			type: SlideType.Lesson,
			title: "Задачи на семинар",
		},
	].map((slide, index) => {
		const finalSlide = { ...standardSlide, ...slide };
		setSlideTitleFromProps(finalSlide);
		finalSlide.id = index.toString();
		return finalSlide;
	});
}

function setSlideTitleFromProps(slide: MenuItem<SlideType>) {
	const descriptions = [];

	const addToDescription = (description: string) => descriptions.push(description);

	switch (slide.type) {
		case SlideType.Lesson: {
			descriptions.push('Урок');
			if(slide.containsVideo) {
				addToDescription('с видео');
			}
			break;
		}
		case SlideType.Exercise:
			descriptions.push('Задача');
			break;
		case SlideType.Quiz:
			descriptions.push('Квиз');
			break;
		case SlideType.Flashcards:
			descriptions.push('Флешкарты');
			break;
	}

	if(slide.isActive) {
		addToDescription('выбраный');
	}

	if(slide.hide) {
		addToDescription('скрытый');
	}

	switch (slide.status) {
		case SlideProgressStatus.canBeImproved:
			addToDescription('можно улучшить');
			break;
		case SlideProgressStatus.done:
			addToDescription('пройден');
			break;
		case SlideProgressStatus.notVisited:
			addToDescription('не посещён');
			break;
	}

	slide.title = descriptions.join(', ');
}

function getCourseNav(): CourseMenuItem[] {
	return [{
		title: "Преподавателю о курсе",
		id: "c069ba64-e101-40e3-9b76-b65a1ae619ae",
		isActive: false,
		isNotPublished: true,
	}, {
		title: "Первое знакомство с C#",
		id: "e1beb629-6f24-279a-3040-cf111f91e764",
		isActive: true,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 0,
			slidesCount: 100,
			inProgressSlidesCount: 0,
			current: 0,
			max: 0,
		},
	}, {
		title: "Ошибки",
		id: "6c13729e-817b-a437-b9d3-275c01f8f4a8",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 25,
			slidesCount: 100,
			inProgressSlidesCount: 0,
			current: 0,
			max: 0,
		},
	}, {
		title: "Ветвления",
		id: "148775ee-9ffa-8932-64d6-d64380484169",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 50,
			slidesCount: 100,
			inProgressSlidesCount: 0,
			current: 0,
			max: 0,
		},
		isNotPublished: true,
		publicationDate: "2021-08-18T11:05:27"
	}, {
		title: "Циклы",
		id: "d083f956-8fa4-6024-04da-cfc8e17bd9db",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 75,
			slidesCount: 100,
			inProgressSlidesCount: 0,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Массивы",
		id: "c777829b-7226-9049-ddf9-895234334f3f",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 100,
			slidesCount: 100,
			inProgressSlidesCount: 0,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Коллекции, строки, файлы",
		id: "7d679719-4676-36c4-f4b3-44682fd6d8b0",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 50,
			slidesCount: 100,
			inProgressSlidesCount: 25,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Тестирование",
		id: "0299fc2c-1fdc-d85d-d654-4a6a1353f64d",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 50,
			slidesCount: 100,
			inProgressSlidesCount: 50,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Сложность алгоритмов",
		id: "fab42d9c-db92-daec-e883-7d5424eb6c13",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 0,
			slidesCount: 100,
			inProgressSlidesCount: 75,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Рекурсивные алгоритмы",
		id: "40de4f88-54d6-3c23-faee-0f9de37ad824",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 0,
			slidesCount: 100,
			inProgressSlidesCount: 100,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Поиск и сортировка",
		id: "e4df2f6b-dc5d-3cc1-1467-0edcc93c211a",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 50,
			slidesCount: 100,
			inProgressSlidesCount: 50,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Практикум",
		id: "0e56024a-ae74-4efa-d8a7-fea7fab5055b",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 100,
			slidesCount: 100,
			inProgressSlidesCount: 0,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Основы ООП",
		id: "b8ff29db-7416-c2f6-aa37-7e58a96ed597",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 0,
			slidesCount: 100,
			inProgressSlidesCount: 100,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Наследование",
		id: "8fe5a2fc-fe15-a2a6-87b8-74f4f36af51d",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 28,
			slidesCount: 100,
			inProgressSlidesCount: 54,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Целостность данных",
		id: "1557d92c-68e6-63d0-69ee-414354353685",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 30,
			slidesCount: 100,
			inProgressSlidesCount: 30,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}, {
		title: "Структуры",
		id: "97940709-9f6d-03f4-2090-63bd08befcf1",
		isActive: false,
		progress: {
			statusesBySlides: {},
			doneSlidesCount: 33,
			slidesCount: 100,
			inProgressSlidesCount: 66,
			current: 0,
			max: 0,
		},
		isNotPublished: true
	}];
}