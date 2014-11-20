(function (mod) {
	if (typeof exports == "object" && typeof module == "object") // CommonJS
		mod(require("../../lib/codemirror"));
	else if (typeof define == "function" && define.amd) // AMD
		define(["../../lib/codemirror"], mod);
	else // Plain browser env
		mod(CodeMirror);
})(function (CodeMirror) {
	"use strict";

	function csharpHint(editor) {
		var cur = editor.getCursor(), token = editor.getTokenAt(cur), tprop = token;
		var from = CodeMirror.Pos(cur.line, token.start);
		var bracketsCount = 0;
		var totalLen = 0;
		var completionList;
		if (token.string === ".") {
			var lastToken = "";
			from = CodeMirror.Pos(cur.line, token.start + 1);
			cur.ch = cur.ch - 1;
			lastToken = editor.getTokenAt(cur);
			if (lastToken.string == ')') {
				bracketsCount++;
				totalLen = 0;
				while (bracketsCount != 0) {
					cur.ch = cur.ch - lastToken.string.length;
					totalLen += lastToken.string.length;
					lastToken = editor.getTokenAt(cur);
					if (lastToken.string == ')')
						bracketsCount++;
					if (lastToken.string == '(')
						bracketsCount--;
					if (cur.ch == 0)
						break;
				}
				cur.ch = cur.ch - 1;
				lastToken = editor.getTokenAt(cur);
				cur.ch = cur.ch + totalLen + 1;
			}
			cur.ch = cur.ch + 1;
			completionList = completer.getCompletions(lastToken.string, "", true);
		}
		else if (!/^[\w@_]*$/.test(token.string)) {
			from = cur;
			completionList = completer.getCompletions("", "", false);
		} else {
			var beforeDot = "";
			cur.ch = cur.ch - token.string.length;
			var lastDot = editor.getTokenAt(cur);
			if (lastDot.string == '.') {
				cur.ch = cur.ch - 1;
				beforeDot = editor.getTokenAt(cur);
				if (beforeDot.string == ')') {
				bracketsCount++;
				totalLen = 0;
					while (bracketsCount != 0) {
						cur.ch = cur.ch - beforeDot.string.length;
						totalLen += beforeDot.string.length;
						beforeDot = editor.getTokenAt(cur);
						if (beforeDot.string == ')')
							bracketsCount++;
						if (beforeDot.string == '(')
							bracketsCount--;
						if (cur.ch == 0)
							break;
					}
					cur.ch = cur.ch - 1;
					beforeDot = editor.getTokenAt(cur);
					cur.ch = cur.ch + totalLen + 1;
				}
				cur.ch = cur.ch + 1;
			}
			cur.ch = cur.ch + token.string.length;

			completionList = completer.getCompletions(beforeDot.string, token.string, false);
		}

		completionList = completionList.sort();

		return {
			list: completionList,
			from: from,
			to: CodeMirror.Pos(cur.line, token.end)
		};
	}
	var completer = new CsCompleter(Object.keys(CodeMirror.resolveMode("text/x-csharp").keywords));


	CodeMirror.registerHelper("hint", "csharp", csharpHint);
});
