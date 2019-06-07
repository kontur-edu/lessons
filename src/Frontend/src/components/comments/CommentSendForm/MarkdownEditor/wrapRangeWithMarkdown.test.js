import CommentSendForm from "../CommentSendForm";

describe('CommentSendForm.wrapRangeWithMarkdown', () => {
	test.each([
		["", 0, 0, "**", "****", 2, 2],
		["", 0, 0, "_", "__", 1, 1],
		["", 0, 0, "```", "``````", 3, 3],
		["hello", 0, 0, "_", "__hello", 1, 1],
		["a", 1, 1, "_", "a__", 2, 2],
		["hello", 1, 2, "_", "h_e_llo", 1, 4],
		["abc", 0, 3, "_", "_abc_", 0, 5],
		["hello", 1, 1, "**", "h****ello", 3, 3],
		["hello", 1, 2, "**", "h**e**llo", 1, 6],
		["hello", 1, 2, "```", "h```e```llo", 1, 8],
	])('%s with selection, %i, %i %s -> %s with selection, %i, %i',
		(text, selectionStart, selectionEnd, markup, expectedText, expectedSelectionStart, expectedSelectionEnd) => {
			const result = CommentSendForm.wrapRangeWithMarkdown(text, {
				start: selectionStart,
				end: selectionEnd
			}, {markup: markup});
			expect(result).toEqual({
				finalText: expectedText,
				finalSelectionRange: {end: expectedSelectionEnd, start: expectedSelectionStart}
			});
		});
});

