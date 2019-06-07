window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    $('.modal').on('shown.bs.modal', function () {
        $(this).find('.focus-on-show').focus();
    })
});