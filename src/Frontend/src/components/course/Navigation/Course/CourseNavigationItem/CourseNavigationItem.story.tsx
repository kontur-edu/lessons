import React from "react";
import CourseNavigationItem from "./CourseNavigationItem";

const mock = () => ({});

const _CourseNavigationItem = (): React.ReactNode => (
	<nav>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			title="Первое знакомство с C#"
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 0,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			title="Ошибки"
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 0,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 0,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
			title="Тут много текста в одном пункте. Надеюсь, таких длинных текстов у нас не будет, но на всякий случай их нужно учесть"
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 0,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
			title="Тутдлинноесловобезпробеловкакжесложноегонабиратьононедолжноничеголоматьвидимолучшийвариантегообрезатьмноготочием"
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			title="Текущий модуль"
			isActive
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 0,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			title="Модуль с прогресс-баром"
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 10,
				slidesCount: 100,
				inProgressSlidesCount: 10,
				current: 0,
				max: 0,
			} }
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			title="Модуль с заполненным прогресс-баром"
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 100,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			title="Тут много текста в одном пункте. Надеюсь, таких длинных текстов у нас не будет, но на всякий случай их нужно учесть"
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 0,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
		/>
		<CourseNavigationItem
			onClick={ mock }
			id={ '1' }
			isActive={ false }
			title="Тутдлинноесловобезпробеловкакжесложноегонабиратьононедолжноничеголоматьвидимолучшийвариантегообрезатьмноготочием"
			progress={ {
				statusesBySlides: {},
				doneSlidesCount: 0,
				slidesCount: 100,
				inProgressSlidesCount: 0,
				current: 0,
				max: 0,
			} }
		/>
	</nav>
);

export default {
	title: "CourseNavigation",
};

export { _CourseNavigationItem };
_CourseNavigationItem.storyName = 'Items';
