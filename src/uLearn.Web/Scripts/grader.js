window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	var $modal = $('.grader-submissions__details__modal');

	$('.grader-submissions__details__link').click(function () {
		var $self = $(this);
		var $loadingIcon = $self.parent().find('.loading-icon');
		$loadingIcon.css('visibility', 'visible');

		$.get($self.data('url')).done(function (data) {
			$modal.find('.modal-body').html(data);
			$modal.modal('show');
		}).always(function() {
			$loadingIcon.css('visibility', 'hidden');
		});
	});
});