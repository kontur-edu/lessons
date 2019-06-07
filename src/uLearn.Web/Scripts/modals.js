window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	$('.modal-header__tabs a').click(function(e) {
		e.preventDefault();

		var $self = $(this);
		var $li = $self.closest('li');
		if ($li.hasClass('active'))
			return;

		var $tabs = $li.closest('.modal-header__tabs');
		$tabs.find('li').removeClass('active');
		$li.addClass('active');

		var $target = $($self.data('target'));
		var $parent = $target.parent();
		$parent.find('> .tab-pane').removeClass('active');
		$target.addClass('active');
	});

	$('.modal').on('show.bs.modal', function() {
		$(this).find('.field-validation-error').text('');
	});

	$('.modal').on('shown.bs.modal', function () {
		$(this).find('[autofocus]').focus();
	});
});