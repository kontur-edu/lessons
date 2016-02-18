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

	var createReplyForm = function (e) {
		e.preventDefault();
		var $replyForm = $(e.target).closest('.comment').next();
		$replyForm.fadeIn();
		$replyForm.find('[name=commentText]').click();
	}

	var expandReplyForm = function (e) {
		e.preventDefault();
		var $textarea = $('<textarea>').attr('name', $(this).attr('name'))
			.attr('placeholder', $(this).attr('placeholder'));
		var $button = $('<button>Отправить</button>').addClass('reply-form__send-button btn btn-primary')
			.attr('disabled', true);

		$(this).replaceWith($textarea);
		$textarea.focus().after($button);
	}

	var disableButtonForEmptyComment = function() {
		var $button = $(this).next();
		$button.attr('disabled', $(this).val() === '');
	}

	var sendComment = function (e) {
		e.preventDefault();

		var $form = $(this).closest('form');
		var $formWrapper = $form.closest('.reply-form');
		var $textarea = $form.find('[name=commentText]');

		$.ajax({
			type: 'post',
			url: $form.attr('action'),
			data: $form.serialize()
		}).success(function (renderedComment) {
			var $newComment = $(renderedComment);
			$formWrapper.before($newComment);
			$textarea.val('').blur();
		});
	}

	var collapseReplyForm = function () {
		var $button = $(this).next();
		if ($(this).val() === '') {
			var $replyForm = $(this).closest('.reply-form');
			var $textInput = $('<input type="text">').attr('name', $(this).attr('name'))
													 .attr('placeholder', $(this).attr('placeholder'));

			$(this).replaceWith($textInput);
			$button.remove();
			if ($replyForm.hasClass('collapse'))
				$replyForm.hide();
		}
	};

	$('.comments').on('click', '.reply-form input[name=commentText]', expandReplyForm);
	$('.comments').on('click', '.comment .comment__likes-count', likeComment);
	$('.comments').on('keyup', '.reply-form textarea[name=commentText]', disableButtonForEmptyComment);
	$('.comments').on('blur', '.reply-form.is-reply textarea[name=commentText]', collapseReplyForm);
	$('.comments').on('click', '.reply-form .reply-form__send-button', sendComment);
	$('.comments').on('click', '.comment .comment__inline-reply', createReplyForm);
})(jQuery);