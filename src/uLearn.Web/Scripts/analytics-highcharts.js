window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	var $chart = $('#usersByCountVisitedSlidesChart');

	function showChartByDataFrom(id) {
		var $element = $(id)[0];
		if (!$element)
			return;
		$chart.highcharts({
			data: {
				table: document.getElementById($element.id)
			},
			chart: {
				type: 'area',
				height: 200,
				width: 300,
			},
			title: {
				text: ''
			},
			legend: {
				enabled: false
			},
			xAxis: {
				type: 'int',
				allowDecimals: false,
				min: 0,
				title: {
					text: 'Просмотрено слайдов'
				}
			},
			yAxis: {
				allowDecimals: false,
				min: 0,
				title: {
					text: 'Количество человек'
				}
			},
			plotOptions: {
				area: {
					marker: {
						radius: 2
					},
					lineWidth: 1,
					states: {
						hover: {
							lineWidth: 1
						}
					},
					threshold: null
				}
			},
			credits: {
				enabled: false,
			},
		});		
	}

	showChartByDataFrom('#usersByCountVisitedSlides');

	$('.analytics__chart__toggle')
		.click(function(e) {
			e.preventDefault();

			var $target = $(e.target);

			$('.analytics__chart__toggle').removeClass('active');
			$target.addClass('active');
			var href = $target.attr('href');
			showChartByDataFrom(href);

			return false;
		});
});