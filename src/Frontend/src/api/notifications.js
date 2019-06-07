import api from "../api"

export function getNotificationsCount(lastTimestamp) {
	return dispatch => {
		return api.get("notifications/count?last_timestamp=" + (lastTimestamp ? lastTimestamp : ""))
		.then(json => {
			dispatch({
				type: 'NOTIFICATIONS__COUNT_UPDATED',
				count: json.count,
				lastTimestamp: json.lastTimestamp,
			})
		})
	}
}