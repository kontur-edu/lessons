window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    $('.btn').button();
    $('.btn-hover').hover(
        function () {
            $(this).addClass($(this).data("hover-class"));
        },
        function () {
            $(this).removeClass($(this).data("hover-class"));
        }
    );

    $('input[type=file]').bootstrapFileInput();
    $('.file-inputs').bootstrapFileInput();
});