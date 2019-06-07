window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	$('.slide-diff-table').each(function () {
		var $self = $(this);
		$self.html(diffHtml($self.data('original'), $self.data('changed'), 'Старый блок', 'Новый блок', true));
	});

	$('.expand-slide-diff-link').click(function(e) {
		e.preventDefault();
	});
});