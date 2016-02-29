(function ($) {
	var scrollTo = function ($element, topPadding, duration) {
		topPadding = topPadding || 100;
		duration = duration || 500;

		var newScrollTop = $element.offset().top - topPadding;
		if (Math.abs($('body').scrollTop() - newScrollTop) < 300)
			return false;

		$('html, body').animate({
			scrollTop: newScrollTop
		}, duration);
	}

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
		scrollTo($replyForm.find('[name=commentText]'));
	}

	var expandReplyForm = function (e) {
		e.preventDefault();
		var $textarea = $('<textarea>').attr('name', $(this).attr('name'))
			.attr('placeholder', $(this).attr('placeholder'));
		var $button = $('<button>Отправить</button>').addClass('reply-form__send-button btn btn-primary')
			.attr('disabled', true);

		$(this).replaceWith($textarea);
		$textarea.focus().after($button);
		scrollTo($textarea, 200);
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

	var approveComment = function(e) {
		e.preventDefault();

		var $self = $(this);
		var url = $self.data('url');
		var token = $self.find('input[name="__RequestVerificationToken"]').val();
		var $comment = $self.closest('.comment');
		var $label = $comment.find('.comment__not-approved');
		
		$.ajax({
			type: 'post',
			url: url,
			data: { __RequestVerificationToken: token }
		}).success(function () {
			//$self.hide();
			$comment.removeClass('not-approved');
			$label.removeClass('label-default')
				.addClass('label-success')
				.text('опубликовано');
			setTimeout(function() {
				$label.fadeOut(300);
			}, 1000);
		});
	};

	var removeComment = function (e) {
		e.preventDefault();

		var $self = $(this);
		var url = $self.data('url');
		var restoreUrl = $self.data('restore-url');
		var token = $self.find('input[name="__RequestVerificationToken"]').val();
		var $comment = $self.closest('.comment');

		$.ajax({
			type: 'post',
			url: url,
			data: { __RequestVerificationToken: token }
		}).success(function() {
			var $commentReplacement = $('<div class="comment__removed">Комментарий удалён. <a href="">Восстановить</a></div>')
				.attr('class', $comment.attr('class'));
			$commentReplacement.find('a').click(function(e) {
				e.preventDefault();
				$.ajax({
					type: 'post',
					url: restoreUrl,
					data: { __RequestVerificationToken: token }
				}).success(function() {
					$commentReplacement.hide();
					$comment.show();
				});
			});
			$comment.hide().after($commentReplacement);
		});
	};

	var pinOrUnpinComment = function (e) {
		e.preventDefault();

		var $self = $(this);
		var url = $self.data('url');
		var token = $self.find('input[name="__RequestVerificationToken"]').val();
		var $comment = $self.closest('.comment');
		var isPinned = $comment.is('.is-pinned');

		$.ajax({
			type: 'post',
			url: url,
			data: {
				__RequestVerificationToken: token,
				isPinned: ! isPinned,
			}
		}).success(function () {
			$comment.toggleClass('is-pinned', ! isPinned);
		});
	};

	var markCommentAsCorrect = function (e) {
		e.preventDefault();

		var $self = $(this);
		var url = $self.data('url');
		var token = $self.find('input[name="__RequestVerificationToken"]').val();
		var $comment = $self.closest('.comment');
		var isCorrect = $comment.is('.is-correct-answer');

		$.ajax({
			type: 'post',
			url: url,
			data: {
				__RequestVerificationToken: token,
				isCorrect: ! isCorrect,
			}
		}).success(function () {
			$comment.toggleClass('is-correct-answer', ! isCorrect);
		});
	};

	$('.comments').on('click', '.reply-form input[name=commentText]', expandReplyForm);
	$('.comments').on('click', '.comment .comment__likes-count', likeComment);
	$('.comments').on('keyup', '.reply-form textarea[name=commentText]', disableButtonForEmptyComment);
	$('.comments').on('blur', '.reply-form.is-reply textarea[name=commentText]', collapseReplyForm);
	$('.comments').on('click', '.reply-form .reply-form__send-button', sendComment);
	$('.comments').on('click', '.comment .comment__inline-reply', createReplyForm);
	$('.comments').on('click', '.comment .comment__not-approved.label-switcher', approveComment);
	$('.comments').on('click', '.comment .comment__remove-link', removeComment);
	$('.comments').on('click', '.comment .comment__pinned.label-switcher', pinOrUnpinComment);
	$('.comments').on('click', '.comment .comment__correct-answer.label-switcher', markCommentAsCorrect);
})(jQuery);