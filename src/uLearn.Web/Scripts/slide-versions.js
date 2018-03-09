function updateExerciseVersionUrl(versionId) {
	var newSearch = $.query.set('version', versionId).toString();
	var newurl = window.location.protocol + "//" + window.location.host + window.location.pathname + newSearch;
	if (history.pushState)
		window.history.pushState({ path: newurl }, '', newurl);
}

function setExerciseVersion(versionId, showOutput,) {
	showOutput = showOutput || false;
	var url = $('.exercise__submission').data('version-update-url');
	url = url.replace('VERSION_ID', versionId);
	url = url.replace('SHOW_OUTPUT', showOutput);

	saveExerciseCodeDraft();

	var $hints = $('#hints-accordion');
	$('.exercise__submission > *:not(.exercise__submissions-panel)').hide();
	$('.exercise__add-review').hide();
	var $loadingPanel = $('<p class="exercise-loading">Загрузка...</p>');
	$('.exercise__submission').append($loadingPanel);
	$hints.hide();

	/* Disabled version switching */
	$('.exercise-version-link').attr('disabled', 'disabled');

	$.get(url, function(data) {
		var $submission = $('.exercise__submission');
		$loadingPanel.hide();
		$submission.html($(data).html());

		initCodeEditor($submission);
		$submission.find('.select-auto-width').each(function() {
			selectSetAutoWidth($(this));
		});
		refreshPreviousDraft();

		updateExerciseVersionUrl(versionId);
		$hints.show();

		$('.exercise-version-link').removeAttr('disabled');

		/* Disable scoring form if need */
		var $scoreForm = $('.exercise__score-form');
		if ($scoreForm.length > 0) {
			var submissionId = parseInt($scoreForm.data('submissionId'));
			$scoreForm.toggle(submissionId === versionId);
		}

        /* placeCodeReviews is available on slide-editor.js */
		placeCodeReviews();

        /* Fetching antiplagiarism status (fetchAntiPlagiarismStatus() is available on antiplagiarism.js) */
        $('.antiplagiarism-status').each(function () {
            fetchAntiPlagiarismStatus($(this));
        });
    });
}

function setSimpleResult($block, details) {
	$block.find(".run-details").text(details);
	$block.show();
}

$(document).ready(function () {
	$('.exercise__submission').on('click', '.exercise-version-link', function (e) {
		e.preventDefault();

		var $self = $(this);
		var versionId = $self.data('version-id');
		setExerciseVersion(versionId);
	});

	$('.exercise__submission').on('change', '[name=version]', function() {
		var $self = $(this);
		setExerciseVersion(parseInt($self.val()));
	});
});