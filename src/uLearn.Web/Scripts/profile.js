window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function() {
	$('.profile__change-email__link').click(function (e) {
		e.preventDefault();

        var $self = $(this);
        var $formGroup = $self.closest('.form-group');
        var $emailInput = $('.profile__email-input');

        $formGroup.hide();
		$emailInput.show();
	});
	
	$('.login-and-registration-form .recommend-russian').keyup(function () {
        var $self = $(this);
        var val = $self.val();
		var $validationField = $self.next();
		if (/[a-z]/i.test(val)) {
            $validationField.removeClass('field-validation-valid').addClass('field-validation-error');
            $validationField.text('Мы советуем указывать имя и фамилию на русском языке, а не латиницей')
		} else {
            $validationField.removeClass('field-validation-error').addClass('field-validation-valid');
		}
    });
});