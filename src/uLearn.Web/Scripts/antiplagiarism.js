function fetchAntiPlagiarismStatus($plagiarismStatus) {
	var shameComment = 'Ой! Наш робот нашёл решения других студентов, подозрительно похожие на ваше. ' +
		'Так может быть, если вы позаимствовали части программы, взяли их из открытых источников либо сами поделились своим кодом. ' +
		'Выполняйте задания самостоятельно.';
	
    $plagiarismStatus.removeClass('found-level0 found-level1 found-level2');
    
    var url = $plagiarismStatus.data('antiplagiarismUrl');
    var addCodeReviewUrl = $plagiarismStatus.data('addExerciseCodeReviewUrl');
    var $plagiarismStatusFixedCopy = $plagiarismStatus.clone().addClass('fixed').hide().insertAfter($plagiarismStatus);
    var $codeMirror = $plagiarismStatus.parent().find('.CodeMirror');
    
    $.getJSON(url, function (data) {
        if (data.status === 'not_checked') {
            $plagiarismStatus.addClass('not-checked');
            $plagiarismStatusFixedCopy.addClass('not-checked');
            $plagiarismStatus.text('Эта задача не проверяется на списывание');
            $plagiarismStatusFixedCopy.text($plagiarismStatus.text());
            return;
        }

        var className = 'found-level' + data.suspicion_level;
        $plagiarismStatus.addClass(className);
        $plagiarismStatusFixedCopy.addClass(className);
        var message = '';
        switch (data.suspicion_level)
        {
            case 0: message = 'похожих решений не найдено'; break;
            case 1:
            case 2:
                var singleNumberMessage = 'у {count} другого студента найдено {very} похожее решение. {details_link} и {shame_link}.';
                var pluralNumberMessage = 'у {count} других студентов найдены {very} похожие решения. {details_link} и {shame_link}.';
                message = data.suspicious_authors_count === 1 ? singleNumberMessage : pluralNumberMessage;
                break;
        }
        message = message.replace('{count}', data.suspicious_authors_count);
        message = message.replace('{very}', data.suspicion_level === 2 ? '<b>очень</b>' : '');
        message = message.replace('{details_link}', 'Посмотрите <a href="' + $plagiarismStatus.data('antiplagiarismDetailsUrl') + '" target="_blank">подробности</a>');
		message = message.replace('{shame_link}', '<a class="internal-page-link antiplagiarism-shame-button" href="#">поставьте 0 баллов</a>');
        
        $plagiarismStatus.html('Проверка на списывание: ' + message);
		$plagiarismStatusFixedCopy.html($plagiarismStatus.html());
		
		if (data.suspicion_level !== 0) {			
			$('.antiplagiarism-shame-button').tooltip({
				title: '<div class="text-left">Нажмите, если тоже думаете, что решение списано: мы поставим за него 0 баллов и оставим студенту комментарий. Все действия обратимы.</div>',
				html: true,
				placement: 'bottom',
				fallbackPlacement: 'left',
				trigger: 'hover',
			});
			
			var $exerciseSubmission = $('.exercise__submission');
			$exerciseSubmission.on('click', '.antiplagiarism-shame-button', function(e) {
				e.preventDefault();
				
				$('.antiplagiarism-shame-button').tooltip('hide');
				
				postExerciseCodeReview(addCodeReviewUrl, {
					head: { line: 0, ch: 0 },
					anchor: { line: 1, ch: 0 }
				}, shameComment);

				/* Set 0 points */
				var $exerciseScore = $('.exercise__score');
				$exerciseScore.find('[data-value="0"]:not(.active)').click();
				
				/* Prohibit further review */
				var $prohibitFurtherReview = $('#prohibitFurtherReview');
				$prohibitFurtherReview.prop('checked', true);
			});
		}
    });
    
    var headerHeight = $('.header').outerHeight();    
    var codeMirrorBottom = $codeMirror.offset().top + $codeMirror.outerHeight();
    $(window).scroll(function () {
        var scrollTop = $(window).scrollTop();
		var plagiarismStatusOffset = $plagiarismStatus.offset().top;

        var isVisible = $plagiarismStatusFixedCopy.is(':visible');
        if (scrollTop >= plagiarismStatusOffset - headerHeight && scrollTop < codeMirrorBottom - 2 * headerHeight) {
            if (! isVisible) {                
                $plagiarismStatusFixedCopy.show();
            }
        }
        else {
            if (isVisible) {
                $plagiarismStatusFixedCopy.hide();
            }
        }
    });
}

/* Extracted from merge addon for codemirror */
var dmp;
function getDiff(a, b, ignoreWhitespace) {
    if (!dmp) dmp = new diff_match_patch();

    var diff = dmp.diff_main(a, b);
    // The library sometimes leaves in empty parts, which confuse the algorithm
    for (var i = 0; i < diff.length; ++i) {
        var part = diff[i];
        if (ignoreWhitespace ? !/[^ \t]/.test(part[1]) : !part[1]) {
            diff.splice(i--, 1);
        } else if (i && diff[i - 1][0] == part[0]) {
            diff.splice(i--, 1);
            diff[i][1] += part[1];
        }
    }
    return diff;
}
/* End of extracted from merge addon for codemirror */

window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
    $('.antiplagiarism__data').each(function () {
        var $self = $(this);
        var originalSubmissionId = $self.data('originalSubmissionId');
        var plagiarismSubmissionId = $self.data('plagiarismSubmissionId');

        var $originalSubmission = $('.code[data-submission-id="' + originalSubmissionId + '"]');
        var $plagiarismSubmission = $('.code[data-submission-id="' + plagiarismSubmissionId + '"]');

        var originalCodeMirror = $originalSubmission[0].codeMirrorEditor;
        var plagiarismCodeMirror = $plagiarismSubmission[0].codeMirrorEditor;
        
        var antiplagiarismData = JSON.parse($self[0].innerHTML);       
        var plagiarismData = antiplagiarismData.plagiarism;
                        
        var originalTokens = getTokensDictionaryByIndex(antiplagiarismData.tokens_positions);
        var plagiarismTokens = getTokensDictionaryByIndex(plagiarismData.tokens_positions);
        
        var originalTokensByLines = getTokensByLines($originalSubmission.text(), antiplagiarismData.tokens_positions);
        var plagiarismTokensByLines = getTokensByLines($plagiarismSubmission.text(), plagiarismData.tokens_positions);
        var bestMatchedLines = findBestMatchedLines(originalTokensByLines, plagiarismTokensByLines, plagiarismData.matched_snippets);
        
        var originalSubmissionLines = $originalSubmission.text().split('\n');
        var plagiarismSubmissionLines = $plagiarismSubmission.text().split('\n');
        originalCodeMirror.operation(function () {
            plagiarismCodeMirror.operation(function () {
                highlightBestMatchedLines(originalCodeMirror, plagiarismCodeMirror, bestMatchedLines, originalSubmissionLines, plagiarismSubmissionLines);    
            });             
        });
        
        /* Align author's labels by height, because sometimes one of them is more than other and codemirror aligning works bad */
        var $authorLabels = $(originalCodeMirror.getWrapperElement()).closest('.antiplagiarism').find('.antiplagiarism__author');
        var maxHeight = 0;
        $authorLabels.each(function (_, label) {
            var labelHeight = $(label).height();
            if (maxHeight < labelHeight)
                maxHeight = labelHeight;
        });
        $authorLabels.height(maxHeight);
        
        /* Setup click handlers */
        $(originalCodeMirror.getWrapperElement()).on('click', function (e) {
            var cursor = originalCodeMirror.getCursor();
            var originalLine = cursor.line;
            
            var bestMatchedLine = bestMatchedLines[originalLine];
            if (bestMatchedLine === undefined || bestMatchedLine < 0) // -2 or -1
                return;

            alignCodeMirrors(originalCodeMirror, plagiarismCodeMirror, originalLine, bestMatchedLine, 'original');
        });
        $(plagiarismCodeMirror.getWrapperElement()).on('click', function (e) {
            var cursor = plagiarismCodeMirror.getCursor();
            var plagiarismLine = cursor.line;

            var originalLine = undefined;
            $.each(bestMatchedLines, function (index, value) {
                if (value === plagiarismLine)
                    originalLine = index;
            });
            
            if (originalLine === undefined)
                return;
            
            alignCodeMirrors(originalCodeMirror, plagiarismCodeMirror, originalLine, plagiarismLine, 'plagiarism');
        });
        
        /* Batch all operations as one: see https://codemirror.net/doc/manual.html for details. It's much faster because
         * doesn't need to fully relayout and redraw DOM tree */
        originalCodeMirror.operation(function () {
            highlightNotAnalyzedParts(originalCodeMirror, antiplagiarismData.analyzed_code_units, originalTokens);    
        });
        plagiarismCodeMirror.operation(function() {
            highlightNotAnalyzedParts(plagiarismCodeMirror, plagiarismData.analyzed_code_units, plagiarismTokens);    
        });       
        
    });
    
    function alignCodeMirrors(originalCodeMirror, plagiarismCodeMirror, originalLine, plagiarismLine, shouldNotMoved) {
        var originalPosition = originalCodeMirror.cursorCoords({ line: originalLine, ch: 1 }, 'local');
        var plagiarismPosition = plagiarismCodeMirror.cursorCoords({ line: plagiarismLine, ch: 1}, 'local');

        var originalMarginTop = 0, plagiarismMarginTop = 0;
        if (originalPosition.top < plagiarismPosition.top)
            originalMarginTop = plagiarismPosition.top - originalPosition.top;
        else
            plagiarismMarginTop = originalPosition.top - plagiarismPosition.top;

        var marginTopDiff = 0;
        if (shouldNotMoved === 'original') 
            marginTopDiff = parseInt($(originalCodeMirror.getWrapperElement()).css('marginTop')) - originalMarginTop;
        else if (shouldNotMoved === 'plagiarism')
            marginTopDiff = parseInt($(plagiarismCodeMirror.getWrapperElement()).css('marginTop')) - plagiarismMarginTop;
        
        $(originalCodeMirror.getWrapperElement()).animate({'marginTop': originalMarginTop}, 500, 'linear');
        $(plagiarismCodeMirror.getWrapperElement()).animate({'marginTop': plagiarismMarginTop}, 500, 'linear');
        
        /* Scroll full document for making effect that only right submissions is scrolled, not left */
        $('html').animate({scrollTop: $('html').scrollTop() - marginTopDiff}, 500, 'linear');        
    }
    
    /* Returns array result, result[i] contains token indexes which contains in i-th line of input */
    function getTokensByLines(text, tokens) {
        var currentTokenIndex = 0;
        var lineIndex = 0;
        var result = [[]];
        for (var charIndex = 0; charIndex < text.length && currentTokenIndex < tokens.length; charIndex++) {
            var char = text[charIndex];
            if (char === '\n') {
                lineIndex++;
                result.push([]);
                continue;
            }
            if (char === '\r')
                continue;
            
            /* Go to next token if current is finished */
            if (charIndex >= tokens[currentTokenIndex].finish_position)
                currentTokenIndex++;
            
            /* No more tokens in input array */
            if (currentTokenIndex >= tokens.length)
                break;
            
            if (charIndex >= tokens[currentTokenIndex].start_position && charIndex < tokens[currentTokenIndex].finish_position) {
                /* Don't add the same token_index again */
                if (result[lineIndex][result[lineIndex].length - 1] !== tokens[currentTokenIndex].token_index)
                    result[lineIndex].push(tokens[currentTokenIndex].token_index)
            }
        }
        return result;
    }
    
    function findBestMatchedLines(originalTokensByLines, plagiarismTokensByLines, matchedSnippets)
    {
        /* Initialize array N x M by zeros */
        var lineWeights = new Array(originalTokensByLines.length);
        for (var lineId = 0; lineId < originalTokensByLines.length; lineId++) {
            lineWeights[lineId] = new Array(plagiarismTokensByLines.length);
            for (var pLineId = 0; pLineId < plagiarismTokensByLines.length; pLineId++)
                lineWeights[lineId][pLineId] = 0;
        }
        
        var originalLineByTokens = invertArray(originalTokensByLines);
        var plagiarismLineByTokens = invertArray(plagiarismTokensByLines);
        
        $.each(matchedSnippets, function (idx, matchedSnippet) {
            for (var tokenIndex = 0; tokenIndex < matchedSnippet.snippet_tokens_count; ++tokenIndex) {
            	try {
					var originalTokenIndex = matchedSnippet.original_submission_first_token_index + tokenIndex;
					var plagiarismTokenIndex = matchedSnippet.plagiarism_submission_first_token_index + tokenIndex;
					var originalLine = originalLineByTokens[originalTokenIndex][0];
					var plagiarismLine = plagiarismLineByTokens[plagiarismTokenIndex][0];

					lineWeights[originalLine][plagiarismLine]++;
				} catch (e) {
					console.error(e);
				}
            }                
        });
        
        /* Search best match for each line (or return -1 if several lines has equal weight ) */
        var result = [];
        var reversedResult = [];
        for (var lineId = 0; lineId < originalTokensByLines.length; lineId++) {
            if (! (lineId in originalTokensByLines) || originalTokensByLines[lineId].length === 0) {
                result[lineId] = -2; // -2 means "no token in this line"
                continue;
            } 
            
            var bestMatch = -1; // -1 means "can not find best matched line"
            var bestMatchWeight = 0;
            for (var pLineId = 0; pLineId < plagiarismTokensByLines.length; pLineId++) {
                if (lineWeights[lineId][pLineId] > bestMatchWeight) {
                    bestMatchWeight = lineWeights[lineId][pLineId];
                    bestMatch = pLineId;
                } else if (lineWeights[lineId][pLineId] === bestMatchWeight)
                    bestMatch = -1;     
            }
            
            /* Doesn't allow duplicated values in result array (except -1 or -2) */
            if (bestMatch >= 0 && bestMatch in reversedResult)
                bestMatch = -1;
            
            result[lineId] = bestMatch;
            reversedResult[bestMatch] = lineId;
        }            
        return result;
    }
    
    function invertArray(array) {
        var result = [];
        $.each(array,  function(id, elems) {
            $.each(elems, function (_, elem) {
                if (! (elem in result))
                    result[elem] = [];
                result[elem].push(id);
            })
        });
        return result;
    }
    
    function isLatinChar(c) {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
    function isLatinUppercase(c) {
        return c >= 'A' && c <= 'Z';
    }
    function isDigit(c) {
        return c >= '0' && c <= '9';
    }
    function hasOnlySpaces(s) {
        for (var i = 0; i < s.length; i++) 
            if (s[i] !== ' ' && s[i] !== '\n' && s[i] !== '\r' && s[i] !== '\t')
                return false;
        return true;
    }
    
    function highlightBestMatchedLines(originalCodeMirror, plagiarismCodeMirror, bestMatchedLines, originalSubmissionLines, plagiarismSubmissionLines)
    {
        var fullMatchedTextMarkerOptions = {
            className: 'antiplagiarism__full-matched',
            title: 'Эта часть кода совпадает полностью. Нажмите, чтобы увидеть аналогичное место в другом решении',
        };        
        var notMatchedTextMarkerOptions = {
            className: 'antiplagiarism__not-matched',
            title: 'Нажмите, чтобы увидеть аналогичное место в другом решении',
        };

        function highlightBestMatchedLine(lineId, bestMatchedLine, allowSuggests) {
            allowSuggests = allowSuggests || 'both';
            
            if (bestMatchedLine < 0) // -2 or -1
                return;
            
            var originalLine = originalSubmissionLines[lineId];
            var plagiarismLine = plagiarismSubmissionLines[bestMatchedLine];
            var diff = getDiff(originalLine, plagiarismLine);

            /* Each `diff` item is 2-element array: [0, "qwer"] for commont substring,
               [1, "abcd"] for adding and [-1, "zxcv"] for removing substring */
            var originalCharIndex = 0;
            var plagiarismCharIndex = 0;
            $.each(diff, function (_, diffItem) {
                var diffType = diffItem[0];
                var diffString = diffItem[1];
                
                /* Correcting: full-matched blocks can start only from beginning of the word
                   (upper-case letter, or if previous char it not letter) or from beginnin og the number.
                   Also full-matched blocks should have length at least 4 chars.
                   Bad full-matched blocks are marked with special value of diffType: -100. It means that it should be marked in both texts, but as not-matched */
                var BAD_FULL_MATCHED = -100;
                var badFullMatchedLength = 0;
                console.log('Diff is', diffItem);
                if (diffType === 0) {
                    var firstChar = diffString[0];
                    var previousChar = originalCharIndex > 0 ? originalLine[originalCharIndex - 1] : '';
                    console.log('Checking full-matched substring "' + diffString + '" for badness. First char is', firstChar, 'previous char is', previousChar);                    
                    if (isLatinChar(firstChar)) {
                        var isFirstLetter = previousChar === '' || isLatinUppercase(firstChar) || !isLatinChar(previousChar);
                        if (! isFirstLetter) {
                            diffType = BAD_FULL_MATCHED;
                            /* Looking for the end of latin word */
                            badFullMatchedLength = 1;
                            while (badFullMatchedLength < diffString.length &&
                                   originalCharIndex + badFullMatchedLength < originalLine.length &&
                                   isLatinChar(originalLine[originalCharIndex + badFullMatchedLength]))
                                badFullMatchedLength++;
                        }
                    }
                    else if (isDigit(firstChar)) {
                        if (isDigit(previousChar)) {
                            diffType = BAD_FULL_MATCHED;
                            /* Looking for the end of number */
                            badFullMatchedLength = 1;
                            while (badFullMatchedLength < diffString.length && 
                                   originalCharIndex + badFullMatchedLength < originalLine.length &&
                                   isDigit(originalLine[originalCharIndex + badFullMatchedLength]))
                                badFullMatchedLength++;
                        }
                    }
                    
                    if (diffString.length < 4) {
                        diffType = BAD_FULL_MATCHED;
                        badFullMatchedLength = diffString.length;
                    }
                }

                if (diffType === BAD_FULL_MATCHED) {
                    originalCodeMirror.markText({line: lineId, ch: originalCharIndex}, {line: lineId, ch: originalCharIndex + badFullMatchedLength}, notMatchedTextMarkerOptions);
                    console.log('originalCharIndex:', originalCharIndex, '+', badFullMatchedLength, '=', originalCharIndex + badFullMatchedLength);
                    originalCharIndex += badFullMatchedLength;
                    plagiarismCodeMirror.markText({line: bestMatchedLine, ch: plagiarismCharIndex}, {line: bestMatchedLine,ch: plagiarismCharIndex + badFullMatchedLength}, notMatchedTextMarkerOptions);
                    console.log('plagiarismCharIndex:', plagiarismCharIndex, '+', badFullMatchedLength, '=', plagiarismCharIndex + badFullMatchedLength);
                    plagiarismCharIndex += badFullMatchedLength;
                    
                    diffString = diffString.substring(badFullMatchedLength);
                    /* All other stuff from diffString should be marked as fully matched string */
                    diffType = 0;
                }

                if (diffType === 0) {
                    originalCodeMirror.markText({line: lineId, ch: originalCharIndex}, {line: lineId, ch: originalCharIndex + diffString.length}, fullMatchedTextMarkerOptions);
                    console.log('originalCharIndex:', originalCharIndex, '+', diffString.length, '=', originalCharIndex + diffString.length);
                    originalCharIndex += diffString.length;
                    plagiarismCodeMirror.markText({line: bestMatchedLine, ch: plagiarismCharIndex}, {line: bestMatchedLine, ch: plagiarismCharIndex + diffString.length}, fullMatchedTextMarkerOptions);
                    console.log('plagiarismCharIndex:', plagiarismCharIndex, '+', diffString.length, '=', plagiarismCharIndex + diffString.length);
                    plagiarismCharIndex += diffString.length;
                }
                var markerOptions;
                if (diffType === -1) {
                    /* If diff contains only space chars (' ', '\t', '\n', ...) then hightlight is as fully matched (red color) */
                    markerOptions = hasOnlySpaces(diffString) ? fullMatchedTextMarkerOptions : notMatchedTextMarkerOptions;
                    originalCodeMirror.markText({line: lineId, ch: originalCharIndex}, {line: lineId, ch: originalCharIndex + diffString.length}, markerOptions);
                    console.log('originalCharIndex:', originalCharIndex, '+', diffString.length, '=', originalCharIndex + diffString.length);
                    originalCharIndex += diffString.length;
                }
                if (diffType === 1) {
                    markerOptions = hasOnlySpaces(diffString) ? fullMatchedTextMarkerOptions : notMatchedTextMarkerOptions;
                    plagiarismCodeMirror.markText({line: bestMatchedLine, ch: plagiarismCharIndex}, {line: bestMatchedLine,ch: plagiarismCharIndex + diffString.length}, markerOptions);
                    console.log('plagiarismCharIndex:', plagiarismCharIndex, '+', diffString.length, '=', plagiarismCharIndex + diffString.length);
                    plagiarismCharIndex += diffString.length;
                }                
            });

            /* See next line and try to concat it to matched block */
            /* allowSuggests is parameter for preventing recursion stack overflow: We should not see previous line if now we are seeing the next line */
            var nextLineId = lineId + 1;
            if (nextLineId in bestMatchedLines && bestMatchedLines[nextLineId] === -1 && (allowSuggests === 'both' || allowSuggests === 'next'))
                suggestMatchedLineAroundBlock(nextLineId, bestMatchedLine + 1, 'next');            
            
            /* See previous line */
            var previousLineId = lineId - 1;
            if (previousLineId in bestMatchedLines && bestMatchedLines[previousLineId] === -1 && (allowSuggests === 'both' || allowSuggests === 'prev'))
                suggestMatchedLineAroundBlock(previousLineId, bestMatchedLine - 1, 'prev');            
        }
        
        function suggestMatchedLineAroundBlock(lineId, suggestedBestMatchedLine, allowSuggests) {
            var suggestedDiff = getDiff(originalSubmissionLines[lineId], plagiarismSubmissionLines[suggestedBestMatchedLine]);

            /* Add next line from original submission to common part if diff with suggested (next from plagiarism) line is small */
            var commonLength = 0;
            $.each(suggestedDiff, function (_, diffItem) {
                if (diffItem[0] === 0)
                    commonLength += diffItem[1].length;
            });

            if (2 * commonLength / (originalSubmissionLines[lineId].length + plagiarismSubmissionLines[suggestedBestMatchedLine].length) >= 0.75) {
                highlightBestMatchedLine(lineId, suggestedBestMatchedLine, allowSuggests);
            }
        }
        
        $.each(bestMatchedLines, highlightBestMatchedLine);    
    }
    
    function getTokensDictionaryByIndex(tokensPositionsArray) {
        var result = {};
        $.each(tokensPositionsArray, function (idx, tokenInfo) {
            result[tokenInfo.token_index] = tokenInfo;
            result[tokenInfo.token_index].finish_position = tokenInfo.start_position + tokenInfo.length;
        });
        return result;        
    }
    
    function highlightNotAnalyzedParts(codeMirrorEditor, analyzedCodeUnits, tokens) {
        var document = codeMirrorEditor.getDoc();

        var highlightedTokes = [];
        $.each(analyzedCodeUnits, function (idx, codeUnit) {
            var firstTokenIndex = codeUnit.first_token_index;
            var lastTokenIndex = codeUnit.first_token_index + codeUnit.tokens_count - 1;
            for (var tokenIndex = firstTokenIndex; tokenIndex <= lastTokenIndex; tokenIndex++)
                highlightedTokes.push(tokenIndex);
        });
        highlightedTokes.sort(function (a, b) {
            return a - b;
        });
        
        var textMarkerOptions = {
            className: 'antiplagiarism__not-analyzed',
            title: 'Эта часть кода не анализируется на списывание',
        };

        var currentHighlightStart = 0;
        for (var idx = 0; idx < highlightedTokes.length; idx++) {
            if (idx === 0 || highlightedTokes[idx - 1] < highlightedTokes[idx] - 1) {
                var currentHighlightFinish = tokens[highlightedTokes[idx]].start_position;
                if (currentHighlightStart !== currentHighlightFinish) {  
                    document.markText(
                        document.posFromIndex(currentHighlightStart),
                        document.posFromIndex(currentHighlightFinish),
                        textMarkerOptions
                    );
                }
            }
            currentHighlightStart = tokens[highlightedTokes[idx]].finish_position;
        }

        document.markText(document.posFromIndex(currentHighlightStart), document.posFromIndex(1e10), textMarkerOptions);
    }    
      
    /* Fetching antiplagiarism status */
    $('.antiplagiarism-status').each(function () {
        fetchAntiPlagiarismStatus($(this));
    });
    
    /* Changing submission on panel */
    $('.antiplagiarism__submissions-panel [name="submissionId"]').change(function () {
        var $self = $(this);
        $('.antiplagiarism').hide();
        $('.suspicion-level-description').hide();
        var $form = $self.closest('form');        
        $form.submit();
    });
});