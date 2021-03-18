interface UsersProgressResponse {
	userProgress: { [userId: string]: UserProgress }
}

interface UserProgress {
	visitedSlides: { [slideId: string]: SlideUserProgress },
	additionalScores: { [slideId: string]: { [scoringGroupId: string]: number } }
}

interface SlideUserProgress {
	score: number,
	usedAttempts: number,
	waitingForManualChecking: boolean,
	prohibitFurtherManualChecking: boolean,
	visited: boolean,
	isSkipped: boolean,
	timestamp: string,
}

export { UsersProgressResponse, UserProgress, SlideUserProgress };
