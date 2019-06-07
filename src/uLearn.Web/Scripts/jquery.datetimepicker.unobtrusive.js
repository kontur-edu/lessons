window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	$('.datetimepicker').each(function() {
		var $self = $(this);
		var format = $self.data('date-format') || 'd.m.Y';
		var timepicker = $self.data('time-picker') || false;
		var lang = $self.data('lang') || 'ru';
		var mark = $self.data('mask') || true;
		$self.datetimepicker({
			format: format,
			timepicker: timepicker,
			lang: lang,
			mark: mark,
		});
	});
});