window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	new Clipboard('.clipboard-link').on('success', function (e) {
		var $trigger = $(e.trigger);
		if ($trigger.data('show-copied')) {
			var oldValue = $trigger.text();
			$trigger.text('скопировано!');
			setTimeout(function() {
				$trigger.text(oldValue);
				},
				1000);
		}
	});
});