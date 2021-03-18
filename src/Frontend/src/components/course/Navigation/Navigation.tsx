import React, { Component, CSSProperties } from "react";
import { connect } from "react-redux";
import { Dispatch } from "redux";

import NavigationContent from './Unit/NavigationContent';
import NextUnit from "./Unit/NextUnit";

import NavigationHeader from "./NavigationHeader";
import CourseNavigationContent from "./Course/CourseNavigationContent";
import Flashcards from "./Course/Flashcards/Flashcards";

import { flashcards } from "src/consts/routes";

import { toggleNavigationAction } from "src/actions/navigation";

import { CourseMenuItem, FlashcardsStatistics, MenuItem, Progress, UnitProgress } from "./types";

import { GroupAsStudentInfo } from "src/models/groups";
import { RootState } from "src/redux/reducers";
import { UnitInfo } from "src/models/course";
import { SlideType } from "src/models/slide";
import { DeviceType } from "src/consts/deviceType";

import styles from './Navigation.less';

const mobileNavigationMenuWidth = 250;//250 is @mobileNavigationMenuWidth, its mobile nav menu width

export type Props = PropsFromRedux & DefaultNavigationProps & CourseNavigationProps & UnitNavigationProps;

interface DefaultNavigationProps {
	courseTitle: string;
	navigationOpened: boolean;
}

interface PropsFromRedux {
	groupsAsStudent: GroupAsStudentInfo[];
	deviceType: DeviceType;
	toggleNavigation: () => void;
}

export interface CourseNavigationProps {
	courseId: string;
	slideId?: string;

	flashcardsStatistics: FlashcardsStatistics;
	containsFlashcards: boolean;

	courseProgress: Progress;
	courseItems: CourseMenuItem[];
}

export interface UnitNavigationProps {
	unitTitle?: string;

	unitProgress?: UnitProgress;

	unitItems?: MenuItem<SlideType>[];
	nextUnit?: UnitInfo | null;

	unitFlashcardsStatistic?: FlashcardsStatistics;

	onCourseClick: () => void;
}

interface State {
	overlayStyle?: CSSProperties;
	sideMenuStyle?: CSSProperties;
	xDown: null | number;
	yDown: null | number;
	touchListenerAdded: boolean,
	currentScrollState: 'top' | 'scroll' | 'bottom',
}

class Navigation extends Component<Props, State> {
	touchDistanceTolerance = 10;
	touchStartMaxX = 24;//is simmilar to left padding on slide
	wrapper: React.RefObject<HTMLDivElement> = React.createRef();
	currentActiveItem: React.RefObject<HTMLLIElement> = React.createRef();
	header: React.RefObject<HTMLDivElement> = React.createRef();
	animationDuration = 300;

	constructor(props: Props) {
		super(props);

		this.state = {
			xDown: null,
			yDown: null,
			touchListenerAdded: false,
			currentScrollState: 'top',
		};
	}

	componentDidMount() {
		document.addEventListener('resize', this.handleWindowSizeChange);
		this.tryAddTouchListener();

		this.tryScrollToActiveItem();

		if(this.wrapper.current) {
			const wrapper = this.wrapper.current;

			wrapper.addEventListener('scroll', () => {
				if(wrapper) {
					const newScrollState = wrapper.scrollTop === 0
						? 'top'
						: (wrapper.scrollTop + wrapper.clientHeight >= wrapper.scrollHeight)
							? 'bottom'
							: 'scroll';
					if(newScrollState !== this.state.currentScrollState) {
						this.setState({
							currentScrollState: newScrollState,
						});
					}
				}
			});
		}
	}

	tryScrollToActiveItem = () => {
		if(this.currentActiveItem.current && this.wrapper.current && this.header.current) {
			const active = this.currentActiveItem.current;
			const wrapper = this.wrapper.current;
			const header = this.header.current;

			const activeRect = active.getBoundingClientRect();
			const wrapperRect = wrapper.getBoundingClientRect();
			const headerRect = header.getBoundingClientRect();

			if(activeRect.top - wrapperRect.top > wrapperRect.height) {
				wrapper.scrollTo(
					{ left: 0, top: activeRect.top - wrapperRect.height + 50, behavior: "auto" });
			} else if(activeRect.top - wrapperRect.top <= headerRect.height) {
				wrapper.scrollTo(
					{ left: 0, top: activeRect.top - activeRect.height - headerRect.height, behavior: "auto" });
			}
		}
	};

	isMobileNavigationMenu = () => {
		const { deviceType } = this.props;
		return deviceType === DeviceType.mobile || deviceType === DeviceType.tablet;
	};

	tryAddTouchListener = () => {
		if(this.isMobileNavigationMenu() && !this.state.touchListenerAdded) {
			document.addEventListener('touchstart', this.handleTouchStart, { passive: true });
			this.setState({
				touchListenerAdded: true,
			});
		}
	};

	getTouches = (evt: TouchEvent) => {
		return evt.touches || evt.changedTouches;
	};

	handleTouchStart = (evt: TouchEvent) => {
		document.addEventListener('touchmove', this.handleTouchMove, { passive: true });
		document.addEventListener('touchend', this.handleTouchEnd, { passive: true });

		const { clientX, clientY, } = this.getTouches(evt)[0];
		const { navigationOpened } = this.props;

		if(!navigationOpened && clientX <= this.touchStartMaxX || navigationOpened) {
			this.setState({
				xDown: clientX,
				yDown: clientY,
			});
		}
	};

	handleTouchEnd = (evt: TouchEvent) => {
		const { clientX, } = evt.changedTouches[0];
		const { toggleNavigation, navigationOpened, } = this.props;
		const { xDown, yDown, overlayStyle, } = this.state;

		if(!xDown || !yDown) {
			return;
		}

		const moveDistance = Math.abs(xDown - clientX);
		const isDistanceEnough = moveDistance !== 0;
		if(isDistanceEnough) {
			const leftSwap = !navigationOpened && clientX > xDown;
			const rightSwap = navigationOpened && clientX < xDown;
			if(leftSwap || rightSwap) {
				toggleNavigation();
			}
		}

		if(overlayStyle) {
			this.playHidingOverlayAnimation();
		}

		this.setState({
			xDown: null,
			yDown: null,
			sideMenuStyle: undefined,
		});

		document.removeEventListener('touchmove', this.handleTouchMove);
		document.removeEventListener('touchend', this.handleTouchEnd);
	};

	handleTouchMove = (evt: TouchEvent) => {
		const { xDown, yDown, } = this.state;
		const { navigationOpened } = this.props;

		if(!xDown || !yDown) {
			return;
		}

		const { clientX, clientY, } = evt.touches[0];

		const xDiff = xDown - clientX;
		const yDiff = yDown - clientY;

		if(Math.abs(xDiff) > this.touchDistanceTolerance && Math.abs(xDiff) > Math.abs(yDiff)) {
			let diff, ratio;
			if(navigationOpened) {
				diff = -xDiff;
				ratio = 1 - Math.abs(Math.min(diff, 1) / mobileNavigationMenuWidth);
			} else {
				diff = -xDiff - mobileNavigationMenuWidth;
				ratio = 1 - Math.abs(Math.min(diff, 0) / mobileNavigationMenuWidth);
			}

			this.setState({
				overlayStyle: {
					visibility: 'visible',
					opacity: ratio,
					transition: 'unset',
				},
				sideMenuStyle: {
					transform: `translateX(${ Math.min(0, diff) }px)`,
					transition: 'unset',
				}
			});
		} else {
			this.setState({
				xDown: null,
				yDown: null,
				overlayStyle: undefined,
				sideMenuStyle: undefined,
			});
		}
	};

	handleWindowSizeChange = () => {
		this.tryAddTouchListener();
	};

	componentWillUnmount() {
		document.removeEventListener('resize', this.handleWindowSizeChange);
		if(this.state.touchListenerAdded) {
			document.removeEventListener('touchstart', this.handleTouchStart);
			document.removeEventListener('touchmove', this.handleTouchMove);
			document.removeEventListener('touchend', this.handleTouchEnd);
		}
	}

	componentDidUpdate(prevProps: Props) {
		const { navigationOpened, unitTitle } = this.props;

		if(this.isMobileNavigationMenu() && prevProps.navigationOpened !== navigationOpened) {
			document.querySelector('body')?.classList.toggle(styles.overflow, navigationOpened);
			if(!navigationOpened) {
				this.playHidingOverlayAnimation();
			}
		}
		if(unitTitle !== prevProps.unitTitle) {
			this.tryScrollToActiveItem();
		}
	}

	playHidingOverlayAnimation = () => {
		this.setState({
			overlayStyle: {
				visibility: 'visible',
			},
		});
		setTimeout(() => {
			this.setState({
				overlayStyle: undefined,
			});
		}, this.animationDuration);
	};

	hideNavigationMenu = () => {
		const { navigationOpened, toggleNavigation, } = this.props;

		if(navigationOpened) {
			toggleNavigation();
		}
	};

	render() {
		const { overlayStyle, sideMenuStyle, currentScrollState, } = this.state;
		const {
			courseTitle,
			groupsAsStudent,
			courseProgress,
			unitTitle,
			onCourseClick,
			unitProgress,
			deviceType,
		} = this.props;

		const isInsideCourse = unitTitle === undefined;

		return (
			<aside>
				<div className={ styles.overlay } style={ overlayStyle } onClick={ this.hideNavigationMenu }/>
				<div className={ styles.contentWrapper } style={ sideMenuStyle } ref={ this.wrapper }>
					<NavigationHeader
						className={ currentScrollState !== 'top' ? styles.shadow : undefined }
						title={ courseTitle }
						groupsAsStudent={ groupsAsStudent }
						courseProgress={ courseProgress }
						returnToCourseNavigationClicked={ onCourseClick }
						createRef={ this.header }
						unitProgress={ unitProgress }
						unitTitle={ unitTitle }
						isInsideCourse={ isInsideCourse }
						deviceType={ deviceType }
					/>
					{
						isInsideCourse
							? this.renderCourseNavigation()
							: this.renderUnitNavigation()
					}
				</div>
			</aside>
		);
	}

	renderUnitNavigation() {
		const {
			toggleNavigation,
			unitTitle,
			unitItems,
			nextUnit,
		} = this.props;

		if(!unitTitle || !unitItems) {
			return null;
		}

		return (
			<>
				<NavigationContent
					items={ unitItems }
					onClick={ toggleNavigation }
					getRefToActive={ this.currentActiveItem }
				/>
				{ nextUnit && <NextUnit unit={ nextUnit } onClick={ this.handleToggleNavigation }/> }
			</>
		);
	}

	handleToggleNavigation = () => {
		this.hideNavigationMenu();
	};

	renderCourseNavigation() {
		const {
			courseItems,
			containsFlashcards,
			courseId,
			slideId,
			toggleNavigation,
			flashcardsStatistics,
		} = this.props;

		return (
			<>
				{ courseItems && courseItems.length &&
				<CourseNavigationContent
					getRefToActive={ this.currentActiveItem }
					items={ courseItems }
				/> }
				{ containsFlashcards &&
				<Flashcards
					statistics={ flashcardsStatistics }
					toggleNavigation={ toggleNavigation }
					courseId={ courseId }
					isActive={ slideId === flashcards }
				/> }
			</>
		);
	}
}

const mapStateToProps = (state: RootState) => {
	const { currentCourseId, } = state.courses;
	const { deviceType, } = state.device;
	const courseId = currentCourseId
		? currentCourseId.toLowerCase()
		: null;
	const groupsAsStudent = state.account.groupsAsStudent;
	const courseGroupsAsStudent = groupsAsStudent
		? groupsAsStudent.filter(group => group.courseId.toLowerCase() === courseId && !group.isArchived)
		: [];

	return { groupsAsStudent: courseGroupsAsStudent, deviceType };
};

const mapDispatchToProps = (dispatch: Dispatch) => ({
	toggleNavigation: () => dispatch(toggleNavigationAction()),
});

const connected = connect(mapStateToProps, mapDispatchToProps)(Navigation);
export default connected;
