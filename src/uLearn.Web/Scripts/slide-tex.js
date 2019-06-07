window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    $(".tex").each(function (index, element) {
        var latex = $(element).text();
        if (latex) {
            element.title = latex;
            katex.render(latex, element);
        }
    });
});