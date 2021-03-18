import React from "react";

import { MenuHeader, MenuItem, MenuSeparator } from "ui";
import LinkComponent from "../../LinkComponent";

import { coursesPath, coursePath } from "src/consts/routes";

import { CourseInfo } from "src/models/course";
import { CourseAccessType, CourseRoleType, } from "src/consts/accessType";
import { DeviceType } from "src/consts/deviceType";

const VISIBLE_COURSES_COUNT = 10;
const COURSES_TO_BE_SORTED_BY_LAST_VISIT_COUNT = 4;

export function getCourseMenuItems(
	courseIds: string[],
	courseById: { [courseId: string]: CourseInfo },
	isSystemAdministrator: boolean
): React.ReactElement[] {
	courseIds = courseIds.filter(item => courseById[item] !== undefined);
	const coursesInfo = courseIds
		.filter(courseId => courseById[courseId])
		.map(courseId => courseById[courseId]);

	const lastVisitedCourses = [...coursesInfo].sort(sortCoursesByTimestamp)
		.slice(0, COURSES_TO_BE_SORTED_BY_LAST_VISIT_COUNT);
	const lastVisitedCoursesIds = new Set(lastVisitedCourses.map(c => c.id));

	const items = [
		...lastVisitedCourses,
		...coursesInfo.filter(c => !lastVisitedCoursesIds.has(c.id))
			.sort(sortCoursesByTitle)
			.slice(0, VISIBLE_COURSES_COUNT - COURSES_TO_BE_SORTED_BY_LAST_VISIT_COUNT)
	].map(
		courseInfo =>
			<MenuItem
				href={ `/${ coursePath }/${ courseInfo.id }` }
				key={ courseInfo.id }
				component={ LinkComponent }>{ courseInfo.title }
			</MenuItem>
	);
	if(courseIds.length > coursesInfo.length || isSystemAdministrator) {
		items.push(
			<MenuItem href={ coursesPath } key="-course-list" component={ LinkComponent }>
				<strong>Все курсы</strong>
			</MenuItem>);
	}
	return items;
}

function sortCoursesByTimestamp(courseInfo: CourseInfo, otherCourseInfo: CourseInfo) {
	if(!otherCourseInfo.timestamp && !courseInfo.timestamp) {
		return 0;
	}
	if(!courseInfo.timestamp) {
		return 1;
	}
	if(!otherCourseInfo.timestamp) {
		return -1;
	}
	return new Date(otherCourseInfo.timestamp).getTime() - new Date(courseInfo.timestamp).getTime();
}

function sortCoursesByTitle(courseInfo: CourseInfo, otherCourseInfo: CourseInfo) {
	return courseInfo.title.localeCompare(otherCourseInfo.title);
}

export function menuItems(courseId: string, role: CourseRoleType, accesses: CourseAccessType[],
	isTempCourse: boolean
): React.ReactNode {
	let items = [
		<MenuItem href={ "/Course/" + courseId } key="Course" component={ LinkComponent }>
			Просмотр курса
		</MenuItem>,

		<MenuSeparator key="CourseMenuSeparator1"/>,

		<MenuItem href={ `/${ courseId }/groups` } key="Groups" component={ LinkComponent }>
			Группы
		</MenuItem>,

		<MenuItem href={ "/Analytics/CourseStatistics?courseId=" + courseId } key="CourseStatistics"
				  component={ LinkComponent }>
			Ведомость курса
		</MenuItem>,

		<MenuItem href={ "/Analytics/UnitStatistics?courseId=" + courseId } key="UnitStatistics"
				  component={ LinkComponent }>
			Ведомость модуля
		</MenuItem>,

		<MenuItem href={ "/Admin/Certificates?courseId=" + courseId } key="Certificates"
				  component={ LinkComponent }>
			Сертификаты
		</MenuItem>,
	];

	const hasUsersMenuItem = role === CourseRoleType.courseAdmin || accesses.indexOf(
		CourseAccessType.addAndRemoveInstructors) !== -1;
	const hasCourseAdminMenuItems = role === CourseRoleType.courseAdmin;

	if(hasUsersMenuItem || hasCourseAdminMenuItems) {
		items.push(<MenuSeparator key="CourseMenuSeparator2"/>);
	}

	if(hasUsersMenuItem) {
		items.push(
			<MenuItem href={ "/Admin/Users?courseId=" + courseId + "&courseRole=Instructor" } key="Users"
					  component={ LinkComponent }>
				Студенты и преподаватели
			</MenuItem>);
	}

	if(hasCourseAdminMenuItems) {
		if(isTempCourse) {
			items = items.concat([
				<MenuItem href={ "/Admin/TempCourseDiagnostics?courseId=" + courseId } key="Diagnostics"
						  component={ LinkComponent }>
					Диагностика
				</MenuItem>
			]);
		} else {
			items = items.concat([
				<MenuItem href={ "/Admin/Packages?courseId=" + courseId } key="Packages"
						  component={ LinkComponent }>
					Экспорт и импорт курса
				</MenuItem>,

				<MenuItem href={ "/Admin/Units?courseId=" + courseId } key="Units"
						  component={ LinkComponent }>
					Модули
				</MenuItem>
			]);
		}

		items = items.concat([
			<MenuItem href={ "/Grader/Clients?courseId=" + courseId } key="GraderClients"
					  component={ LinkComponent }>
				Клиенты грейдера
			</MenuItem>
		]);
	}

	items = items.concat([
		<MenuSeparator key="CourseMenuSeparator3"/>,

		<MenuItem href={ "/Admin/Comments?courseId=" + courseId } key="Comments"
				  component={ LinkComponent }>
			Комментарии
		</MenuItem>,

		<MenuItem href={ "/Admin/CheckingQueue?courseId=" + courseId } key="ManualCheckingQueue"
				  component={ LinkComponent }>
			Код-ревью и проверка тестов
		</MenuItem>,
	]);

	return items;
}

export function sysAdminMenuItems(courseIds: string[],
	courseById: { [courseId: string]: CourseInfo }
): React.ReactNode {
	return [
		<MenuItem href="/Account/List?role=SysAdmin" component={ LinkComponent } key="Users">
			Пользователи
		</MenuItem>,

		<MenuItem href="/Analytics/SystemStatistics" component={ LinkComponent } key="Statistics">
			Статистика
		</MenuItem>,

		<MenuItem href="/Sandbox" component={ LinkComponent } key="Sandbox">
			Песочница C#
		</MenuItem>,

		<MenuItem href="/Admin/StyleValidations" component={ LinkComponent } key="StyleValidations">
			Стилевые ошибки C#
		</MenuItem>,

		<MenuSeparator key="SysAdminMenuSeparator"/>,

		<MenuHeader key="Courses">
			Курсы
		</MenuHeader>,
	].concat(getCourseMenuItems(courseIds, courseById, true));
}

export const isIconOnly = (deviceType: DeviceType): boolean => deviceType === DeviceType.tablet || deviceType === DeviceType.mobile;

export const maxDropdownHeight = window.innerHeight - 50 - 20; // max == height - headerHeight - additiveBottomSpace(so its not touching the bottom)
