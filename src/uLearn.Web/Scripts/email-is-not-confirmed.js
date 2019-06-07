window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	var $modal = $('#emailIsNotConfirmedModal');
	var cookieName = 'emailIsNotConfirmed';

	/* TODO (andgein): copy-paste with ulearn-update-invitation.js */
	var getCookie = function(name) {
		var matches = document.cookie.match(new RegExp(
		  "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
		));
		return matches ? decodeURIComponent(matches[1]) : undefined;
	}

	var setCookie = function (name, value) {
		var expires = new Date(2020, 1, 1);
		var updatedCookie = name + "=" + encodeURIComponent(value) + ";path=/;expires=" + expires.toUTCString();
		document.cookie = updatedCookie;
	};

	var showEmailIsNotConfirmedModal = function() {
		if (getCookie(cookieName) === undefined) {
			setCookie(cookieName, 'true');
			$modal.modal();
		}
	}

	if ($modal.length)
		showEmailIsNotConfirmedModal();
});