(function ($) {
	var likeComment = function () {
		var $self = $(this);
		var url = $self.data('url');
		var token = $self.find('input[name="__RequestVerificationToken"]').val();
		var $counter = $self.find('.comment__likes-count__counter');
		$.ajax({
			type: 'post',
			url: url,
			data: { __RequestVerificationToken: token },
			dataType: 'json'
		}).success(function (data) {
			$self.toggleClass('is-liked', data.liked);
			$counter.text(data.likesCount > 0 ? data.likesCount : '');
		});
	}

	var expandReplyForm = function() {
		var $textarea = $('<textarea>').attr('name', $(this).attr('name'))
									   .attr('placeholder', $(this).attr('placeholder'));
		var $button = $('<button>Отправить</button>').addClass('reply-form__send-button btn btn-primary')
													 .attr('disabled', true);
		var $form = $(this).closest('form');
		var $formWrapper = $form.closest('.reply-form');

		$textarea.keyup(function() {
			$button.attr('disabled', $textarea.val() === '');
		});

		$textarea.blur(function() {
			if ($(this).val() === '') {
				var $textInput = $('<input type="text">').attr('name', $(this).attr('name'))
														 .attr('placeholder', $(this).attr('placeholder'));
				$textInput.click(expandReplyForm);

				$(this).replaceWith($textInput);
				$button.remove();
			}
		});

		$button.click(function(e) {
			e.preventDefault();

			$.ajax(
			{
				type: 'post',
				url: $form.attr('action'),
				data: $form.serialize(),
				dataType: 'json'
			}).success(function(data) {
				if (data.status === 'ok') {
					var $newComment = $(data.rendered);
					$newComment.find('.comment__likes-count').click(likeComment);
					$formWrapper.before($newComment);
					$textarea.val('').blur();
				}
			});
		});

		$(this).replaceWith($textarea);
		$textarea.focus().after($button);
	}

	$('.reply-form input[name=commentText]').click(expandReplyForm);
	$('.comments .comment .comment__likes-count').click(likeComment);
})(jQuery);