(function ($) {
	var scrollTo = function ($element, topPadding, duration) {
		topPadding = topPadding || 100;
		duration = duration || 500;

		var newScrollTop = $element.offset().top - topPadding;
		if (Math.abs($('body').scrollTop() - newScrollTop) < 300)
			return;

		$('html, body').animate({
			scrollTop: newScrollTop
		}, duration);
	}
	
	String.prototype.br2nl = function() {
		return this.replace(/<br\s*\/?>/gi, "\n");
	}

	String.prototype.nl2br = function () {
		return this.replace(/\n/g, "<br>");
	}

	String.prototype.encodeMultiLineText = function() {
		return $.encodeHtmlEntities(this).nl2br();
	}

	String.prototype.decodeMultiLineText = function() {
		return $.decodeHtmlEntities(this.br2nl());
	}

	jQuery.encodeHtmlEntities = function(text) {
		return $('<span>').text(text).html();
	}

	jQuery.decodeHtmlEntities = function (html) {
		return $('<span>').html(html).text();
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
		$button.attr('disabled', $(this).val().trim() === '');
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
		}).success(function (data) {
			if (data.status && data.status !== 'ok') {
				var $button = $form.find('.reply-form__send-button');
				var $errorMessage = $('<div>').addClass('comment__error-message').text(data.message);
				$button.after($errorMessage);
				$errorMessage.delay(1500).fadeOut(1000);
				return;
			}
			var renderedComment = data;
			var $newComment = $(renderedComment);

			// Don't collapse reply form fully, write another reply right in it
			if ($formWrapper.hasClass('collapse'))
				$formWrapper.removeClass('collapse');
			// If it's first reply, remove 'answer' link from top-level comment
			if ($formWrapper.prev().is('.comment'))
				$formWrapper.prev().find('.comment__inline-reply').hide();

			// Insert new comment before reply form
			$formWrapper.before($newComment);
			// and remove comment's text from textarea, collapse it
			$textarea.val('').trigger('blur');
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
		var isApproved = ! $comment.is('.not-approved');

		$.ajax({
			type: 'post',
			url: url,
			data: {
				__RequestVerificationToken: token,
				isApproved: ! isApproved,
			}
		}).success(function () {
			$comment.toggleClass('not-approved', isApproved);
		});
	};

	var deleteComment = function (e) {
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
					$commentReplacement.remove();
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

	var editComment = function(e) {
		e.preventDefault();
		var $self = $(this);
		var url = $self.data('url');
		var token = $self.find('input[name="__RequestVerificationToken"]').val();
		var $comment = $self.closest('.comment');
		var $commentText = $comment.find('.comment__text');
		var $commentFooter = $comment.find('.comment__footer');
		var $editTextarea = $('<textarea class="comment__new-text" name="commentText"></textarea>').val($commentText.html().decodeMultiLineText());
		var $saveButton = $('<button class="btn btn-primary">Сохранить</button>');
		var $cancelButton = $('<button class="btn btn-link">Отмена</button>');

		$commentText.hide();
		$commentFooter.hide();

		$commentText.after($editTextarea);
		$editTextarea.after($saveButton);
		$saveButton.after($cancelButton);

		$saveButton.click(function () {
			var newText = $editTextarea.val();
			$.ajax({
				type: 'post',
				url: url,
				data: {
					__RequestVerificationToken: token,
					newText: newText,
				}
			}).success(function () {
				$commentText.html(newText.encodeMultiLineText());

				$cancelButton.click();
			});
		});

		$cancelButton.click(function() {
			$commentText.show();
			$commentFooter.show();

			$editTextarea.remove();
			$saveButton.remove();
			$cancelButton.remove();
		});
	}

	var scrollToCommentFromHash = function() {
		var hash = window.location.hash;
		var match;
		if ((match = /^#comment-(\d+)$/.exec(hash)) !== null)
			scrollTo($('.comment[data-comment-id=' + match[1] + ']')
				.animate({ backgroundColor: '#fdd' })
				.delay(500)
				.animate({ backgroundColor: 'initial' })
				);
	};

	var autoEnlargeTextarea = function () {
		var $this = $(this);
		// Save current height as min height in first time
		if (!$this.data('min-height'))
			$this.data('min-height', parseInt($this.css('height')));
		// By default, max textarea's height is 400px, after it will be scrollable
		var maxHeight = $this.data('max-height') ? $this.data('max-height') : 400;

		var $clone = $this.clone().css('visibility', 'hidden');
		$('body').append($clone);
		$clone.css('height', 'auto');
		var newHeight = Math.max($clone[0].scrollHeight + 5, $this.data('min-height'));
		$clone.remove();
		newHeight = Math.min(newHeight, maxHeight);
		$this.css('height', newHeight + 'px');
	}

	$('.comments').on('click', '.reply-form input[name=commentText]', expandReplyForm);
	$('.comments').on('click', '.comment .comment__likes-count', likeComment);
	$('.comments').on('keyup', 'textarea[name=commentText]', disableButtonForEmptyComment);
	$('.comments').on('blur', '.reply-form.is-reply textarea[name=commentText]', collapseReplyForm);
	$('.comments').on('click', '.reply-form .reply-form__send-button', sendComment);
	$('.comments').on('click', '.comment .comment__inline-reply', createReplyForm);
	$('.comments').on('click', '.comment .comment__not-approved.label-switcher', approveComment);
	$('.comments').on('click', '.comment .comment__hide-link', approveComment);
	$('.comments').on('click', '.comment .comment__edit-link', editComment);
	$('.comments').on('click', '.comment .comment__delete-link', deleteComment);
	$('.comments').on('click', '.comment .comment__pinned.label-switcher', pinOrUnpinComment);
	$('.comments').on('click', '.comment .comment__correct-answer.label-switcher', markCommentAsCorrect);
	$('.comments').on('input', '.reply-form textarea[name=commentText]', autoEnlargeTextarea);

	$(document).ready(function() {
		scrollToCommentFromHash();
	});
})(jQuery);