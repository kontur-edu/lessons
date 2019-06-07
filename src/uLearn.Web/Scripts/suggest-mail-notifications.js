window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
    var $modal = $('#suggest-mail-transport__modal');
	if ($modal.length)
		$modal.modal();
});