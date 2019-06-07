window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	var $modal = $('#notifications__transport-settings__modal');
	var $panel = $modal.find('.notifications__transport-settings__panel');
	var saveUrl = $modal.data('saveUrl');
	var disableTransportUrlTemplate = $('.notifications__disable-transport__button').attr('href');

	function getNotificationSettings(courseId, transportId, notificationType) {
		for (var idx in window.notificationTransportsSettings) {
			if (!window.notificationTransportsSettings.hasOwnProperty(idx))
				continue;

			var setting = window.notificationTransportsSettings[idx];
			if (setting.courseId === courseId &&
				setting.transportId === transportId &&
				setting.notificationType === notificationType)
				return setting.isEnabled;
		}
		return undefined;
	}

	$('.notifications__transport-settings__link').click(function () {
		var $self = $(this);
		var transportId = $self.data('transportId');
		var transportEnableSignature = $self.data('transportEnableSignature');
		var modalTitle = $self.data('modalTitle');

		var disableTransportUrl = disableTransportUrlTemplate.replace("TRANSPORT-ID", transportId)
			.replace("TRANSPORT-ENABLE-SIGNATURE", transportEnableSignature);
		$('.notifications__disable-transport__button').attr('href', disableTransportUrl);

		$panel.find('[name="transportId"]').val(transportId);
		$modal.find('.modal-title').text(modalTitle);

		$('.notifications__transport-settings__course__link').removeClass('active');
		$panel.removeClass('active');

		$modal.modal();
	});

	$('.notifications__transport-settings__link.auto-open').each(function () {
		$(this).click();
	});

	$('.notifications__transport-settings__course__link').click(function () {
		var $self = $(this);
		var courseId = $self.data('courseId');
		var courseTitle = $self.data('courseTitle');
		var transportId = parseInt($panel.find('[name="transportId"]').val());

		$panel.find('[name="courseId"]').val(courseId);
		$modal.find('.course-title').text(courseTitle);
		$('.notifications__transport-settings__course__link').removeClass('active');
		$self.addClass('active');
		$panel.addClass('active');

		// Clear all statuses
		$panel.find('.status').text('');

		var notificationTypesForCourse = window.notificationTypesByCourse[courseId];
		var $checkboxes = $panel.find('input[type="checkbox"]');
		$checkboxes.each(function () {
			var $checkbox = $(this);
			var $checkboxWrapper = $checkbox.closest('.checkbox');
			var notificationType = $checkbox.data('notificationType');
			$checkboxWrapper.toggle(notificationTypesForCourse.indexOf(notificationType) !== -1);

			var isEnabled = getNotificationSettings(courseId, transportId, notificationType);
			if (isEnabled === undefined)
				isEnabled = $checkbox.data('default');

			$checkbox.prop('checked', isEnabled);
		});
	});

	$panel.find('input[type="checkbox"]').change(function(e) {
		e.preventDefault();

		var $checkbox = $(this);
		var $status = $checkbox.closest('.checkbox').find('.status');

		var transportId = $panel.find('[name="transportId"]').val();
		var courseId = $panel.find('[name="courseId"]').val();
		var notificationType = $checkbox.data('notificationType');
		var isEnabled = $checkbox.prop('checked');

		$.post(saveUrl,
				{
					'transportId': transportId,
					'courseId': courseId,
					'notificationType': notificationType,
					'isEnabled': isEnabled,
				}, null, 'json')
			.done(function (result) {
				if (result.status === 'ok') {
					window.notificationTransportsSettings = result.notificationTransportsSettings;

					$status.addClass('text-success').text('Сохранено!');
					setTimeout(function () {
							$status.text('').removeClass('text-success');
						},
						1000);
				}
			})
			.fail(function (result) {
				$status.addClass('text-danger').text('Произошла ошибка');
				console.error(result);
			})
			.always(function () {
			});

		return false;
	});
});