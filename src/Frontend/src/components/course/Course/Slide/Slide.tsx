import React from "react";

import { BlocksWrapper, Exercise, Image, Spoiler, StaticCode, Text, Video, } from "./Blocks";
import CourseLoader from "src/components/course/Course/CourseLoader";

import { loadSlide } from "src/actions/slides";
import { connect } from "react-redux";
import classNames from 'classnames';
import queryString from "query-string";

import {
	Block,
	BlockTypes,
	ExerciseBlock,
	ShortSlideInfo,
	SpoilerBlock,
	TexBlock,
	VideoBlock,
} from "src/models/slide";
import { RootState } from "src/models/reduxState";
import { MatchParams } from "src/models/router";
import { Dispatch } from "redux";

import styles from './Slide.less';

type BlockComponent = typeof Video | typeof StaticCode | typeof Exercise | typeof Text | typeof Image | typeof Spoiler;

const mapTypeToBlock
	: { [T in BlockTypes]: BlockComponent }
	= {
	[BlockTypes.video]: Video,
	[BlockTypes.code]: StaticCode,
	[BlockTypes.exercise]: Exercise,
	[BlockTypes.text]: Text,
	[BlockTypes.tex]: Text,
	[BlockTypes.image]: Image,
	[BlockTypes.spoiler]: Spoiler,
};

interface BlockToRender {
	Block: BlockComponent,
	fullSizeBlock: boolean,
	hide: boolean,
	props: Record<string, unknown>,
}

const fullSizeBlockTypes: { [T in BlockTypes]: boolean } = {
	[BlockTypes.video]: true,
	[BlockTypes.spoiler]: true,
	[BlockTypes.code]: false,
	[BlockTypes.exercise]: false,
	[BlockTypes.text]: false,
	[BlockTypes.tex]: false,
	[BlockTypes.image]: false,
};

interface Props {
	courseId: string,
	slideId: string,
	slideBlocks?: Block<BlockTypes>[],
	slideLoading: boolean,
	loadSlide: (courseId: string, slideId: string,) => void,
	showHiddenBlocks: boolean,
	slideInfo: ShortSlideInfo,
	isLti: boolean,
}

class Slide extends React.Component<Props> {
	static defaultProps: Partial<Props> = {
		showHiddenBlocks: true,
	};

	componentDidMount() {
		const { slideBlocks, } = this.props;
		if(!slideBlocks) {
			this.loadSlide();
		}
	}

	componentDidUpdate(prevProps: Props) {
		const { slideBlocks, slideLoading } = this.props;

		if(prevProps.slideId !== this.props.slideId || !slideBlocks && !slideLoading) {
			this.loadSlide();
		}
	}

	loadSlide = () => {
		const { loadSlide, courseId, slideId, } = this.props;
		loadSlide(courseId, slideId);
	};

	render = () => {
		const { slideBlocks, showHiddenBlocks, slideInfo, isLti, } = this.props;
		const isHiddenSlide = slideInfo.hide;

		if(!slideBlocks) {
			return (<CourseLoader/>);
		}

		const exerciseSlideBlock = slideBlocks.find(sb => sb.$type === BlockTypes.exercise);
		if(isLti && exerciseSlideBlock) {
			return this.renderSlideBlocks([JSON.parse(JSON.stringify(exerciseSlideBlock))]);
		}

		if(showHiddenBlocks) {
			return this.renderSlideBlocks(JSON.parse(JSON.stringify(slideBlocks)));
		}

		if(isHiddenSlide) {
			return this.renderHiddenSlide();
		}

		const slideBlocksForStudent = this.getSlidBlocksForStudents(slideBlocks);


		if(slideBlocksForStudent.length === 0) {
			return this.renderHiddenSlide();
		}

		return this.renderSlideBlocks(JSON.parse(JSON.stringify(slideBlocksForStudent)));
	};

	getSlidBlocksForStudents = (blocks: Block<BlockTypes>[]) => {
		const slideBlocksForStudent = [];

		for (const block of blocks) {
			if(block.hide) {
				continue;
			}
			if(block.$type === BlockTypes.spoiler) {
				this.filterSpoilerBlocksForStudents(block as SpoilerBlock);
			}

			slideBlocksForStudent.push(block);
		}

		return slideBlocksForStudent;
	};

	filterSpoilerBlocksForStudents = (spoiler: SpoilerBlock) => {
		spoiler.blocks = spoiler.blocks.filter(b => !b.hide);
		for (const insideSpoiler of spoiler.blocks.filter(b => b.$type === BlockTypes.spoiler)) {
			this.filterSpoilerBlocksForStudents(insideSpoiler as SpoilerBlock);
		}
	};

	renderSlideBlocks = (slideBlocks: Block<BlockTypes>[]) => {
		this.addAdditionalPropsToBlocks(slideBlocks);
		const blocksPacks = [];

		for (let i = 0; i < slideBlocks.length; i++) {
			const blocksPart = this.getBlocksPack(slideBlocks, i);

			i += blocksPart.blocks.length - 1;
			blocksPacks.push(blocksPart);
		}
		const onlyOneBlock = blocksPacks.length === 1;
		return blocksPacks.map(({ blocks, hide, fullSizeBlocksPack }, i) => {
			const renderedBlocks = blocks.map(this.mapBlockToComponent);
			return (
				fullSizeBlocksPack
					? renderedBlocks
					: <BlocksWrapper
						key={ i }
						isBlock={ !onlyOneBlock }
						hide={ hide }
					>
						{ renderedBlocks }
					</BlocksWrapper>
			);
		});
	};

	renderHiddenSlide = () => {
		return (
			<BlocksWrapper>
				<p>Студенты не увидят этот слайд в навигации</p>
			</BlocksWrapper>
		);
	};

	addAdditionalPropsToBlocks = (slideBlocks: Block<BlockTypes>[]) => {
		const { slideId, courseId, showHiddenBlocks, slideInfo, isLti, } = this.props;
		const { autoplay } = queryString.parse(window.location.search);
		const { maxScore } = slideInfo;

		for (const [i, block] of slideBlocks.entries()) {
			const type = block.$type;
			switch (type) {
				case BlockTypes.tex: {
					const texBlock = block as TexBlock;
					texBlock.content = this.getContentFromTexLines(texBlock);
					break;
				}
				case BlockTypes.spoiler: {
					const spoilerBlock = block as SpoilerBlock;

					const { slideId, } = this.props;

					spoilerBlock.blocksId = slideId; // make spoiler close content on slide change
					if(i !== 0) {
						spoilerBlock.isPreviousBlockHidden = slideBlocks[i - 1].hide || false;
					}
					if(spoilerBlock.hide) {
						const blocksInHiddenSpoiler = spoilerBlock.blocks.map(b => ({ ...b, hide: true }));
						spoilerBlock.renderedBlocks = this.renderSlideBlocks(
							JSON.parse(JSON.stringify(blocksInHiddenSpoiler)));
					} else {
						spoilerBlock.renderedBlocks = this.renderSlideBlocks(
							JSON.parse(JSON.stringify(spoilerBlock.blocks)));
					}
					break;
				}
				case BlockTypes.video: {
					const videoBlock = block as VideoBlock;

					if(i === 0) {
						if(autoplay) {
							videoBlock.autoplay = !!autoplay; //autoplay for first video on slide
						}

						if(slideBlocks.length === 1) {
							videoBlock.openAnnotation = true; // only video on slide => open annotation
						}
					}

					videoBlock.annotationWithoutBottomPaddings = !block.hide &&
						(i < slideBlocks.length - 1
							? slideBlocks[i + 1].$type !== BlockTypes.video
							: true);
					break;
				}
				case BlockTypes.code: {
					break;
				}
				case BlockTypes.exercise: {
					const exerciseBlock = block as ExerciseBlock;
					exerciseBlock.slideId = slideId;
					exerciseBlock.courseId = courseId;
					exerciseBlock.forceInitialCode = !showHiddenBlocks;
					exerciseBlock.maxScore = maxScore;
					exerciseBlock.isLti = isLti;
					break;
				}
			}
		}
	};

	getContentFromTexLines = ({ lines }: TexBlock) => {
		return lines.reduce((ac, cv) => ac + `<p class="tex">${ cv }</p>`, '');
	};

	getBlocksPack = (slideBlocks: Block<BlockTypes>[], i: number) => {
		const block = this.mapElementToBlock(slideBlocks[i]);

		const blocks = [block];
		const blocksPack = { blocks, hide: block.hide, fullSizeBlocksPack: block.fullSizeBlock };

		for (let k = i + 1; k < slideBlocks.length; k++) {
			const otherBlock = this.mapElementToBlock(slideBlocks[k]);
			if(otherBlock.fullSizeBlock === block.fullSizeBlock && otherBlock.hide === block.hide) {
				blocks.push(otherBlock);
			} else {
				break;
			}
		}
		return blocksPack;
	};

	mapElementToBlock = ({
		$type,
		hide = false,
		...props
	}: Block<BlockTypes>): BlockToRender => {
		return {
			Block: mapTypeToBlock[$type],
			fullSizeBlock: fullSizeBlockTypes[$type],
			hide,
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			props: { ...props }
		};
	};

	mapBlockToComponent = ({ Block, props, hide, }: BlockToRender, index: number,
		arr: BlockToRender[]
	) => {
		const className = classNames(
			{ [styles.firstChild]: index === 0 },
			{ [styles.lastChild]: index === arr.length - 1 }
		);
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		return <Block key={ index } className={ className } hide={ hide } { ...props }/>;
	};
}

const mapStateToProps = (state: RootState, { courseId, slideId, }: MatchParams) => {
	const { slides, } = state;
	const { slidesByCourses, slideLoading } = slides;

	const props: Pick<Props, 'courseId' | 'slideId' | 'slideLoading' | 'slideBlocks'> = {
		courseId,
		slideId,
		slideLoading,
		slideBlocks: undefined,
	};

	const coursesSlides = slidesByCourses[courseId];

	if(coursesSlides) {
		props.slideBlocks = coursesSlides[slideId];
	}

	return props;
};

const mapDispatchToProps = (dispatch: Dispatch) => ({
	loadSlide: (courseId: string, slideId: string) => loadSlide(courseId, slideId)(dispatch),
});


export default connect(mapStateToProps, mapDispatchToProps)(Slide);
