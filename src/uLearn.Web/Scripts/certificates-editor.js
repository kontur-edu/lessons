window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	var $modal = $('#createOrUpdateCertificateTemplateModal');
	var $form = $('#createOrUpdateCertificateTemplateModal form');
	var token = $('input[name="__RequestVerificationToken"]').val();

	var loadBuiltinParametersValuesForUser = function ($certificateRow, userId) {
		var url = $('.preview-certificates').data('builtinParametersValueUrl');
		url = url.replace('USER_ID', userId);

		$certificateRow.find('[data-parameter]').each(function() {
			$(this).text('');
			$('.loading-spinner-template').clone().removeClass('loading-spinner-template').appendTo($(this)).show();
		});

		$.getJSON(url, function (data) {
			for (var parameterName in data)
				$certificateRow.find('[data-parameter="' + parameterName + '"]').text(data[parameterName]);
		});
	}

	var validateIfAllUsersFilled = function () {
		var allFilled = true;
		$('.preview-certificates .user-id').each(function() {
			if ($(this).val() === '')
				allFilled = false;
		});

		if (allFilled) {
			$('.generate-certificates-button').removeAttr('disabled');
			$('.generate-certificates-status').hide();
		} else {
			$('.generate-certificates-button').attr('disabled', 'disabled');
			$('.generate-certificates-status').show();
		}
	}

	$('.create-template-link').click(function (e) {
		e.preventDefault();

		$form.attr('action', $form.data('createTemplateUrl'));
		$form.find('[name="templateId"]').val('');
		$form.find('[name="name"]').val('');
		$form.find('[name="archive"]').attr('required', 'required');
		$form.find('button.action-button').text('Загрузить');
		$form.find('.current-archive-download').hide();
		$form.find('.remove-template-link').hide();
		$modal.find('.modal-title').text('Новый шаблон');
		$modal.modal();
	});

	$('.edit-template-link').click(function (e) {
		e.preventDefault();

		var $self = $(this);
		var templateId = $self.data('templateId');
		var templateName = $self.data('templateName');

		$form.attr('action', $form.data('editTemplateUrl'));
		$form.find('[name="templateId"]').val(templateId);
		$form.find('[name="name"]').val(templateName);
		$form.find("[name=archive]").removeAttr('required');
		$form.find('button.action-button').text('Сохранить');
		$form.find('.current-archive-download-link').attr('href', $self.data('templateUrl'));
		$form.find('.current-archive-download').show();
		$form.find('.remove-template-link').show();
		$modal.find('.modal-title').text('Редактировать шаблон');
		$modal.modal();
	});

	$('.remove-template-link').click(function (e) {
		e.preventDefault();

		var $self = $(this);
		var $form = $self.closest('form');
		var courseId = $form.find('[name=courseId]').val();
		var templateId = $form.find('[name=templateId]').val();
		$.ajax({
			type: 'post',
			url: $self.data('url'),
			data: {
				__RequestVerificationToken: token,
				courseId: courseId,
				templateId: templateId,
			}
		}).done(function () {
			window.location.reload(true);
		});
	});

	$('.add-certificate-input').each(function () {
		var $self = $(this);
		var $form = $self.closest('form');
		var url = $self.data('url');
		$self.autocomplete({
			source: url,
			select: function (event, ui) {
				var item = ui.item;
				$self.val(item.value);
				$form.find('[name="userId"]').val(item.id);
				$form.find('.add-certificate-button').text('Выдать сертификат: ' + item.value);
				$form.find('.certificate-template__parameter').show();
				$form.find('.add-certificate-button').show();
				$form.find('.preview-certificate-button').show();
				return false;
			}
		});
	});

	$('.certificate').on('click', '.remove-certificate-link', function (e) {
		e.preventDefault();

		var $self = $(this);
		$.ajax({
			type: 'post',
			url: $self.data('url'),
			data: {
				__RequestVerificationToken: token,
			}
		}).fail(function () {
			alert('При отзыве сертификата произошла какая-то ошибка');
		});
		$self.parent().remove();
	});

	$('.add-certificate-button').click(function () {
		var $form = $(this).closest('form');
		$form.find('[name="isPreview"]').val("false");
		$form.attr('target', '');
		return true;
	});

	$('.preview-certificate-button').click(function () {
		var $form = $(this).closest('form');
		$form.find('[name="isPreview"]').val("true");
		$form.attr('target', '_blank');
		return true;
	});

	$('.generate-multiple-certificates-link').click(function () {
		var $modal = $('#multipleCertificatesGenerationModal');
		$modal.find('[name=templateId]').val($(this).data('templateId'));
		$modal.modal();
	});

	$('.preview-certificates .select-another-user-link').click(function (e) {
		e.preventDefault();

		var $self = $(this);
		var $parent = $self.closest('.certificate__user');
		var $selectUserBlock = $parent.find('.select-another-user-block');
		$selectUserBlock.find('input').val('');
		$parent.children().not($selectUserBlock).hide();
		$selectUserBlock.show();
	});

	var hideAnotherUserSelection = function($certificateUserBlock) {
		var $selectUserBlock = $certificateUserBlock.find('.select-another-user-block');
		$certificateUserBlock.children().not($selectUserBlock).show();
		$selectUserBlock.hide();
	}

	$('.preview-certificates .cancel-select-another-user-link').click(function(e) {
		e.preventDefault();

		var $self = $(this);
		hideAnotherUserSelection($self.closest('.certificate__user'));
	});

	$('.preview-certificates .select-another-user-input').each(function () {
		var $self = $(this);
		var $certificateUserBlock = $self.closest('.certificate__user');
		var $userIdInput = $certificateUserBlock.find('.user-id');
		var $userNameDiv = $certificateUserBlock.find('.user-name');
		var url = $self.data('url');
		$self.autocomplete({
			source: url,
			select: function (event, ui) {
				var item = ui.item;
				var userId = item.id;
				$userIdInput.val(userId);
				$userNameDiv.text(item.value);
				$certificateUserBlock.find('.select-another-user-predefined-select').remove();
				$certificateUserBlock.find('.nobody-found').remove();
				loadBuiltinParametersValuesForUser($certificateUserBlock.closest('.certificate'), userId);
				validateIfAllUsersFilled();
				hideAnotherUserSelection($certificateUserBlock);
				return false;
			}
		});
	});

	$('.preview-certificates .select-another-user-predefined-select').change(function() {
		var $self = $(this);
		var $certificateUserBlock = $self.closest('.certificate__user');
		var $userIdInput = $certificateUserBlock.find('.user-id');
		var $userNameDiv = $certificateUserBlock.find('.user-name');

		var $selected = $self.find('option:selected');
		var userId = $selected.attr('value');
		var userName = $selected.text();
		if (userId !== '') {
			$userIdInput.val(userId);
			$userNameDiv.text(userName);
			$certificateUserBlock.find('.select-another-user-predefined-select').remove();
			loadBuiltinParametersValuesForUser($certificateUserBlock.closest('.certificate'), userId);
			validateIfAllUsersFilled();
			hideAnotherUserSelection($certificateUserBlock);
		}
	});

	$('.preview-certificates .remove-certificate-preview-link').click(function(e) {
		e.preventDefault();
		$(this).closest('.certificate').remove();
		validateIfAllUsersFilled();
	});

	$('.preview-certificates .remove-certificate-preview-parameter-link').click(function(e) {
		e.preventDefault();
		var parameter = $(this).closest('th').data('parameter');
		$('[data-parameter="' + parameter + '"]').remove();
	});

	var initBuiltinParametersValues = function () {
		$('.preview-certificates .certificate').each(function () {
			var $self = $(this);
			var userId = $self.find('.user-id').val();
			if (userId)
				loadBuiltinParametersValuesForUser($self, userId);
		});
	};

	initBuiltinParametersValues();
	validateIfAllUsersFilled();
});