window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    $('.popover-trigger').popover({
        html: true
    });
});