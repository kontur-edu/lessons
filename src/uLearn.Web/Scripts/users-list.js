function ToggleRole(event, target, toggleClass) {
	event.stopPropagation();
	var obj = $(target);
	var url = obj.data("toggle-url");
	var token = $('#AntiForgeryTokenContainer input[name="__RequestVerificationToken"]').val();
	$.ajax({
			url: url,
			method: "POST",
			data: {
				__RequestVerificationToken: token
			}
		})
		.success(function() {
			toggleClass(obj);
		});
}

function ToggleButtonClass(button) {
	button.toggleClass(button.data("css-class"));
}

function ToggleDropDownClass(dropdownElement) {
	var parent = $(dropdownElement.parents()[0]);
	var cssClass = parent.data("css-class");
	dropdownElement.toggleClass(cssClass);
	var elements = dropdownElement.siblings().andSelf();
	var button = $(parent.siblings("button")[0]);
	var buttonCss = button.data("css-class");
	button.removeClass(buttonCss);
	for (var i = 0; i < elements.length; ++i) {
		var elem = $(elements[i]);
		if (elem.hasClass(cssClass)) {
			button.addClass(buttonCss);
			break;
		}
	}
}
