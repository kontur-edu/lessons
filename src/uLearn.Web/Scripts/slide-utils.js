function ShowPanel(e) {
	e.preventDefault();
	var button = $(e.target);
	button.siblings().removeClass('btn-primary');
	button.addClass('btn-primary');
	var divId = button.data('div-id');
	var div = $('#' + divId);
	div.siblings('div.statistic-container').css('display', 'none');
	if (div.text() == '')
		div.load(div.data('url'));
	div.css('display', 'block');
}

function selectSetAutoWidth($select) {
	var $fakeSelect = $select.clone().css('display', 'none').css('width', 'auto').insertAfter($select);
	$fakeSelect.find('option').remove();
	$fakeSelect.append('<option>');
	var currentText = $select.find('option:selected').text();
	$fakeSelect.find('option').text(currentText);
	var fakeSelectWidth = $fakeSelect.width();
	$fakeSelect.remove();
	$select.width(fakeSelectWidth * 1.03);
}

function setAutoUpdater($element) {
	var interval = $element.data('update-interval') || 1000;
	var url = $element.data('update-url') || console.log('Invalid data-update-url on ', $element);
	var timer = setInterval(function () {
		/* If element has been removed from DOM then stop */
		if ($element.parent().length === 0) {
			clearInterval(timer);
			return;
		}
		$.get(url, function (answer) {
			var $answer = $(answer);
			if ($answer.text() !== $element.text()) {
				$element.html($answer.html());
			}
		});
	}, interval);
}

window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	$('.js__onchange-send-form').change(function () {
		$(this).closest('form').submit();
	});

	$('.select-auto-width').each(function () {
		var $self = $(this);
		selectSetAutoWidth($self);
		$self.change(function() {
			selectSetAutoWidth($self);
		});
	});

	$('.js__auto-update').each(function() {
		setAutoUpdater($(this));
	});
});