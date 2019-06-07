window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	function initSpoilerBlock($spoilerBlock) {
		var $button = $spoilerBlock.find('.spoiler-block__button button');
		var $content = $spoilerBlock.find('.spoiler-block__content');

		$button.click(function(){
			$button.hide();
			$content.show();

			toggleQuizButton();
		})
	}

	function toggleQuizButton() {
		var $spoilerBlocks = $('.spoiler-block');
		var $quizButton = $('.quiz-submit-btn');

		var $blockedBlocks = $spoilerBlocks.filter('[data-hide-quiz-button="true"]');
		var shouldHideQuizButton = false;
		$blockedBlocks.each(function (){
			var $button = $(this).find('.spoiler-block__button button');
			if ($button.is(':visible')) {
				console.log($button);
				shouldHideQuizButton = true;
			}
		});
		$quizButton.toggle(!shouldHideQuizButton);
	}
	
	var $spoilerBlocks = $('.spoiler-block');
	toggleQuizButton();

	$spoilerBlocks.each(function () { initSpoilerBlock($(this)) });
});