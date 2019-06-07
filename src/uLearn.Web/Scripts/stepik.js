window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	/* Course export */

	$('.stepik__course-export__select-course li').click(function() {
		var $self = $(this);

		var $ul = $('.stepik__course-export__select-course');
		$ul.find('li').not(this).animate({ opacity: 0.3 });
		$self.animate({ opacity: 1 }).addClass('selected');

		$('[name="stepikCourseId"]').val($self.data('courseId'));
		$('.stepik__course-export__select-course__buttons').show();
	});

	$('.stepik__course-export__select-course__buttons button').click(function (e) {
		e.preventDefault();

		var $self = $(this);
		var $form = $self.closest('form');

		var url = $self.data('url');
		$form.attr('action', url);
		$form.submit();
	});

	$('.stepik__course-export__lessons-partitions .strike-line').click(function(e) {
		e.preventDefault();

		var $self = $(this);
		$self.toggleClass('selected');
	});

	$('.stepik__course-export__initial__buttons button').click(function(e) {
		e.preventDefault();

		var $self = $(this);
		var $form = $self.closest('form');

		var $selected = $('.stepik__course-export__lessons-partitions .strike-line.selected');
		var selectedSlidesIds = $selected.map(function() { return $(this).data('slideId'); }).get().join(',');

		$form.find('[name="newLessonsSlidesIds"]').val(selectedSlidesIds);
		$form.submit();
	});

	/* Course updating */

	$('.stepik__course-update__slide').click(function(e) {
		if ($(e.target).closest('.checkbox').length > 0)
			return;

		e.preventDefault();
		var $row = $(this).closest('.stepik__course-update__slide');
		var $checkbox = $row.find('.checkbox input');
		$checkbox.click();
	});

	$('.stepik__course-update__slide .checkbox input[type="checkbox"]').click(function() {
		var $checkbox = $(this);
		var isChecked = $checkbox.is(':checked');
		var $row = $checkbox.closest('.stepik__course-update__slide');
		$row.toggleClass('selected', isChecked);
	});

	$('.stepik__course-update__buttons button').click(function (e) {
		e.preventDefault();

		var $self = $(this);
		var $form = $self.closest('form');

		var $selected = $('.stepik__course-update__slide.selected');
		var selectedSlidesIds = $selected.map(function () { return $(this).data('slideId'); }).get().join(',');

		$form.find('[name="updateSlidesIds"]').val(selectedSlidesIds);
		$form.submit();
	});
});