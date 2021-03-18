import React from "react";
import NavigationItem from "./NavigationItem";
import { SlideType } from "src/models/slide";

const mock = () => ({});
const metro = {
	isFirstItem: false,
	isLastItem: false,
	connectToPrev: false,
	connectToNext: false,
};

const _NavigationItem = (): React.ReactNode => (
	<nav>
		<NavigationItem
			onClick={ mock }
			isActive={ false }
			id={ '1' }
			maxScore={ 1 }
			type={ SlideType.Lesson }
			visited={ false }
			metro={ metro }
			title="Пункт меню со счетом"
			score={ 0.45 }
			url={ "" }/>
		<NavigationItem
			onClick={ mock }
			isActive={ false } id={ '1' }
			maxScore={ 1 }
			type={ SlideType.Lesson }
			visited={ false }
			metro={ metro }
			title="Пункт меню со счетом и описанием"
			url={ "" }
			score={ 0 }
		/>
		<NavigationItem
			onClick={ mock }
			isActive={ false }
			id={ '1' }
			maxScore={ 1 }
			score={ 0 }
			visited={ false }
			type={ SlideType.Lesson }
			title="Пункт меню с метро"
			url={ "" }
			metro={ {
				...metro,
				connectToPrev: true,
			} }
		/>
		<NavigationItem
			title="Пункт меню с метро"
			url={ "" }
			type={ SlideType.Lesson }
			onClick={ mock }
			isActive={ false }
			id={ '1' }
			maxScore={ 1 }
			visited={ false }
			metro={ metro }
			score={ 0 }
		/>
		<NavigationItem
			title="Пункт меню с иконкой"
			url={ "" }
			type={ SlideType.Quiz }
			onClick={ mock }
			isActive={ false }
			id={ '1' }
			maxScore={ 1 }
			visited={ false }
			metro={ metro }
			score={ 0 }
		/>
		<NavigationItem
			id={ '1' }
			title="Пункт меню с иконкой и описанием и счетом"
			url={ "" }
			score={ 3 }
			maxScore={ 5 }
			isActive
			type={ SlideType.Quiz }
			onClick={ mock }
			visited={ false }
			metro={ metro }
		/>
		<NavigationItem
			title="Пункт меню с иконкой"
			url={ "" }
			type={ SlideType.Exercise }
			id={ '1' }
			score={ 3 }
			maxScore={ 5 }
			isActive={ false }
			onClick={ mock }
			visited={ false }
			metro={ metro }
		/>
		<NavigationItem
			title="Пункт меню с иконкой"
			url={ "" }
			visited
			type={ SlideType.Quiz }
			id={ '1' }
			score={ 3 }
			maxScore={ 5 }
			isActive={ false }
			onClick={ mock }
			metro={ {
				...metro,
				connectToNext: true,
			} }
		/>
		<NavigationItem
			title="Пункт меню с иконкой"
			url={ "" }
			visited
			type={ SlideType.Exercise }
			metro={ {
				...metro,
				connectToPrev: true,
				isLastItem: true,
			} }
			id={ '1' }
			score={ 3 }
			maxScore={ 5 }
			isActive={ false }
			onClick={ mock }
		/>
	</nav>
);

export default {
	title: "ModuleNavigation",
};
export { _NavigationItem };
_NavigationItem.storyName = "navigation items";
