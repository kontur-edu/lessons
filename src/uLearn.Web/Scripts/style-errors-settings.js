window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    $('.style-errors__settings input[type="checkbox"]').change(function () {
        var $self = $(this);
        var isChecked = $self.is(':checked');
        var url = $self.data('changeUrl').replace('IS_ENABLED', isChecked.toString());
        $.post(url, 'json').done(function(data) {
            if (data.status === "ok") {
                var $status = $self.parent().find('.style-errors__settings__status');
                $status.addClass('text-success').text('Сохранено!');
                setTimeout(function () {
                    $status.removeClass('text-success').text('');
                },
                1000);
            }
        });
    });
});