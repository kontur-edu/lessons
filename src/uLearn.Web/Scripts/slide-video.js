function onYouTubeIframeAPIReady() {
	var $videoBlocks = $('.youtube-video');
	var rateCookieName = 'youtube-video-rate';

	var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(navigator.userAgent) ? true : false;
	$videoBlocks.each(function () {
		var $videoBlock = $(this);

		var width = $videoBlock.data('width') || '390';
		var height = $videoBlock.data('height') || '640';
		var videoId = $videoBlock.data('videoId');
		var autoplay = $videoBlock.data('autoplay') && !isMobile;
		var elementId = 'youtube-video__' + videoId;
		$videoBlock.attr('id', elementId);
		var frameLoaded = false;
		var playerReady = false;
		var initialized = false;
		var intervalId = setInterval(function () {
			if($("#" + elementId)[0].contentWindow != null) {
				if(initialized) {
					clearInterval(intervalId);
					return;
				}
				if(!playerReady) {
					return;
				}
				init(player);
				clearInterval(intervalId);
			}
		}, 10);
		var player = new YT.Player(elementId, {
			height: width,
			width: height,
			videoId: videoId,
			events: {
				'onReady': function (e) {
					playerReady = true;
					if(!frameLoaded) {
						return;
					}
					init(e.target);
				},
				'onPlaybackRateChange': function (e) {
					var newRate = e.data;
					Cookies.set(rateCookieName, newRate);
				}
			},
			playerVars: {
				/* Disable related videos */
				rel: 0,
			},
		});
		
		function init(p) {
			var rate = parseFloat(Cookies.get(rateCookieName) || '1');
			p.setPlaybackRate(rate);
			if (autoplay)
				p.playVideo();
			initialized = true;
		}
	});
}

window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	var enableYoutubeIframeApi = function() {
		window['YT'] = undefined;
		
		var tag = document.createElement('script');
		tag.src = "https://www.youtube.com/iframe_api";
		var firstScriptTag = document.getElementsByTagName('script')[0];
		firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
	};

	var $videoBlocks = $('.youtube-video');

	if ($videoBlocks.length > 0)
		enableYoutubeIframeApi();
});