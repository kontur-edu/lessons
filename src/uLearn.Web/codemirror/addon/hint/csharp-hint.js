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
		var cur = editor.getCursor(), token = editor.getTokenAt(cur);
		var from = CodeMirror.Pos(cur.line, token.start);
		var to = CodeMirror.Pos(cur.line, token.end);
		var completionList;
		if (token.string === ".") {
			from = to;
			var lastTokenPos = skipBrackets(editor, cur);
			var lastToken = editor.getTokenAt(lastTokenPos);
			completionList = completer.getCompletions(lastToken.string, "", true);
		}
		else if (!/^[\w@_]*$/.test(token.string)) {
			from = cur;
			to = cur;
			completionList = completer.getCompletions("", "", false);
		} else {
			var beforeDot = "";
			var lastDot = getPrevToken(editor, cur);
			if (lastDot.token.string == '.') {
				var beforeDotCur = skipBrackets(editor, lastDot.pos);
				beforeDot = editor.getTokenAt(beforeDotCur);
			}
			completionList = completer.getCompletions(beforeDot.string, token.string, false);
		}

		completionList = completionList.sort();

		return {
			list: completionList,
			from: from,
			to: to
		};
	}
	var completer = new CsCompleter(Object.keys(CodeMirror.resolveMode("text/x-csharp").keywords));

	function skipBrackets(editor, pos) {
		var cursor = getPrevToken(editor, pos);
		if (cursor.token.string !== ')')
			return cursor.pos;
		var balance = 1;
		while (balance) {
			cursor = getPrevToken(editor, cursor.pos);
			if (cursor.token.string === ')')
				balance++;
			if (cursor.token.string === '(')
				balance--;
		}
		return getPrevToken(editor, cursor.pos).pos;
	}

	function getPrevToken(editor, pos) {
		var res = { ch: pos.ch, line: pos.line };
		var token = editor.getTokenAt(res);
		do {
			res.ch = token.start;
			if (res.ch === 0) {
				res = CodeMirror.Pos(res.line - 1);
			}
			token = editor.getTokenAt(res);
		} while (token.string.length == 0 || !/\S/.test(token.string));
		return {
			pos: res,
			token: token
		};
	}


	CodeMirror.registerHelper("hint", "csharp", csharpHint);
});