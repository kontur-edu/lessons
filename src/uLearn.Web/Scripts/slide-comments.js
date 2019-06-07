/* Set this function as handler for `input` event on textarea */
function autoEnlargeTextarea() {
    var $this = $(this);
    // Save current height as min height in first time
    if (!$this.data('min-height'))
        $this.data('min-height', parseInt($this.css('height')));
    // By default, max textarea's height is 400px, after it will be scrollable
    var maxHeight = $this.data('max-height') ? $this.data('max-height') : 400;

    var $clone = $this.clone().css('visibility', 'hidden');
    $this.after($clone);
    $clone.css('height', 'auto');
    var newHeight = Math.max($clone[0].scrollHeight + 5, $this.data('min-height'));
    $clone.remove();
    newHeight = Math.min(newHeight, maxHeight);
    $this.css('height', newHeight + 'px');
}

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

	var addAntiForgeryToken = function (data) {
		var token = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
        if (typeof(data) === "string") {
            return data + "&__RequestVerificationToken=" + token;
        } else {
            data.__RequestVerificationToken = token;
            return data;
        }
	};

	var $commentsRules;

	String.prototype.br2nl = function () {
		return this.replace(/<br\s*\/?>/gi, "\n");
	}

	String.prototype.nl2br = function () {
		return this.replace(/\n/g, "<br>");
	}

	String.prototype.encodeMultiLineText = function () {
		return $.encodeHtmlEntities(this).nl2br();
	}

	String.prototype.decodeMultiLineText = function () {
		return $.decodeHtmlEntities(this.br2nl());
	}

	jQuery.encodeHtmlEntities = function (text) {
		return $('<span>').text(text).html();
	}

	jQuery.decodeHtmlEntities = function (html) {
		return $('<span>').html(html).text();
	}

	var likeComment = function () {
		var $self = $(this);
		var url = $self.data('url');
		var $counter = $self.find('.comment__likes-count__counter');
		$.ajax({
			type: 'post',
			url: url,
			data: addAntiForgeryToken({}),
			dataType: 'json'
		}).done(function (data) {
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

	var showCommentsRulesIfNeeded = function ($textarea) {
		/* Don't show comments rules if they are already shown or under instructors-only comments */
		if ($('.comments__rules:visible').length === 0 && $textarea.closest('.comments-for-instructors-only').length === 0)
			$textarea.after($commentsRules.clone());
	}

	var onTextareaFocus = function (e) {
		var $textarea = $(this);
		showCommentsRulesIfNeeded($textarea);
	}

	var hideCommentsRules = function(e) {
		var $textarea = $(this);
		var $parent = $textarea.parent();
		var $localCommentsRules = $parent.find('.comments__rules');
		if ($textarea.val() === '')
			$localCommentsRules.remove();
	}

	var expandReplyForm = function (e) {
		e.preventDefault();
		var $textarea = $('<textarea>').attr('name', $(this).attr('name'))
			.attr('placeholder', $(this).attr('placeholder'));
		var $button = $('<button>Отправить</button>').addClass('reply-form__send-button btn btn-primary')
			.attr('disabled', true);

		$(this).replaceWith($textarea);
		$textarea.after($button).focus();
		scrollTo($textarea, 200);
	}

	var disableButtonForEmptyComment = function ($textarea) {
		var $button = $textarea.closest('.reply-form').find('button');
		$button.attr('disabled', $textarea.val().trim() === '');
	}

	var onTextareaKeyUp = function (e) {
		var $textarea = $(this);
		disableButtonForEmptyComment($textarea);

		/* Send comment by Ctrl+Enter (or Cmd+Enter on Mac OS) */
		if ((e.ctrlKey || e.metaKey) && (e.keyCode === 13 || e.keyCode === 10)) {
			var $button = $textarea.closest('.reply-form').find('button');
			$button.click();
		}
	}

	var sendComment = function (e) {
		e.preventDefault();

		var $form = $(this).closest('form');
		var $formWrapper = $form.closest('.reply-form');
		var $textarea = $form.find('[name=commentText]');
		var $button = $form.find('.reply-form__send-button');
		$button.attr('disabled', 'disabled');

		$.ajax({
			type: 'post',
			url: $form.attr('action'),
			data: addAntiForgeryToken($form.serialize())
		}).done(function (data) {
			if (data.status && data.status !== 'ok') {
				$button.removeAttr('disabled');
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
		var $self = $(this);
		var $replyForm = $(this).closest('.reply-form');
		var $button = $replyForm.find('button');
		if ($self.val() === '') {
			var $textInput = $('<input type="text">').attr('name', $self.attr('name'))
													 .attr('placeholder', $self.attr('placeholder'));

			$self.replaceWith($textInput);
			$button.remove();
			if ($replyForm.hasClass('collapse'))
				$replyForm.hide();
		}
	};

	var approveComment = function(e) {
		e.preventDefault();

		var $self = $(this);
		var url = $self.data('url');
		var $comment = $self.closest('.comment');
		var isApproved = ! $comment.is('.not-approved');

		$.ajax({
			type: 'post',
			url: url,
			data: addAntiForgeryToken({
				isApproved: ! isApproved,
			})
		}).done(function () {
			$comment.toggleClass('not-approved', isApproved);
		});
	};

	var deleteComment = function (e) {
		e.preventDefault();

		var $self = $(this);
		var url = $self.data('url');
		var restoreUrl = $self.data('restore-url');
		var $comment = $self.closest('.comment');

		$.ajax({
			type: 'post',
			url: url,
			data: addAntiForgeryToken({})
		}).done(function() {
			var $commentReplacement = $('<div class="comment__removed">Комментарий удалён. <a href="">Восстановить</a></div>')
				.attr('class', $comment.attr('class'));
			$commentReplacement.find('a').click(function(e) {
				e.preventDefault();
				$.ajax({
					type: 'post',
					url: restoreUrl,
					data: addAntiForgeryToken({})
				}).done(function() {
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
		var $comment = $self.closest('.comment');
		var isPinned = $comment.is('.is-pinned');

		$.ajax({
			type: 'post',
			url: url,
			data: addAntiForgeryToken({
				isPinned: ! isPinned,
			})
		}).done(function () {
			$comment.toggleClass('is-pinned', ! isPinned);
		});
	};

	var markCommentAsCorrect = function (e) {
		e.preventDefault();

		var $self = $(this);
		var url = $self.data('url');
		var $comment = $self.closest('.comment');
		var isCorrect = $comment.is('.is-correct-answer');

		$.ajax({
			type: 'post',
			url: url,
			data: addAntiForgeryToken({
				isCorrect: ! isCorrect,
			})
		}).done(function () {
			$comment.toggleClass('is-correct-answer', ! isCorrect);
		});
	};

	var editComment = function(e) {
		e.preventDefault();
		var $self = $(this);
		var url = $self.data('url');
		var $comment = $self.closest('.comment');
		var $commentText = $comment.find('.comment__text');
		var originalText = $commentText.data('originalText');
		var $commentFooter = $comment.find('.comment__footer');
		var $editTextarea = $('<textarea class="comment__new-text" name="commentText"></textarea>').val(originalText);
		var $saveButton = $('<button class="btn btn-primary">Сохранить</button>');
		var $cancelButton = $('<button class="btn btn-link">Отмена</button>');

		$commentText.hide();
		$commentFooter.hide();

		$commentText.after($editTextarea);
		$editTextarea.after($saveButton).trigger('input').focus();
		$saveButton.after($cancelButton);

		$saveButton.click(function () {
			var newText = $editTextarea.val();
			$.ajax({
				type: 'post',
				url: url,
				data: addAntiForgeryToken({
					newText: newText,
				})
			}).done(function (renderedCommentText) {
				$commentText.replaceWith($(renderedCommentText));
				$commentText.data('originalText', newText);
				// Remove form, restore comment view (see below)
				$cancelButton.trigger('click');
			});
		});

		$cancelButton.click(function() {
			$commentText.show();
			$commentFooter.show();

			/* .val('').blur() for hiding comment rules */
			$editTextarea.val('').blur().remove();
			$saveButton.remove();
			$cancelButton.remove();
		});
	}

	var scrollToCommentFromHash = function() {
		var hash = window.location.hash;
		var match;
		if ((match = /^#comment-(\d+)$/.exec(hash)) !== null && $('.comment[data-comment-id=' + match[1] + ']').length > 0) {
			var $comment = $('.comment[data-comment-id=' + match[1] + ']');
            scrollTo($comment);
        }
	};

    window.documentReadyFunctions = window.documentReadyFunctions || [];
	
    window.documentReadyFunctions.push(function () {
        var $comments = $('.comments');
		$commentsRules = $('.comments__rules');

        /* Set up handlers */
        $comments.on('click', '.reply-form input[name=commentText]', expandReplyForm);
        $comments.on('click', '.comment .comment__likes-count', likeComment);
        $comments.on('keyup', 'textarea[name=commentText]', onTextareaKeyUp);
        $comments.on('blur', '.comment textarea[name=commentText]', hideCommentsRules);
        $comments.on('blur', '.reply-form textarea[name=commentText]', hideCommentsRules);
        $comments.on('blur', '.reply-form.is-reply textarea[name=commentText]', collapseReplyForm);
        $comments.on('click', '.reply-form .reply-form__send-button', sendComment);
        $comments.on('click', '.comment .comment__inline-reply', createReplyForm);
        $comments.on('click', '.comment .comment__not-approved.label-switcher', approveComment);
        $comments.on('click', '.comment .comment__hide-link', approveComment);
        $comments.on('click', '.comment .comment__edit-link', editComment);
        $comments.on('click', '.comment .comment__delete-link', deleteComment);
        $comments.on('click', '.comment .comment__pinned.label-switcher', pinOrUnpinComment);
        $comments.on('click', '.comment .comment__correct-answer.label-switcher', markCommentAsCorrect);
        $comments.on('input', 'textarea[name=commentText]', autoEnlargeTextarea);
        $comments.on('focus', 'textarea[name=commentText]', onTextareaFocus);

        scrollToCommentFromHash();        
    });
})(jQuery);