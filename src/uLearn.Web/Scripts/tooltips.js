window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	$('[data-toggle="tooltip"]').tooltip();
});