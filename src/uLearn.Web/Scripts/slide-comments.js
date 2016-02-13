(function($){
    var expandReplyForm = function () {
        var $textarea = $('<textarea>').attr('name', $(this).attr('name'))
                                       .attr('placeholder', $(this).attr('placeholder'));
        var $button = $('<button>Отправить</button>').addClass('reply-form__send-button btn btn-primary');

        $textarea.blur(function () {
            if ($(this).val() === '') {
                var $textInput = $('<input type="text">').attr('name', $(this).attr('name'))
                                                         .attr('placeholder', $(this).attr('placeholder'));
                $textInput.click(expandReplyForm);

                $(this).replaceWith($textInput);
                $button.remove();
            }
        });

        $(this).replaceWith($textarea);
        $textarea.focus().after($button);
    }

    $('.reply-form input[name=comment-text]').click(expandReplyForm);
})(jQuery);