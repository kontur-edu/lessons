function submitQuiz(courseId, slideId, expectedCount, isLti) {
	if (areAllAnswered(expectedCount)) {
		/* Disable quiz submit button */
		$('.quiz-submit-btn').prop('disabled', true);

		var answers = [];
		$(".quiz").each(function () {
			var id = $(this).find('input, textarea').attr('id'); //id of quiz
			var content = id.split('quizBlock');
			var val;
			if ($(this).hasClass('quiz-block-input')) {
				val = $(this).find('input, textarea').val();
				answers.push(new QuizAnswer("Text", content[0], "", val));
			}
			if ($(this).hasClass('quiz-block-ordering__item')) {
				val = $(this).data('item-id');
				answers.push(new QuizAnswer("Ordering", content[0], val, ""));
			}
			if ($(this).hasClass('quiz-block-matching__item')) {
				var orderId = $(this).find('.quiz-block-matching__droppable').data('item-id');
				val = $(this).find('.quiz-block-matching__droppable').data('movable-item-id');
				answers.push(new QuizAnswer("Matching", content[0], orderId, val));
			}
			if ($('#' + id).is(':checked')) {
				var type = content[1] == "True" || content[1] == "False" ? "TrueFalse" : "Checkbox";
				answers.push(new QuizAnswer(type, content[0], content[1], ""));
			}
		});
		var answer = JSON.stringify(answers);
		$.ajax(
			{
				type: "POST",
				url: $("#SubmitQuiz").data("url"),
				data: {
					courseId: courseId,
					slideId: slideId,
					answer: answer,
					isLti: isLti
				}
			}).done(function (ans) {
				$("#quiz-status").text("Отправляем...");
				window.scrollTo(0, 0);
				if (ans.url) 
					window.location = ans.url;
				else
					window.location.reload();	
			})
			.fail(function (req) {
				console.log(req.responseText);
			})
			.always(function (ans) {
			});
		console.log(answers);
		return answer;
	} else
		alert("Выполните все задания перед отправкой теста");
};


function areAllAnswered(needingCount) {
	var realCount = 0;
	$(".quiz-block-mark").each(function () {
		if ($(this).children().children().is(':checked')) {
			realCount++;
		}
	});
	$(".quiz-block-input").each(function () {
		if ($(this).find('input, textarea').val() != "") {
			realCount++;
		}
	});
	realCount += $('.quiz-block-ordering').length;
	$(".quiz-block-matching").each(function () {
		var unfilled = $.grep($(this).find('.quiz-block-matching__droppable'), function(el) {
			return $(el).data('movable-item-id') === undefined;
		});
		if (unfilled.length === 0)
			realCount++;
		
	});
	return realCount >= needingCount;
}


function QuizAnswer(type, blockId, itemId, text) {
	this.BlockType = type;
	this.BlockId = blockId;
	this.ItemId = itemId;
	this.Text = text;
}

window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	$.fn.moveUp = function() {
		$.each(this, function() {
			$(this).after($(this).prev());
		});
	};

	$.fn.moveDown = function() {
		$.each(this, function() {
			$(this).before($(this).next());
		});
	};

	$(".quiz-block-ordering:not(.not-movable) ul").sortable();

	$(".quiz-block-ordering .quiz-block-ordering__item .quiz-block-ordering__item__icons").click(function(e) {
		var $target = $(e.target);
		var $item = $target.closest("li");
		if ($target.is(".glyphicon-arrow-down"))
			$item.moveDown();
		if ($target.is(".glyphicon-arrow-up"))
			$item.moveUp();
	});


	$('.quiz-block-matching:not(.not-movable) .quiz-block-matching__movable-item').draggable({
		revert: "invalid",
	}).click(function (e) {
		e.stopPropagation();
	
        var $self = $(this);
        var itemId = $self.data('itemId');
        var $container = $self.closest('.quiz-block-matching').find('.added:data(movable-item-id)').filter(
        	function(){
        		return $(this).data('movableItemId') === itemId;
        	}
		);

        /* Cancel old clicks */
        $self.closest('.quiz-block-matching').find('.clicked').removeClass('clicked');
        $self.closest('.quiz-block-matching').find('.clicked-item').removeClass('clicked-item');

		$container.toggleClass('clicked');
		$self.toggleClass('clicked-item');
    });

	$('.quiz-block-matching:not(.not-movable) .quiz-block-matching__droppable')
		.add('.quiz-block-matching:not(.not-movable) .quiz-block-matching__source__droppable').each(function () {
		var $this = $(this);
		var blockId = $(this).data('blockId');
		
		$this.droppable({
			activeClass: 'active',
			hoverClass: 'hover',
			accept: function($el) {
				return $el.data('blockId') === blockId &&
					(!$(this).hasClass('added') || $(this).data('movableItemId') === $el.data('itemId'));
			},
			drop: function (event, ui) {
				if ($(this).hasClass('added'))
					return false;
				$(this).addClass('added');
				var $dropped = $(ui.draggable);
				var $droppedOn = $(this);

				$dropped.offset($droppedOn.offset());
				$droppedOn.data('movableItemId', $dropped.data('itemId'));
			},
			out: function (event, ui) {
				var $dropped = $(ui.draggable);
				if ($(this).data('movableItemId') === $dropped.data('itemId')) {
					$(this).removeClass('added');
					$(this).removeData('movableItemId');
				}
			}
		});
	}).click(function () {
		var $self = $(this);
        
		if ($self.hasClass('added'))
			return;
		
        var $clicked = $self.closest('.quiz-block-matching').find('.clicked');
        if ($clicked.length > 0)
        {
			var itemId = $clicked.data('movableItemId');
            var $draggable = $self.closest('.quiz-block-matching').find('.quiz-block-matching__movable-item').filter(function(){ return $(this).data('itemId') === itemId; });
            
            $draggable.offset($self.offset());
            $self.data('movableItemId', itemId);
            $self.addClass('added');

            $clicked.removeData('movableItemId');
            $clicked.removeClass('clicked');            
            $clicked.removeClass('added');
            $draggable.removeClass('clicked-item');
        }
    });

	/* Auto-height by maximum height in matching block */
	$('.quiz-block-matching').each(function(idx, block) {
		var $block = $(block);
		var $allItems = $block.find('.quiz-block-matching__fixed-item').
			add($block.find('.quiz-block-matching__movable-item')).
			add($block.find('.quiz-block-matching__droppable')).
			add($block.find('.quiz-block-matching__source__droppable'));
		var heights = $allItems.map(function (_, el) { return $(el).outerHeight(); }).get();
		var maxHeight = Math.max.apply(null, heights);

		$allItems.outerHeight(maxHeight);
	});
	
	/* Remove ?send=1 from query-string */
	if ($('.quiz__blocks').length > 0) {
        var urlObject = new URL(window.location.href);
        urlObject.searchParams.delete('send');
        window.history.replaceState({}, '', urlObject.href);
    }
});