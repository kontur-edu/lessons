$(document).ready(function() {
	$('.profile__change-email__link').click(function (e) {
		e.preventDefault();

        let $self = $(this);
        let $formGroup = $self.closest('.form-group');
        let $emailInput = $('.profile__email-input');

        $formGroup.hide();
		$emailInput.show();
	});
	
	$('.login-and-registration-form .recommend-russian').keyup(function () {
        let $self = $(this);
        let val = $self.val();
		let $validationField = $self.next();
		if (/[a-z]/i.test(val)) {
            $validationField.removeClass('field-validation-valid').addClass('field-validation-error');
            $validationField.text('Мы советуем указывать имя и фамилию на русском языке, а не латиницей')
		} else {
            $validationField.removeClass('field-validation-error').addClass('field-validation-valid');
		}
    });
});