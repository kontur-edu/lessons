$(function() {
	$.fn.smoothScroll = function (callback, $link) {
		var $object = this;
		var needScroll =  $link.data('scroll') !== false;
		if ($object.length) {
			var scrollTo = function () {
				if (!needScroll)
					return;
				$('html').animate({
					scrollTop: $object.first().offset().top - $('.navbar').height()
				},
				{
					duration: 1000,
					complete: callback
				});
			};
			if ($link && $link.data('toggle')) {
				if (!$object.is(':visible')) {
					$object.data('originalClasses', $object.attr('class'));
					$object.removeClass('hidden').removeClass('hidden-xs').removeClass('hidden-xs-inline');
					$object.fadeIn(200, 'linear');
					setTimeout(scrollTo, 200);
				} else {
					$object.attr('class', $object.data('originalClasses'));
				}
			}
			else if ($object.is(':visible'))
				scrollTo();
			return true;
		} 
		return false;
	};

	$('a[href*="#"]:not([href="#"]):not(.no-smooth-scrolling)').click(function() {
		if (location.pathname.replace(/^\//,'') == this.pathname.replace(/^\//,'') && location.hostname == this.hostname) {
			var $target = $(this.hash);
			if (! $target.length) {
				$target = $('.' + this.hash.slice(1));
				if (! $target.length)
					$target = $('[name=' + this.hash.slice(1) + ']');
			}
			return ! $target.smoothScroll(null, $(this));
		}
	});
});