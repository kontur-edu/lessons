window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    $('.selectpicker').selectpicker();
	
	$('.selectpicker').on('rendered.bs.select', function() {
		$(this).removeClass('form-control');
	});

	$('.selectpicker').on('changed.bs.select', function(e, index, newValue, oldValue) {
		var $self = $(this);

		var $selectedOption = $($self.find('option')[index]);
		if ($selectedOption.data('exclusive') && newValue) {
			$self.selectpicker('deselectAll');
			$self.val($selectedOption.val());
			$self.selectpicker('render');
			/* Close menu */
			$self.trigger('click');
		};

		if (!$selectedOption.data('exclusive') && newValue) {
			if ($self.find('option[data-exclusive="true"]:selected').length) {
				$self.selectpicker('deselectAll');
				$self.val($selectedOption.val());
				$self.selectpicker('render');
			}
		}
	});
});