var $runResults = $(".run-result");
var $serviceError = $runResults.filter(".run-service-error");
var $compileError = $runResults.filter(".run-compile-error");
var $styleError = $runResults.filter(".run-style-error");
var $waError = $runResults.filter(".run-wa");
var $waErrorNoDiff = $runResults.filter(".run-wa-no-diff");
var $success = $runResults.filter(".run-success");


function setSimpleResult($block, details) {
	$block.find(".run-details").text(details);
	$block.show();
}

function setWA(expected, actual) {
	var $difTable = $waError.find(".diff-table");
	//	var solutionsDiff = diffUsingJS(actual, expected);
	var solutionsDiff = diffHtml(actual, expected);
	$difTable.html(solutionsDiff);
	$waError.show();
}

function RunSolutionResult() {
	this.IsCompillerFailure = false;
	this.IsCompileError = false;
	this.IsStyleViolation = false;
	this.IsRightAnswer = false;
	this.ExpectedOutput = "";
	this.ActualOutput = "";
	this.CompilationError = "";
}

function setResults(ans) {
	///<param name="ans" type="RunSolutionResult"></param>
	if (ans.IsCompillerFailure) setSimpleResult($serviceError, ans.CompilationError);
	else if (ans.IsCompileError) setSimpleResult($compileError, ans.CompilationError);
	else if (ans.IsStyleViolation) setSimpleResult($styleError, ans.CompilationError);
	else if (ans.IsRightAnswer) {
		setSimpleResult($success, ans.ActualOutput);
		slideNavigation.makeShowSolutionsNext();
	}
	else if (ans.ExpectedOutput === null)
		setSimpleResult($waErrorNoDiff, ans.ActualOutput);
	else
		setWA(ans.ExpectedOutput, ans.ActualOutput);
}

var $runButton = $(".run-solution-button");

$runButton.click(function () {
	var code = $(".code-exercise")[0].codeMirrorEditor.getValue();
	if (code.length == 0)
		code = " ";
	$runButton
		.text("...running...")
		.addClass("active")
		.prop("disabled", true);
	$runResults.hide();

		$.ajax(
		{
			type: "POST",
			url: $runButton.data("url"),
			data: code
		}).success(setResults)
		.fail(function (req) {
			setSimpleResult($serviceError, req.status + " " + req.statusText);
			console.log(req.responseText);
		})
		.always(function () {
			$runButton
				.text("Run")
				.removeClass("active")
				.prop("disabled", false);
		});
});