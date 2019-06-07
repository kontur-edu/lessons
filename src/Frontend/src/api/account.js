import api from "../api"

export function getCurrentUser() {
	return dispatch => {
		return api.get('account')
		.then(json => {
			let isAuthenticated = json.isAuthenticated;
			if (!isAuthenticated)
				dispatch({type: 'ACCOUNT__USER_INFO_UPDATED', isAuthenticated: false});
			else {
				let user = json.user;
				dispatch({
					type: 'ACCOUNT__USER_INFO_UPDATED',
					isAuthenticated: true,
					id: user.id,
					login: user.login,
					firstName: user.firstName,
					lastName: user.lastName,
					visibleName: user.visibleName,
					accountProblems: json.accountProblems,
					systemAccesses: json.systemAccesses,
				});
				dispatch(api.account.getRoles());
			}
		})
		.catch(error => {
			if (error.response && error.response.status === 401) { // Unauthorized
				dispatch({type: 'ACCOUNT__USER_INFO_UPDATED', isAuthenticated: false});
			}
		})
	}
}

export function getRoles() {
	return dispatch => {
		return api.get('account/roles')
		.then(json => {
			let courseRoles = json.courseRoles;
			let courseRolesObject = {};
			courseRoles.forEach(c => courseRolesObject[c.courseId.toLowerCase()] = c.role);

			let courseAccesses = json.courseAccesses;
			let courseAccessesObject = {};
			courseAccesses.forEach(c => courseAccessesObject[c.courseId.toLowerCase()] = c.accesses);

			dispatch({
				type: 'ACCOUNT__USER_ROLES_UPDATED',
				isSystemAdministrator: json.isSystemAdministrator,
				roleByCourse: courseRolesObject,
				accessesByCourse: courseAccessesObject
			})
		})
	}
}

export function logout() {
	return () => {
		return api.post('account/logout')
		.then(json => {
			if (json.logout) {
				localStorage.removeItem('exercise_solutions');
				api.clearApiJwtToken();
				redirectToMainPage();
			}
		});

		function redirectToMainPage() {
			let parser = document.createElement('a');
			parser.href = window.location.href;
			window.location.href = parser.protocol + "//" + parser.host;
		}
	}
}
