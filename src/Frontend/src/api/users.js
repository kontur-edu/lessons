import api from "../api/"

export function getCourseInstructors(courseId, query, count) {
	return api.get('users?course_id=' + courseId + "&course_role=Instructor&query=" + query + (count === undefined ? "" : "&count=" + count));
}