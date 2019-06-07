import api from "../api/"

// Groups
export function getCourseGroups(courseId) {
	return api.get("groups?course_id=" + courseId);
}

export function getCourseArchivedGroups(courseId) {
	return api.get("groups?course_id=" + courseId + "&archived=true")
}

// Group
export function getGroup(groupId) {
	return api.get("groups/" + groupId);
}

export function createGroup(courseId, name) {
	return api.post("groups?course_id=" + courseId,
		api.createRequestParams({name}));
}

export function copyGroup(groupId, destinationCourseId, makeMeOwner) {
	return api.post("groups/" + groupId + "/copy?destination_course_id="
		+ encodeURIComponent(destinationCourseId) + '&make_me_owner=' + makeMeOwner);
}

export function saveGroupSettings(groupId, groupSettings) {
	return api.patch("groups/" + groupId,
		api.createRequestParams(groupSettings));
}

export function deleteGroup(groupId) {
	return api.delete("groups/" + groupId);
}

export function changeGroupOwner(groupId, ownerId) {
	return api.put("groups/" + groupId + '/owner',
		api.createRequestParams({ownerId}));
}

// Scores
export function getGroupScores(groupId) {
	return api.get("groups/" + groupId + '/scores')
}

export function saveScoresSettings(groupId, scoresId) {
	return api.post("groups/" + groupId + '/scores',
		api.createRequestParams({'scores': scoresId}));
}

// Accesses
export function getGroupAccesses(groupId) {
	return api.get("groups/" + groupId + "/accesses");
}

export function addGroupAccesses(groupId, userId) {
	return api.post("groups/" + groupId + "/accesses/" + userId);
}

export function removeAccess(groupId, userId) {
	return api.delete("groups/" + groupId + "/accesses/" + userId);
}

// Students
export function getStudents(groupId) {
	return api.get("groups/" + groupId + "/students");
}

export function deleteStudents(groupId, studentIds) {
	return api.delete("groups/" + groupId + "/students",
		api.createRequestParams({studentIds}));
}

export function copyStudents(groupId, studentIds) {
	return api.post("groups/" + groupId + "/students",
		api.createRequestParams({studentIds}));
}
