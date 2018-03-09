$(document).ready(function () {
	initCodeEditor();

	$('.exercise__add-review').each(function () {
		var $self = $(this);
		var addReviewUrl = $self.data('url');

		$self.find('.exercise__close-review').click(function(e) {
			$self.hide();
			$self.closest('.exercise').find('.code')[0].codeMirrorEditor.focus();
			e.preventDefault();
		});
		$self.find('.exercise__add-review__comment').on('input', function () {
			if ($(this).val() === '')
				$self.find('.exercise__add-review__button').attr('disabled', 'disabled');
			else
				$self.find('.exercise__add-review__button').removeAttr('disabled');
		});

		$self.find('.exercise__add-review__button').click(function () {
			var $button = $(this);
			var comment = $self.find('.exercise__add-review__comment').val();
			var params = {
				StartLine: editorLastRange.anchor.line,
				StartPosition: editorLastRange.anchor.ch,
				FinishLine: editorLastRange.head.line,
				FinishPosition: editorLastRange.head.ch,
				Comment: comment,
			}
			$button.text('Сохранение..').attr('disabled', 'disabled');
			$.post(addReviewUrl, params, function (renderedReview) {
				addExerciseCodeReview(renderedReview);
				$self.find('.exercise__add-review__comment').val('');
				$self.hide();
				$button.text('Сохранить (Ctrl+Enter)').removeAttr('disabled');
			});
		});

		/* Trigger button clock on Ctrl + Enter */
		$self.find('.exercise__add-review__comment').keydown(function(e) {
			if (e.ctrlKey && e.keyCode === 13) {
				$self.find('.exercise__add-review__button').trigger('click');
			}
		});
	});

	$('.exercise__submission').on('click', '.show-output-button', function () {
		$('#exercise__submission__output').slideToggle();
		$(this).toggleClass('active');
	});

	if ($('.exercise__reviews').length > 0)
		placeCodeReviews();
});

var editorLastRange, currentReviewTextMarker, reviewsTextMarkers, exerciseCodeDoc, $exerciseCodeBlock;

function initCodeEditor($parent) {
	if (!$parent)
		$parent = $(document);
	editorLastRange = null;
	currentReviewTextMarker = null;
	reviewsTextMarkers = {};
	exerciseCodeDoc = null;

	if ($('.code-review').length > 0)
		$exerciseCodeBlock = $('.code-review')[0];
	else
		$exerciseCodeBlock = $('.code-reviewed')[0];

	CodeMirror.commands.autocomplete = function (cm) {
		cm.showHint({ hint: CodeMirror.hint.csharp });
	};

	codeMirrorClass($parent.find('.code-exercise'), true, false, false);
	codeMirrorClass($parent.find('.code-sample'), false, false, false);    
	codeMirrorClass($parent.find('.code-guest'), false, true, false);
	codeMirrorClass($parent.find('.code-review'), false, false, true);
	codeMirrorClass($parent.find('.code-reviewed'), false, false, false);
    codeMirrorClass($parent.find('.code-antiplagiarism'), false, false, false);

	if ($exerciseCodeBlock) {
		var exerciseCodeEditor = $exerciseCodeBlock.codeMirrorEditor;
		exerciseCodeDoc = exerciseCodeEditor.getDoc();
	}

	function getMode(lang) {
		// see http://codemirror.net/mode/

		var langIds = {
			"cs": "x-csharp",
			"py": "x-python"
		};

		lang = lang || "cs";
		return "text/" + langIds[lang];
	}

	function unselectAllReviews(removeClassSelected) {
		if (removeClassSelected || "undefined" === typeof(removeClassSelected))
			$('.exercise__review.selected').removeClass('selected');
		if (currentReviewTextMarker) {
			currentReviewTextMarker.clear();
			currentReviewTextMarker = null;
		}
	}

	function selectReview($review) {
		var startLine = $review.data('start-line');
		var startPosition = $review.data('start-position');
		var finishLine = $review.data('finish-line');
		var finishPosition = $review.data('finish-position');

        unselectAllReviews(! $review.hasClass('selected'));
        if (! $review.hasClass('selected'))             
            $review.addClass('selected');
        
        
		currentReviewTextMarker = exerciseCodeDoc.markText({ line: startLine, ch: startPosition }, { line: finishLine, ch: finishPosition }, {
			className: 'exercise__code__reviewed-fragment__selected'
		});
	}
	
	function codeMirrorClass($objects, editable, guest, review) {
		$objects.each(function () {
			var element = this;
			var $el = $(element);
			var langId = $el.data("lang");
			$el.parent().find('.loading-spinner').hide();
			
			let theme = 'default';
			if (editable || guest)
				theme = 'cobalt';
			if ($el.closest('.revealed').length > 0)
				theme = 'pastel-on-dark';
			
			var editor = CodeMirror.fromTextArea(element,
			{
				mode: getMode(langId),
				lineNumbers: true,
				theme: theme, 
				indentWithTabs: true,
				tabSize: 4,
				indentUnit: 4,
				lineWrapping: true,
				extraKeys: {
					"Ctrl-Space": "autocomplete",
					".": function(cm) {
						setTimeout(function() { cm.execCommand("autocomplete"); }, 100);
						return CodeMirror.Pass;
					}
				},
				readOnly: !editable,
				//autoCloseBrackets: true, // workaround: autoCloseBracket breakes indentation after for|while|...
				styleActiveLine: editable,
				matchBrackets: true,
                styleSelectedText: true,
			});

			if (review) {
				var $addReviewPopup = $('.exercise__add-review').first();
				var $addReviewPopupInput = $addReviewPopup.find('.exercise__add-review__comment');
				
				/* Hack: focus review input just on blur */
				$addReviewPopupInput.blur(function () {
					if ($addReviewPopupInput.is(':visible')) {
						$addReviewPopupInput.focus();
					}
				});
				editor.on("beforeSelectionChange",
					function (cm, params) {
						unselectAllReviews();

						if (params.ranges < 1)
							return;

						var range = params.ranges[0];
						editorLastRange = range;
						var maxLine = Math.max(range.anchor.line, range.head.line);
						var coords = cm.cursorCoords({ line: maxLine + 1, ch: 1 }, 'page');

						if (range.anchor == range.head) {
							$addReviewPopup.hide();
							return;
						}
						$addReviewPopup.show();
						$addReviewPopup.offset({ top: coords.top, left: coords.left });

						$addReviewPopupInput.trigger('input');
						$addReviewPopupInput.focus();
					});
			}

			editor.on('cursorActivity',
				function(cm) {
					var cursor = cm.getDoc().getCursor();
					unselectAllReviews();
					var $foundReview = false;
					$('.exercise__reviews .exercise__review')
						.each(function() {
							if ($foundReview !== false)
								return;
							var $review = $(this);
							var startLine = $review.data('start-line');
							var startPosition = $review.data('start-position');
							var finishLine = $review.data('finish-line');
							var finishPosition = $review.data('finish-position');
							if (startLine > cursor.line || (startLine === cursor.line && startPosition > cursor.ch))
								return;
							if (finishLine < cursor.line || (finishLine === cursor.line && finishPosition < cursor.ch))
								return;
							$foundReview = $review;
						});
					if ($foundReview !== false)
						selectReview($foundReview);
				});

			element.codeMirrorEditor = editor;
			if (editable)
				editor.focus();
			if (guest)
				editor.on("mousedown", function(cm) { loginForContinue(); });
		});
	}
	
	let collapseCommentForm = function ($textarea, unselectCurrentReview) {
        let $replyForm = $textarea.closest('.exercise__review__reply-form');

        let $input = $replyForm.find('input[type="text"]');
        let $footer = $replyForm.find('.exercise__review__reply-footer');
        $textarea.hide();
        $footer.hide();
        $input.show();

        if (unselectCurrentReview)
        	unselectAllReviews();
    };

    let $exerciseReviews = $('.exercise__reviews');
    
    $exerciseReviews.on('click', '.exercise__review', function () {
		if (!$exerciseCodeBlock)
			return;

		var $review = $(this).closest('.exercise__review');
		selectReview($review);
	});

	$exerciseReviews.on('click', '.exercise__delete-review', function (e) {
		e.preventDefault();

		var $self = $(this);
		var $review = $self.closest('.exercise__review');
		var token = $('input[name="__RequestVerificationToken"]').val();
		var reviewId = $self.data('id');
		var url = $self.data('url');
		$.post(url, {
			__RequestVerificationToken: token
		}, function (data) {
			if (data.status !== 'ok') {
				console.log(data);
				alert('Can\'t delete comment: error');
			} else {
				$review.remove();
				placeCodeReviews();

				reviewsTextMarkers[reviewId].clear();
				unselectAllReviews();
			}
		}, 'json');

		return false;
	});

	$('.exercise__reviews .exercise__review').each(function () {
		var $review = $(this);
		createMarkTextForReview($review);
	});
	
	$exerciseReviews.on('focus', '.exercise__review__reply-form input[type="text"]', function (e) {		
		let $self = $(this);
		let $replyForm = $self.closest('.exercise__review__reply-form');
		$self.hide();
		
		let $textarea = $replyForm.find('textarea');
		let $footer = $replyForm.find('.exercise__review__reply-footer');
		$textarea.show().focus();
		$footer.show();

        let $button = $replyForm.find('.exercise__review__reply-button button');
		$button.attr('disabled', 'disabled');
		
		let $review = $self.closest('.exercise__review');
        selectReview($review);

        placeCodeReviews();
    });

    $exerciseReviews.on('keyup', '.exercise__review__reply-form textarea', function (e) {
        let $self = $(this);
        let $replyForm = $self.closest('.exercise__review__reply-form');
        let $button = $replyForm.find('.exercise__review__reply-button button');
        
        if ($self.val() === '')
        	$button.attr('disabled', 'disabled');
        else {
            $button.removeAttr('disabled');
            
            /* Send comment by Ctrl+Enter (or Cmd+Enter on Mac OS) */
            if ((e.ctrlKey || e.metaKey) && (e.keyCode === 13 || e.keyCode === 10)) {
                $button.click();
            }
        }

        placeCodeReviews();
    });
	
    $exerciseReviews.on('blur', '.exercise__review__reply-form textarea', function (e) {
        let $self = $(this);
               
        if ($self.val() !== '')
        	return;
        
        collapseCommentForm($self, true);
        placeCodeReviews();
    });

    /* autoEnlargeTextarea is exported from slide-comments.js */
    $exerciseReviews.on('input', '.exercise__review__reply-form textarea', autoEnlargeTextarea);

    $exerciseReviews.on('click', '.exercise__review__reply-button button', function (e) {
    	e.preventDefault();
    	
		let $self = $(this);
        let $replyForm = $self.closest('.exercise__review__reply-form');
        let $textarea = $replyForm.find('textarea');
        
        let reviewId = $self.data('id');
        let url = $self.data('url');
        let token = $('input[name="__RequestVerificationToken"]').val();
        $.post(url, {
            __RequestVerificationToken: token,
			reviewId: reviewId,
			text: $textarea.val(),
        }, function (data) {
         	let $review = $self.closest('.exercise__review');
         	let $place = $review.find('.exercise__review__new-comment-place');
         	$(data).insertBefore($place);
         	$textarea.val('');
         	/* Return height of textarea*/
         	$textarea.css('height', $textarea.data('minHeight') + 'px').removeData('minHeight');
         	collapseCommentForm($textarea, false);
            placeCodeReviews();
        });
    });
    
    $exerciseReviews.on('click', '.exercise__delete-review-comment', function (e) {
		e.preventDefault();
		e.stopPropagation();
		
		let $self = $(this);
		let $comment = $self.closest('.exercise__review-comment');

        let url = $self.data('url');
        let token = $('input[name="__RequestVerificationToken"]').val();
        $.post(url, {
            __RequestVerificationToken: token,
        }, function (data) {
        	if (data.status === 'ok') {
                $comment.hide();
                placeCodeReviews();
            } else {
        		console.log(data);
        		alert('Не могу удалить комментарий: произошла ошибка');
			}
        });
    });    
}

function createMarkTextForReview($review) {
	var reviewId = $review.data('id');
	var startLine = $review.data('start-line');
	var startPosition = $review.data('start-position');
	var finishLine = $review.data('finish-line');
	var finishPosition = $review.data('finish-position');

	reviewsTextMarkers[reviewId] = exerciseCodeDoc.markText({ line: startLine, ch: startPosition }, { line: finishLine, ch: finishPosition }, {
		className: 'exercise__code__reviewed-fragment'
	});
}

function placeCodeReviews() {
    let $reviews = $('.exercise__reviews .exercise__review');    
    let $exerciseReviews = $('.exercise__reviews');
    
    if ($reviews.length === 0 || $exerciseReviews.length === 0)
        return;
    
    let startHeight = $exerciseReviews.offset().top;
    let lastReviewBottomHeight = 0;
    $reviews.each(function() {
        let $review = $(this);
        let startLine = $review.data('startLine');
        let startPosition = $review.data('startPosition');
        let minHeight = exerciseCodeDoc.cm.charCoords({line: startLine, ch: startPosition}, 'local').top;
        let offset = Math.max(5, minHeight - lastReviewBottomHeight);
        $review.css('marginTop', offset + 'px');

		lastReviewBottomHeight = $review.offset().top + $review.outerHeight() - startHeight;
	});

    /* Make codemirror window and reviews panel equal by heights */
	let exerciseBlockHeight = $(exerciseCodeDoc.getEditor().display.wrapper).outerHeight();
	$exerciseReviews.css('minHeight', exerciseBlockHeight + 'px');
	let $codeMirrorSizer = $(exerciseCodeDoc.getEditor().display.sizer);
	let currentSizerHeight = parseInt($codeMirrorSizer.css('minHeight'));
	let exerciseReviewsHeight = $exerciseReviews.outerHeight();
	$codeMirrorSizer.css('height', Math.max(currentSizerHeight, exerciseReviewsHeight) + 'px');

	/* New reviews can be added so we need to call .tooltip() again */
    $exerciseReviews.find('[data-toggle="tooltip"]').tooltip();
}

function orderByStartLineAndPosition(first, second) {
	var firstStartLine = $(first).data('startLine');
	var firstStartPosition = $(first).data('startPosition');
	var secondStartLine = $(second).data('startLine');
	var secondStartPosition = $(second).data('startPosition');
	if (firstStartLine !== secondStartLine)
		return firstStartLine < secondStartLine ? -1 : 1;
	return firstStartPosition < secondStartPosition ? -1 : 1;
}

function addExerciseCodeReview(renderedReview) {
	var $reviewsBlock = $('.exercise__reviews');
	var $newReview = $(renderedReview);
	createMarkTextForReview($newReview);
	$reviewsBlock.append($newReview);

	var $reviews = $reviewsBlock.find('.exercise__review').get();
	$reviews.sort(orderByStartLineAndPosition);
	$.each($reviews, function (idx, $item) { $reviewsBlock.append($item); });

	placeCodeReviews();
}

var refreshPreviousDraftLastId = undefined;
function refreshPreviousDraft(id) {
	if (id == undefined)
		id = refreshPreviousDraftLastId;
	refreshPreviousDraftLastId = id;

	window.onbeforeunload = function () {
		saveExerciseCodeDraft(id);
	}
	if (localStorage[id] != undefined && $('.code-exercise').length > 0) {
		$('.code-exercise')[0].codeMirrorEditor.setValue(localStorage[id]);
	}
}

function saveExerciseCodeDraft(id) {
	if (id == undefined)
		id = refreshPreviousDraftLastId;

	if ($('.code-exercise').length > 0)
		localStorage[id] = $('.code-exercise')[0].codeMirrorEditor.getValue();
}