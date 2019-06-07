window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	$('.tablesorter[data-autoload="true"]').each(function() {
		var $self = $(this);
		$self.tablesorter({
			widthFixed: true,
			widgets: ['zebra', 'stickyHeaders'],
			sortInitialOrder: $self.data('initial-order') || 'asc',

			onRenderHeader: function() {
				$(this).find('span').addClass('headerSpan');
			},
			selectorHeaders: 'thead th',

			cssAsc: "headerSortUp",
			cssDesc: "headerSortDown",
			cssHeader: "header",
			tableClass: 'tablesorter',

			widgetOptions: {
				columns: ["primary", "secondary", "tertiary"],
				stickyHeaders_offset: ".header",
			},

			textAttribute: 'data-sort-value'
		});
	});
});