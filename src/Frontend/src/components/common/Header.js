import React, { Component } from 'react'
import * as PropTypes from 'prop-types'
import { MenuItem, MenuSeparator } from "@skbkontur/react-ui/components/all";
import Icon from "@skbkontur/react-icons"
import MenuHeader from "@skbkontur/react-ui/MenuHeader"
import Tooltip from "@skbkontur/react-ui/Tooltip"
import Loader from "@skbkontur/react-ui/Loader"
import DropdownMenu from "@skbkontur/react-ui/DropdownMenu"
import DropdownContainer from "@skbkontur/react-ui/components/DropdownContainer/DropdownContainer"
import HeaderComponentErrorBoundary from "./Error/HeaderComponentErrorBoundary";
import { Link, withRouter } from "react-router-dom";
import { connect } from "react-redux";
import { findDOMNode } from "react-dom"

import styles from './Header.less';

import { getQueryStringParameter } from "../../utils";
import { toggleNavigation } from "../../actions/navigation";

import api from "../../api"


let accountPropTypes = PropTypes.shape({
	isAuthenticated: PropTypes.bool.isRequired,
	id: PropTypes.string,
	login: PropTypes.string,
	firstName: PropTypes.string,
	lastName: PropTypes.string,
	isSystemAdministrator: PropTypes.bool.isRequired,
	roleByCourse: PropTypes.object.isRequired,
	accessesByCourse: PropTypes.object.isRequired,
	accountProblems: PropTypes.arrayOf(PropTypes.object)
}).isRequired;

const LinkComponent = ({ href, ...rest }) => (<Link to={ href } { ...rest } />);

class Header extends Component {
	constructor(props) {
		super(props);

		this.state = Header.mapPropsToState(props);
	}

	static mapPropsToState(props) {
		const { account, courses } = props;
		const { currentCourseId, courseById } = courses;
		const { groupsAsStudent, roleByCourse, accessesByCourse, isSystemAdministrator } = account;

		let controllableCourseIds = [];
		if (isSystemAdministrator) {
			controllableCourseIds = Object.keys(courseById);
		} else {
			controllableCourseIds = Object.keys(roleByCourse)
				.filter(courseId => roleByCourse[courseId] !== 'tester')
				.map(s => s.toLowerCase());
			if (groupsAsStudent) {
				const groupsAsStudentIds = groupsAsStudent.map(g => g.courseId.toLowerCase());

				controllableCourseIds = [
					...new Set(controllableCourseIds.concat(groupsAsStudentIds)),
				]
				.filter((e) => courseById.hasOwnProperty(e))
				.sort((a, b) => {
					const first = courseById[a].title.toLowerCase();
					const second = courseById[b].title.toLowerCase();
					if (first > second)
						return 1;
					if (first < second)
						return -1;
					return 0;
				});
			}
		}

		let courseRole;
		if (isSystemAdministrator) {
			courseRole = 'courseAdmin';
		} else {
			courseRole = roleByCourse[currentCourseId];
			if (courseRole === undefined) {
				courseRole = "";
			}
		}

		const isCourseMenuVisible = (
			courses !== undefined &&
			currentCourseId !== undefined &&
			controllableCourseIds.indexOf(currentCourseId) !== -1 &&
			courseRole !== "" &&
			courseRole !== 'tester'
		);

		const courseAccesses = isCourseMenuVisible
			? accessesByCourse[currentCourseId] || []
			: [];

		return {
			isSystemAdministrator,
			controllableCourseIds,
			isCourseMenuVisible,
			courseRole,
			courseAccesses,
			currentCourseId
		};
	}

	componentWillReceiveProps(nextProps, nextContext) {
		this.setState(Header.mapPropsToState(nextProps));
	}

	render() {
		const { initializing } = this.props;

		/* Div should have class .header because some legacy javascript code uses $('.header') for calculating header height */
		return (
			<div className={ styles["header"] + " header" } id="header">
				{ Header.renderPhoneHeader() }
				{ Header.renderDefaultHeader() }
				{ !initializing && this.renderUserRoleMenu() }
			</div>
		)
	}

	static renderDefaultHeader() {
		return (
			<div className={ styles["visible-at-least-tablet"] }>
				<Logo>
					<LinkComponent href={ '/' }>
						Ulearn.me
					</LinkComponent>
				</Logo>
			</div>
		)
	}

	static renderPhoneHeader() {
		return (
			<div className={ styles.phoneHeaderElementsContainer }>
				<div className={ styles.phoneHeaderElement }>
					<LinkComponent href={ '/' }>
						U.me
					</LinkComponent>
				</div>
				{ isInsideCourse() &&
				<div className={ styles.phoneHeaderElement }>
					<NavMenuComponent/>
				</div> }
			</div>
		);
	}

	renderUserRoleMenu() {
		const { account } = this.props;

		return (
			<div>
				<HeaderComponentErrorBoundary>
					{ this.renderDefaultUserRoleMenu() }
					{ this.renderPhoneUserRoleMenu() }
				</HeaderComponentErrorBoundary>
				<HeaderComponentErrorBoundary className={ styles["header__menu"] }>
					<Menu account={ account }/>
				</HeaderComponentErrorBoundary>
			</div>
		)
	}

	renderDefaultUserRoleMenu() {
		const { isSystemAdministrator, controllableCourseIds, isCourseMenuVisible, courseRole, courseAccesses, currentCourseId } = this.state;

		return (
			<div className={ styles["visible-at-least-tablet"] }>
				{ isSystemAdministrator &&
				<SysAdminMenu controllableCourseIds={ controllableCourseIds }/> }
				{ !isSystemAdministrator && controllableCourseIds.length > 0 &&
				<MyCoursesMenu controllableCourseIds={ controllableCourseIds }/> }
				{ isCourseMenuVisible &&
				<CourseMenu courseId={ currentCourseId } role={ courseRole } accesses={ courseAccesses }/> }
			</div>
		);
	}

	renderPhoneUserRoleMenu() {
		const { isSystemAdministrator, controllableCourseIds, isCourseMenuVisible, courseRole, courseAccesses, currentCourseId } = this.state;

		return (
			<div className={ styles["visible-only-phone"] }>
				{ controllableCourseIds.length > 0 && <MobileCourseMenu
					isSystemAdministrator={ isSystemAdministrator }
					controllableCourseIds={ controllableCourseIds }
					isCourseMenuVisible={ isCourseMenuVisible }
					courseId={ isCourseMenuVisible ? currentCourseId : "" }
					role={ courseRole }
					accesses={ courseAccesses }
				/> }
			</div>
		);
	}
}

const mapStateToHeaderProps = ({ account, courses }) => {
	return { account, courses, }
};

const mapDispatchToHeaderProps = (dispatch) => {
	return {
		toggleNavigation: () => dispatch(toggleNavigation()),
	}
};

Header.propTypes = {
	account: accountPropTypes,
	initializing: PropTypes.bool.isRequired,
	toggleNavigation: PropTypes.func,
};


export default connect(mapStateToHeaderProps, mapDispatchToHeaderProps)(Header);

class Logo extends Component {
	render() {
		return (
			<div className={ styles["header__logo"] }>
				{ this.props.children }
			</div>
		)
	}
}

const isInsideCourse = () => {
	const pathname = window.location.pathname;
	return pathname
		.toLowerCase()
		.startsWith('/course/');
};

function NavMenu({ toggleNavigation }) {
	return (
		<button className={ styles.navMenuButton } onClick={ toggleNavigation }>
			<Icon size={ 22 } name="Menu"/>
		</button>
	)
}

NavMenu.propTypes = {
	toggleNavigation: PropTypes.func,
};

const mapStateToNavMenuProps = (state) => {
	return {};
};

const mapDispatchToNavMenuProps = (dispatch) => {
	return {
		toggleNavigation: () => dispatch(toggleNavigation()),
	};
};

const NavMenuComponent = connect(mapStateToNavMenuProps, mapDispatchToNavMenuProps)(NavMenu);

class AbstractMyCoursesMenu extends Component {
	static VISIBLE_COURSES_COUNT = 10;

	static _getCourseMenuItems(courseIds, courseById, isSystemAdministrator) {
		courseIds = courseIds.filter(item => courseById[item] !== undefined);
		let visibleCourseIds = courseIds.slice(0, AbstractMyCoursesMenu.VISIBLE_COURSES_COUNT);
		let items = visibleCourseIds.filter(courseId => courseById.hasOwnProperty(courseId)).map(courseId =>
			<MenuItem
				href={ "/Course/" + courseId }
				key={ courseId }
				component={ LinkComponent }>{ courseById[courseId].title }
			</MenuItem>
		);
		if (courseIds.length > visibleCourseIds.length || isSystemAdministrator)
			items.push(
				<MenuItem href="/Admin/Courses" key="-course-list" component={ LinkComponent }>
					<strong>Все курсы</strong>
				</MenuItem>);
		return items;
	}

	static propTypes = {
		controllableCourseIds: PropTypes.arrayOf(PropTypes.string).isRequired,
	};

	static mapStateToProps(state) {
		return {
			courses: state.courses
		}
	}
}

class SysAdminMenu extends AbstractMyCoursesMenu {
	static menuItems(courseIds, courseById) {
		throw new Error();
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
		].concat(AbstractMyCoursesMenu._getCourseMenuItems(courseIds, courseById, true));
	}

	render() {
		return (
			<div className={ styles["header__sysadmin-menu"] }>
				<DropdownMenu
					caption={
						<div>
							<span className={ styles["visible-only-phone"] }>
								<span className={ styles["icon"] }>
									<Icon name="DocumentGroup"/>
								</span>
							</span>
							<span className={ `${ styles["caption"] } ${ styles["visible-at-least-tablet"] }` }>
								Администрирование
								<span className={ styles["caret"] }/>
							</span>
						</div>
					}
				>
					{ SysAdminMenu.menuItems(this.props.controllableCourseIds, this.props.courses.courseById) }
				</DropdownMenu>
			</div>
		)
	}
}

SysAdminMenu = connect(SysAdminMenu.mapStateToProps)(SysAdminMenu);

class MyCoursesMenu extends AbstractMyCoursesMenu {
	static menuItems(courseIds, courseById) {
		return AbstractMyCoursesMenu._getCourseMenuItems(courseIds, courseById, false)
	}

	render() {
		return (
			<div className={ styles["header__my-courses-menu"] }>
				<DropdownMenu
					caption={
						<div>
							<span className={ styles["visible-only-phone"] }>
								<span className={ styles["icon"] }>
									<Icon name="DocumentGroup"/>
								</span>
							</span>
							<span className={ `${ styles["caption"] } ${ styles["visible-at-least-tablet"] }` }>
								Мои курсы
								<span className={ styles["caret"] }/>
							</span>
						</div>
					}
				>
					{ MyCoursesMenu.menuItems(this.props.controllableCourseIds, this.props.courses.courseById) }
				</DropdownMenu>
			</div>
		)
	}
}

MyCoursesMenu = connect(MyCoursesMenu.mapStateToProps)(MyCoursesMenu);

class CourseMenu extends Component {
	static menuItems(courseId, role, accesses) {
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

		let hasUsersMenuItem = role === 'courseAdmin' || accesses.indexOf('addAndRemoveInstructors') !== -1;
		let hasCourseAdminMenuItems = role === 'courseAdmin';

		if (hasUsersMenuItem || hasCourseAdminMenuItems)
			items.push(<MenuSeparator key="CourseMenuSeparator2"/>);

		if (hasUsersMenuItem)
			items.push(
				<MenuItem href={ "/Admin/Users?courseId=" + courseId } key="Users" component={ LinkComponent }>
					Студенты и преподаватели
				</MenuItem>);

		if (hasCourseAdminMenuItems)
			items = items.concat([
				<MenuItem href={ "/Admin/Packages?courseId=" + courseId } key="Packages"
						  component={ LinkComponent }>
					Экспорт и импорт курса
				</MenuItem>,

				<MenuItem href={ "/Admin/Units?courseId=" + courseId } key="Units"
						  component={ LinkComponent }>
					Модули
				</MenuItem>,

				<MenuItem href={ "/Grader/Clients?courseId=" + courseId } key="GraderClients"
						  component={ LinkComponent }>
					Клиенты грейдера
				</MenuItem>
			]);

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

	render() {
		const { courseId, role, accesses } = this.props;
		const courseById = this.props.courses.courseById;
		const course = courseById[courseId];
		if (typeof course === 'undefined') {
			return null;
		}

		return (
			<div className={ styles["header__course-menu"] }>
				<DropdownMenu
					menuWidth={ 300 }
					caption={
						<div>
						<span className={ styles["visible-only-phone"] }>
							<span className={ styles["icon"] }>
								<Icon name="DocumentSolid"/>
							</span>
						</span>
							<span className={ `${ styles["caption"] } ${ styles["visible-at-least-tablet"] }` }
								  title={ course.title }>
							<span className={ styles["courseName"] }>{ course.title }
							</span>
							<span className={ styles["caret"] }/>
						</span>
						</div> }
				>
					{ CourseMenu.menuItems(courseId, role, accesses) }
				</DropdownMenu>
			</div>
		)
	}

	static propTypes = {
		courseId: PropTypes.string.isRequired,
		courses: PropTypes.object,
		role: PropTypes.string.isRequired,
		accesses: PropTypes.arrayOf(PropTypes.string).isRequired
	};

	static mapStateToProps(state) {
		return {
			courses: state.courses
		}
	}
}

CourseMenu = connect(CourseMenu.mapStateToProps)(CourseMenu);

class MobileCourseMenu extends AbstractMyCoursesMenu {
	render() {

		return (
			<div className={ styles["header__course-menu"] }>
				<DropdownMenu
					menuWidth={ 250 }
					caption={
						<span className={ styles["icon"] }>
							<Icon name="DocumentSolid"/>
						</span> }
				>
					{ this.props.isCourseMenuVisible ? CourseMenu.menuItems(this.props.courseId, this.props.role, this.props.accesses) : null }
					{ this.props.isCourseMenuVisible ? <MenuSeparator/> : null }
					{
						this.props.isSystemAdministrator
							? SysAdminMenu.menuItems(this.props.controllableCourseIds, this.props.courses.courseById)
							: MyCoursesMenu.menuItems(this.props.controllableCourseIds, this.props.courses.courseById)
					}
				</DropdownMenu>
			</div>
		)
	}

	static propTypes = {
		isCourseMenuVisible: PropTypes.bool.isRequired,
		courseId: PropTypes.string.isRequired,
		role: PropTypes.string.isRequired,
		accesses: PropTypes.arrayOf(PropTypes.string).isRequired,
		isSystemAdministrator: PropTypes.bool.isRequired,
		controllableCourseIds: PropTypes.arrayOf(PropTypes.string).isRequired,
		courses: PropTypes.object
	}
}

MobileCourseMenu = connect(MobileCourseMenu.mapStateToProps)(MobileCourseMenu);

class Menu extends Component {
	render() {
		let returnUrl = this.props.location.pathname + this.props.location.search;
		if (returnUrl.startsWith("/Login")
			|| returnUrl.startsWith("/Account/Register")
			|| returnUrl.startsWith("/Login/ExternalLoginConfirmation")
			|| returnUrl.startsWith("/Login/ExternalLoginCallback")) {
			returnUrl = getQueryStringParameter("returnUrl");
		}

		if (this.props.account.isAuthenticated) {
			return (
				<div className={ styles["header__menu"] }>
					<NotificationsMenu/>
					<ProfileLink account={ this.props.account }/>
					<Separator/>
					<LogoutLink/>
				</div>
			)
		} else {
			return (
				<div className={ styles["header__menu"] }>
					<RegistrationLink returnUrl={ returnUrl }/>
					<Separator/>
					<LoginLink returnUrl={ returnUrl }/>
				</div>
			)
		}
	}

	static propTypes = {
		account: accountPropTypes,
		location: PropTypes.object,
	}
}

Menu = withRouter(Menu);

class NotificationsMenu extends Component {
	constructor(props) {
		throw new Error();
		super(props);
		this.onClick = this.onClick.bind(this);

		this.state = {
			isOpened: false,
			isLoading: false,
			notificationsHtml: "",
			counter: props.notifications.count,
			windowWidth: window.innerWidth
		}
	}

	componentWillReceiveProps(nextProps, nextContext) {
		this.setState({
			counter: nextProps.notifications.count
		});
	}

	componentDidMount() {
		window.addEventListener('resize', this._handleWindowSizeChange);
		document.addEventListener('mousedown', this._handleClickOutside);
		document.addEventListener('click', this._handleClickInsideNotification);
	}

	componentWillUnmount() {
		window.removeEventListener('resize', this._handleWindowSizeChange);
		document.removeEventListener('mousedown', this._handleClickOutside);
		document.addEventListener('click', this._handleClickInsideNotification);
	}

	_handleWindowSizeChange = () => {
		this.setState({ windowWidth: window.innerWidth });
	};

	_handleClickOutside = (event) => {
		if (this.ref && !this.ref.contains(event.target) && this.dropdownContainerRef &&
			!this.dropdownContainerRef.contains(event.target)) {
			this.setState({
				isOpened: false,
			});
		}
	};

	_handleClickInsideNotification = (event) => {
		let node = event.target;

		while (true) {
			if (!node || (node.classList && node.classList.contains('notifications__new-comment-notification'))) {
				break;
			}
			node = node.parentNode;
		}

		if (this.ref && this.dropdownContainerRef && (this.ref.contains(node) ||
			this.dropdownContainerRef.contains(node))) {
			this.setState({
				isOpened: false,
			});
		}

	};

	static _loadNotifications() {
		return fetch("/Feed/NotificationsPartial", { credentials: "include" }).then(
			response => response.text()
		)
	}

	onClick() {
		if (this.state.isOpened) {
			this.setState({
				isOpened: false,
			});
		} else {
			this.setState({
				isOpened: true,
				isLoading: true,
			});
			NotificationsMenu._loadNotifications().then(
				notifications => {
					this.props.resetNotificationsCount();
					this.setState({
						isLoading: false,
						notificationsHtml: notifications
					})
				});
		}
	}

	render() {
		const { windowWidth, isOpened, counter, isLoading, notificationsHtml } = this.state;
		const isMobile = windowWidth <= 767;
		return (
			<div className={ isOpened ? styles["opened"] : "" } ref={ node => this.ref = node }>
				<NotificationsIcon counter={ counter } onClick={ this.onClick }/>
				{
					isOpened &&
					<DropdownContainer getParent={ () => findDOMNode(this) } offsetY={ 0 } align="right"
									   offsetX={ isMobile ? -112 : 0 }>
						<div className={ styles["dropdown-container"] }
							 ref={ node => this.dropdownContainerRef = node }>
							<Notifications isLoading={ isLoading } notifications={ notificationsHtml }/>
						</div>
					</DropdownContainer>
				}
			</div>
		)
	}

	static mapStateToProps(state) {
		return {
			notifications: state.notifications
		}
	}

	static mapDispatchToProps(dispatch) {
		return {
			resetNotificationsCount: () => dispatch({
				type: 'NOTIFICATIONS__COUNT_RESETED'
			})
		};
	}
}

NotificationsMenu = connect(NotificationsMenu.mapStateToProps, NotificationsMenu.mapDispatchToProps)(NotificationsMenu);

class NotificationsIcon extends Component {
	render() {
		return (
			<div
				className={ `${ styles["header__notifications-icon"] } ${ this.props.counter === 0 ? styles["without-counter"] : "" }` }
				onClick={ this.props.onClick }>
                <span className={ styles["icon"] }>
                    <Icon name="NotificationBell"/>
                </span>
				{
					this.props.counter > 0 &&
					<span className={ styles["counter"] }>
                        { this.props.counter > 99 ? "99+" : this.props.counter }
                    </span>
				}
			</div>
		)
	}

	static propTypes = {
		counter: PropTypes.number.isRequired,
		onClick: PropTypes.func
	};

	static defaultProps = {
		onClick: () => 1
	};
}

class Notifications extends Component {
	render() {
		const { isLoading, notifications } = this.props;
		if (isLoading)
			return (
				<div className={ styles["notifications__dropdown"] }>
					<Loader type="normal" active={ true }/>
				</div>
			);
		else
			return <div className={ styles["notifications__dropdown"] }
						dangerouslySetInnerHTML={ { __html: notifications } }/>;
	}

	static propTypes = {
		isLoading: PropTypes.bool.isRequired,
		notifications: PropTypes.string.isRequired
	}
}

class ProfileLink extends Component {
	constructor(props) {
		super(props);
		this.openTooltip = this.openTooltip.bind(this);
		this.closeTooltip = this.closeTooltip.bind(this);
		this.state = {
			tooltipTrigger: 'opened',
		}
	}

	openTooltip() {
		this.setState({
			tooltipTrigger: 'opened'
		})
	}

	closeTooltip() {
		this.setState({
			tooltipTrigger: 'closed'
		})
	}

	render() {
		let icon = <Icon name="User"/>;
		let isProblem = this.props.account.accountProblems.length > 0;
		if (isProblem) {
			let firstProblem = this.props.account.accountProblems[0];
			icon = (
				<Tooltip trigger={ this.state.tooltipTrigger } pos="bottom center" render={ () => (
					<div style={ { width: '250px' } }>
						{ firstProblem.description }
					</div>
				) } onCloseClick={ this.closeTooltip }>
                    <span onMouseOver={ this.openTooltip }>
                        <Icon name="Warning" color="#f77"/>
                    </span>
				</Tooltip>
			)
		}

		return (<div className={ styles["header__profile-link"] }>
			<Link to="/Account/Manage">
                <span className={ styles["icon"] }>
                    { icon }
                </span>
				<span className={ styles["username"] }>
                    { this.props.account.visibleName || 'Профиль' }
                </span>
			</Link>
		</div>)
	}

	static propTypes = {
		account: accountPropTypes
	}
}

class Separator extends Component {
	render() {
		return <div className={ styles["header__separator"] }/>
	}
}

class LogoutLink extends Component {
	constructor(props) {
		super(props);
		this.onClick = this.onClick.bind(this);
	}

	onClick(e) {
		this.props.logout();
		e.preventDefault();
	}

	render() {
		return (
			<div className={ styles["header__logout-link"] }>
				<button className={ styles["header__logout-button"] } onClick={ this.onClick }>
					Выйти
				</button>
			</div>)
	}

	static mapStateToProps(state) {
		return {};
	}

	static mapDispatchToProps(dispatch) {
		return {
			logout: () => dispatch(api.account.logout())
		};
	}
}

LogoutLink = connect(LogoutLink.mapStateToProps, LogoutLink.mapDispatchToProps)(LogoutLink);

class RegistrationLink extends Component {
	render() {
		return (
			<div className={ styles["header__registration-link"] }>
				<Link to={ "/Account/Register?returnUrl=" + (this.props.returnUrl || "/") }>Зарегистрироваться</Link>
			</div>
		)
	}

	static propTypes = {
		returnUrl: PropTypes.string
	}
}

class LoginLink extends Component {
	render() {
		return (
			<div className={ styles["header__login-link"] }>
				<Link to={ "/Login?returnUrl=" + (this.props.returnUrl || "/") }>Войти</Link>
			</div>
		)
	}

	static propTypes = {
		returnUrl: PropTypes.string
	}
}