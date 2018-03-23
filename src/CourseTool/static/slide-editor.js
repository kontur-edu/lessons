CodeMirror.commands.autocomplete = function (cm) {
	var hint = cm.options.langInfo.hint;
	if (hint)
		cm.showHint({ hint: hint });
}

function getLangInfo(langId) {
	// see http://codemirror.net/mode/

	if (!langId)
		return { mode: "text/plain", hint: null };

	switch (langId) {
		case "cs":
		case "сsharp":
			return { mode: "text/x-csharp", hint: CodeMirror.hint.csharp };
		case "py":
		case "python":
			return { mode: "text/x-python", hint: CodeMirror.hint.python };
		case "js":
		case "javascript":
			return { mode: "text/javascript", hint: CodeMirror.hint.javascript };
		case "ts":
		case "typescript":
			return { mode: "text/typescript", hint: CodeMirror.hint.javascript };
		case "css":
			return { mode: "text/css", hint: CodeMirror.hint.css };
		case "html":
			return { mode: "text/html", hint: CodeMirror.hint.html };
		default:
			return { mode: "text/" + langId, hint: null };
	}
}

function codeMirrorClass(c, editable) {
	var codes = document.getElementsByClassName(c);
	for (var i = 0; i < codes.length; i++) {
		var element = codes[i];
		var $el = $(element);
		var langId = $el.data("lang");
		var langInfo = getLangInfo(langId);
		var editor = CodeMirror.fromTextArea(element,
		{
			mode: langInfo.mode,
			langInfo: langInfo,
			lineNumbers: true,
			theme: editable ? "cobalt" : "default",
			indentWithTabs: true,
			tabSize: 4,
			indentUnit: 4,
			extraKeys: {
				"Ctrl-Space": "autocomplete",
				".": function(cm) {
					setTimeout(function() { cm.execCommand("autocomplete"); }, 100);
					return CodeMirror.Pass;
				}
			},
			readOnly: !editable,
			//autoCloseBrackets: true, // bug: autoCloseBracket breakes indentation after for|while|...
			styleActiveLine: editable,
			matchBrackets: true,
	});
		element.codeMirrorEditor = editor;
		if (editable)
			editor.focus();

	}
}

codeMirrorClass("code-exercise", true);
codeMirrorClass("code-sample", false);

function refreshPreviousDraft(ac, id) {
	if ($('.code-exercise').length > 0) {
		window.onbeforeunload = function() {
			if (ac == 'False')
				localStorage[id] = $('.code-exercise')[0].codeMirrorEditor.getValue();
		}
		if (localStorage[id] != undefined && ac == 'False') {
			$('.code-exercise')[0].codeMirrorEditor.setValue(localStorage[id]);
		}
	}
}