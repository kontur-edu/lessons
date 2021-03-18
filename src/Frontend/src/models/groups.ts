import { ShortUserInfo } from "./users";

export interface GroupAsStudentInfo {
	id: number,
	courseId: string,
	name: string,
	isArchived: boolean,
	apiUrl: string,
}

export interface GroupAccessesResponse {
	accesses: GroupAccessesInfo[],
}

export interface GroupAccessesInfo {
	user: ShortUserInfo,
	accessType: GroupAccessType,
	grantedBy: ShortUserInfo,
	grantTime: string,
}

export enum GroupAccessType {
	FullAccess = 1,
	Owner = 100,
}

export interface GroupStudentsResponse {
	students: GroupStudentInfo[],
}

export interface GroupStudentInfo {
	user: ShortUserInfo,
	addingTime: string,
}

export interface GroupScoringGroupsResponse {
	scores: GroupScoringGroupInfo[],
}

export interface GroupScoringGroupInfo {
	areAdditionalScoresEnabledForAllGroups: boolean,
	canInstructorSetAdditionalScoreInSomeUnit: boolean,
	areAdditionalScoresEnabledInThisGroup?: boolean,
}

export interface GroupInfo {
	id: string,
	createTime?: string | null,
	name: string,
	isArchived: boolean,
	owner: ShortUserInfo,
	inviteHash: string,
	isInviteLinkEnabled: boolean,
	areYouStudent: boolean,
	isManualCheckingEnabled: boolean,
	isManualCheckingEnabledForOldSolutions: boolean,
	defaultProhibitFurtherReview: boolean,
	canStudentsSeeGroupProgress: boolean,
	studentsCount: number,
	accesses: GroupAccessesInfo[],
	apiUrl: string,
}

export interface CopyGroupResponse {
	id: string,
	apiUrl: string,
}
