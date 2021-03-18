import api from "src/api/index";
import { resetStudentsLimits } from "src/consts/routes";
import {
	CopyGroupResponse,
	GroupAccessesResponse,
	GroupInfo,
	GroupScoringGroupsResponse,
	GroupStudentsResponse
} from "src/models/groups";

// Groups
export function getCourseGroups(courseId: string): Promise<{ groups: GroupInfo[] }> {
	return api.get("groups?course_id=" + courseId);
}

export function getCourseArchivedGroups(courseId: string): Promise<{ groups: GroupInfo[] }> {
	return api.get("groups?course_id=" + courseId + "&archived=true");
}

// Group
export function getGroup(groupId: string): Promise<GroupInfo> {
	return api.get("groups/" + groupId);
}

export function createGroup(courseId: string, name: string): Promise<Response> {
	return api.post("groups?course_id=" + courseId,
		api.createRequestParams({ name }));
}

export function copyGroup(groupId: string, destinationCourseId: string,
	makeMeOwner: boolean
): Promise<CopyGroupResponse> {
	return api.post("groups/" + groupId + "/copy?destination_course_id="
		+ encodeURIComponent(destinationCourseId) + '&make_me_owner=' + makeMeOwner);
}

export function saveGroupSettings(groupId: string, groupSettings: Record<string, unknown>): Promise<GroupInfo> {
	return api.patch("groups/" + groupId,
		api.createRequestParams(groupSettings));
}

export function deleteGroup(groupId: string): Promise<Response> {
	return api.delete("groups/" + groupId);
}

export function changeGroupOwner(groupId: string, ownerId: string): Promise<Response> {
	return api.put("groups/" + groupId + '/owner',
		api.createRequestParams({ ownerId }));
}

// Scores
export function getGroupScores(groupId: string): Promise<GroupScoringGroupsResponse> {
	return api.get("groups/" + groupId + '/scores');
}

export function saveScoresSettings(groupId: string, checkedScoresSettingsIds: string[]): Promise<Response> {
	return api.post("groups/" + groupId + '/scores',
		api.createRequestParams({ 'scores': checkedScoresSettingsIds }));
}

// Accesses
export function getGroupAccesses(groupId: string): Promise<GroupAccessesResponse> {
	return api.get("groups/" + groupId + "/accesses");
}

export function addGroupAccesses(groupId: string, userId: string): Promise<Response> {
	return api.post("groups/" + groupId + "/accesses/" + userId);
}

export function removeAccess(groupId: string, userId: string): Promise<Response> {
	return api.delete("groups/" + groupId + "/accesses/" + userId);
}

// Students
export function getStudents(groupId: string): Promise<GroupStudentsResponse> {
	return api.get("groups/" + groupId + "/students");
}

export function deleteStudents(groupId: string, studentIds: string[]): Promise<Response> {
	return api.delete("groups/" + groupId + "/students",
		api.createRequestParams({ studentIds }));
}

export function copyStudents(groupId: string, studentIds: string[]): Promise<Response> {
	return api.post("groups/" + groupId + "/students",
		api.createRequestParams({ studentIds }));
}

export function resetLimitsForStudents(groupId: string, studentIds: string[]): Promise<Response> {
	return api.post("groups/" + groupId + resetStudentsLimits,
		api.createRequestParams({ studentIds }));
}
