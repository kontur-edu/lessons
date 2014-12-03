$('.tablesorter').tablesorter({
	widthFixed: true,
	widgets: ['zebra'],

	onRenderHeader: function () {
		$(this).find('span').addClass('headerSpan');
	},
	selectorHeaders: 'thead th',

	cssAsc: "headerSortUp",
	cssDesc: "headerSortDown",
	cssHeader: "header",
	tableClass: 'tablesorter',

	widgetOptions: {
		columns: ["primary", "secondary", "tertiary"],
	},

	textAttribute: 'data-sort-value'
});