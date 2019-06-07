window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	$('.flexslider').flexslider({
		animation: "slide",
		slideshow: false
	});
});