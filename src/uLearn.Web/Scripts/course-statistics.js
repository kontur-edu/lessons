window.documentReadyFunctions = window.documentReadyFunctions || [];

window.documentReadyFunctions.push(function () {
	var $courseStatistics = $('.course-statistics');

	/* Sticky header functions */
	var createTableHeaderCopy = function ($table, minTopOffset) {
		var $thead = $table.find('thead');
		var $copy = $thead.clone(true, true);
		
		var pendingChanges = [];
		$thead.find('tr').each(function (rowId, tr) {
			var $rowCopy = $copy.find('tr').eq(rowId);
			var $thsCopy = $rowCopy.find('th');
			$(tr).find('th').each(function (cellId, th) {
				var width = th.offsetWidth;
				width = Math.max(width, 20); // min width is 20px

				/* Put equals randomId to cell and it's copy to easy find pair
				   We use .attr() instead of .data() here because in other case '[data-random-id=1]' will not work */
				var randomId = Math.floor(Math.random() * 10000000);
				pendingChanges.push([th, width, randomId]);
                pendingChanges.push([$thsCopy[cellId], width, randomId]);	
			});
		});

		/* Run all changes in DOM as a batch, because we don't want doing relayouting too often */
		for (var i = 0; i < pendingChanges.length; i++)
		{
			var change = pendingChanges[i];
			var $object = $(change[0]);
			var width = change[1];
			var randomId = change[2];
			$object.css({ 'maxWidth': width, 'minWidth': width, 'width': width }).attr('data-random-id', randomId);
		}

		/* Add <table></table> tags around copied thead */
		var $tableForHeader = $('<table></table>')
			.addClass($table.attr('class'))
			.addClass('sticky-header')			
			.css({
				'position': 'fixed',
				'left': $table.position().left,
				'top': minTopOffset,
				'z-index': 2
			});
		$tableForHeader.append($copy);
		return $tableForHeader;
	};

	var createTableFirstColumnCopy = function ($table) {
		var $tbody = $table.find('tbody');
		var $copy = $tbody.clone(true, true);
		$copy.find('td:not(:first-child)').remove();
		$copy.find('tr').each(function (rowId, tr) {
			var $originalRow = $tbody.find('tr').eq(rowId);
			var $originalCells = $originalRow.find('td');
			$(tr).find('td').each(function (cellId, td) {
				var $td = $(td);
				var $originalTd = $originalCells.eq(cellId);
				var width = $originalTd.outerWidth();
				var height = $originalTd.outerHeight();

				/* Put equals randomId to cell and it's copy to easy find pair
				   We use .attr() instead of .data() here because in other case '[data-random-id=1]' will not work */
				var randomId = Math.floor(Math.random() * 10000000);
				$td.css({ 'maxWidth': width, 'minWidth': width, 'width': width, 'maxHeight': height, 'minHeight': height, 'height': height })
					.attr('data-random-id', randomId);
				$originalTd.css({ 'maxWidth': width, 'minWidth': width, 'width': width, 'maxHeight': height, 'minHeight': height, 'height': height })
					.attr('data-random-id', randomId);
			});
		});

		/* Add <table></table> tags around copied column */
		var $tableAround = $('<table></table>')
			.addClass($table.attr('class'))
			.addClass('sticky-column')
			.css({
				'position': 'fixed',
				'left': 0,
				'top': $table.offset().top,
				'z-index': 1
			});
		$tableAround.append($copy);
		return $tableAround;
	};

	var documentHeaderHeight = $('.header').outerHeight();
	var $stickyHeader;
	var $stickyColumn;

	/* Call this function each time when table header position is changed */
	var relocateStickyHeaderAndColumn = function ($table, $stickyHeader, $stickyColumn, minTopOffset) {
		var $tbody = $table.find('tbody');

		var scrollTop = $(document).scrollTop() + minTopOffset;
		var tableTopOffset = $table.offset().top;
		var isStickyHeaderVisible = tableTopOffset < scrollTop;
		$stickyHeader.toggle(isStickyHeaderVisible);

		var scrollLeft = $(document).scrollLeft();
		var tableLeftOffset = $table.offset().left;
		var isStickyColumnVisible = tableLeftOffset < scrollLeft;
		$stickyColumn.toggle(isStickyColumnVisible);

		$stickyHeader.css('left', $table.position().left - scrollLeft);
		$stickyColumn.css('top', $tbody.position().top - scrollTop + minTopOffset);
	};

	/* Call this function each time when rows order has been changed */
	var rerenderStickyColumn = function ($table, minTopOffset) {
		if ($stickyColumn)
			$stickyColumn.remove();

		$stickyColumn = createTableFirstColumnCopy($table);
		$stickyColumn.hide();
		var $legacyContainer = $('<div></div>').addClass('legacy-page').append($stickyColumn);
		$('body').append($legacyContainer);

		relocateStickyHeaderAndColumn($table, $stickyHeader, $stickyColumn, minTopOffset);
	};

	var rerenderStickyHeaderAndColumn = function ($table, minTopOffset) {
		if ($stickyHeader)
			$stickyHeader.remove();

		$stickyHeader = createTableHeaderCopy($table, minTopOffset);
		$stickyHeader.hide();
		var $legacyContainer = $('<div></div>').addClass('legacy-page').append($stickyHeader);
		$('body').append($legacyContainer);

		rerenderStickyColumn($table, minTopOffset);
	};

	/* Call func() for this cell and paired cell from sticky header */
	var callFunctionForPairedCells = function(func, $cell) {
		var randomId = $cell.data('randomId');
		$('.course-statistics').find('thead th').filter('[data-random-id=' + randomId + ']').each(function() {
			func($(this));
		});
	};

	/* End of sticky header functions */

	var addColspan = function($td, diff) {
		var colspan = parseInt($td.attr('colspan'));
		var newColspan = colspan + diff;
		$td.attr('colspan', newColspan);
		$td.toggle(newColspan > 0);
	}

	var _toggleUnitScoringGroup = function ($scoringGroupTitle) {
		/* It's needed for sticky header. For sticky header $courseStatisticsParent is not equal to $courseStatistics */
		var $courseStatisticsParent = $scoringGroupTitle.closest('.course-statistics');

		var unitId = $scoringGroupTitle.data('unitId');
		var scoringGroupId = $scoringGroupTitle.data('scoringGroup');
		var isExpanded = !$scoringGroupTitle.data('expanded');
		$scoringGroupTitle.data('expanded', isExpanded);

		var $expandIcon = $scoringGroupTitle.find('.expand-icon');
		$expandIcon.text(isExpanded ? '◀' : '▶');
		$expandIcon.remove();
		if (isExpanded)
			$scoringGroupTitle.find('a').prepend($expandIcon);
		else
			$scoringGroupTitle.find('a').append($expandIcon);

		var filterByUnitAndScoringGroup = '[data-unit-id="' + unitId + '"][data-scoring-group="' + scoringGroupId + '"]';
		$courseStatisticsParent.find('.slide-score' + filterByUnitAndScoringGroup).toggle(isExpanded);
		$courseStatisticsParent.find('.slide-title' + filterByUnitAndScoringGroup).toggle(isExpanded);
		$courseStatisticsParent.find('.slide-max-score' + filterByUnitAndScoringGroup).toggle(isExpanded);
		var slidesCount = $courseStatistics.find('.slide-title' + filterByUnitAndScoringGroup).length;

		var $unitTitleTh = $courseStatisticsParent.find('.unit-title[data-unit-id="' + unitId + '"]').closest('th');
		addColspan($unitTitleTh, isExpanded ? slidesCount : -slidesCount);
		
		$courseStatisticsParent.find(filterByUnitAndScoringGroup).toggleClass('in-expanded-scoring-group', isExpanded);
		$unitTitleTh.toggleClass('in-expanded-scoring-group', $courseStatistics.find('td.in-expanded-scoring-group[data-unit-id="' + unitId + '"]').length > 0);
		$courseStatisticsParent.toggleClass('with-expanded-scoring-group', $courseStatistics.find('.in-expanded-scoring-group').length > 0);
	};

	var toggleUnitScoringGroup = function ($scoringGroupTitle) {
		callFunctionForPairedCells(_toggleUnitScoringGroup, $scoringGroupTitle);
	};

	$courseStatistics.find('.expand-scoring-group__link').click(function(e) {
		e.preventDefault();
		var $parent = $(this).closest('.scoring-group-title');
		toggleUnitScoringGroup($parent);
	});

	var disableScoringGroupFilterIfNeeded = function() {
		var countChecked = $('.course-statistics__enable-scoring-group__checkbox:checked').length;
		if (countChecked <= 1)
			$('.course-statistics__enable-scoring-group__checkbox:checked').attr('disabled', 'disabled');
		else
			$('.course-statistics__enable-scoring-group__checkbox').removeAttr('disabled');
	};

	$('.course-statistics__enable-scoring-group__checkbox').change(function () {
		var $self = $(this);
		$self.attr('disabled', 'disabled');
		var isChecked = $self.prop('checked');
		var scoringGroupId = $self.data('scoringGroup');
		var $loadingIcon = $self.closest('.checkbox').find('.loading-icon');
		$loadingIcon.show();

		setTimeout(function() {
			$courseStatistics.find('.scoring-group-title[data-scoring-group="' + scoringGroupId + '"]').each(function() {
				/* Collapse if expanded */
				if ($(this).data('expanded'))
					toggleUnitScoringGroup($(this));

				var unitId = $(this).data('unitId');
				var $unitTitleTh = $('.course-statistics .unit-title[data-unit-id="' + unitId + '"]').closest('th');
				addColspan($unitTitleTh, isChecked ? 1 : -1);
			});

			var filterByScoringGroup = '[data-scoring-group="' + scoringGroupId + '"]';
			$('.course-statistics .scoring-group-title' + filterByScoringGroup).toggle(isChecked);
			$('.course-statistics .scoring-group-score' + filterByScoringGroup).toggle(isChecked);
			$('.course-statistics .scoring-group-max-score' + filterByScoringGroup).toggle(isChecked);

			disableScoringGroupFilterIfNeeded();
			$loadingIcon.hide();
			$self.removeAttr('disabled');
		}, 0);
	});

	var compareAsIntsAndStrings = function (firstValue, secondValue, order) {
		var firstValueInt = parseInt(firstValue);
		var secondValueInt = parseInt(secondValue);
		if (firstValueInt < secondValueInt) return order === 'asc' ? -1 : 1;
		if (firstValueInt > secondValueInt) return order === 'asc' ? 1 : -1;
		if (firstValue.toLowerCase() < secondValue.toLowerCase()) return order === 'asc' ? -1 : 1;
		if (firstValue.toLowerCase() > secondValue.toLowerCase()) return order === 'asc' ? 1 : -1;
		return 0;
	}

	var compareByKeyFunction = function ($first, $second, keyFunction, order) {
		var firstValue = keyFunction($first);
		var secondValue = keyFunction($second);
		return compareAsIntsAndStrings(firstValue, secondValue, order);
	}

	/* Table sorting */
	var sortTable = function ($table, sortingFunctions, groupingFunction) {
		var $groupHeaders = $table.find('tbody tr.group-header-row');
		var groupHeadersByFunctionValue = {};
		if (groupingFunction) {
			$groupHeaders.show();
			$groupHeaders.each(function() {
				groupHeadersByFunctionValue[groupingFunction($(this))] = this;
			});
		} else {
			/* Hide group headers if grouping is disabled */
			$groupHeaders.hide();
		}
		var rows = $table.find('tbody tr.student').get();

		rows.sort(function (first, second) {
			var $firstRow = $(first);
			var $secondRow = $(second);
			var compare;
			if (groupingFunction) {
				var valueGroupingFunction = function(element) {
					return $(groupHeadersByFunctionValue[groupingFunction(element)]).text();
				};
				compare = compareByKeyFunction($firstRow, $secondRow, valueGroupingFunction, 'asc');
				if (compare !== 0)
					return compare;
			}
			for (var idx in sortingFunctions) {
				if (sortingFunctions.hasOwnProperty(idx)) {
					var sortingFunction = sortingFunctions[idx];
					compare = sortingFunction($firstRow, $secondRow);
					if (compare !== 0)
						return compare;
				}
			}
			return 0;
		});

		var $tbody = $table.children('tbody');
		var prevGroupingValue = undefined;
		var alreadyInsertedRowsIds = [];
		var rowsToAppend = [];
		$.each(rows, function (index, row) {
			var $row = $(row);

			if (groupingFunction) {
				var currentGroupingValue = groupingFunction($row);
				if (prevGroupingValue !== currentGroupingValue) {
					//$tbody.append(groupHeadersByFunctionValue[currentGroupingValue]);
                    rowsToAppend.push(groupHeadersByFunctionValue[currentGroupingValue]);
				}
				prevGroupingValue = currentGroupingValue;
			}

			/* If grouping is enabled, hide and ignore duplicate rows */
			var rowId = $row.data('user-id');
			if (!groupingFunction && alreadyInsertedRowsIds.indexOf(rowId) >= 0) {
				$row.hide();
				// $stickyColumnRow.hide();
				return;
			}
			alreadyInsertedRowsIds.push(rowId);

            rowsToAppend.push(row);            
		});

        /* Append all rows to sorted table and make them visible */		
		$tbody.append(rowsToAppend);
		$(rowsToAppend).show();
        
		setTimeout(function() {
            requestAnimationFrame(function () {
            	rerenderStickyColumn($courseStatistics, documentHeaderHeight);
			});
		}, 0);
	};

	var getCompareFunction = function(filter, order) {
		return function ($firstRow, $secondRow) {
			var firstValue = $firstRow.children('td' + filter).text().toUpperCase();
			var secondValue = $secondRow.children('td' + filter).text().toUpperCase();

			return compareAsIntsAndStrings(firstValue, secondValue, order);
		}
	};

	var changeOrder = function(order) {
		return order === 'desc' ? 'asc' : 'desc';
	};

	var getSortingFunction = function($th, order) {
		/* Ordering by name*/
		if ($th.hasClass('student-name'))
			return getCompareFunction('.student-name', order);
		
		/* Ordering by score */
		var unitId = $th.data('unitId');
		var scoringGroupId = $th.data('scoringGroup');
		var slideId = $th.data('slideId');
		var filter = '[data-scoring-group="' + scoringGroupId + '"][data-unit-id="' + unitId + '"]';
		if (slideId)
			filter += '[data-slide-id="' + slideId + '"]';

		return getCompareFunction(filter, order);
	};

	var getCurrentSortingFunctions = function() {
		var $oldSortingThs = $courseStatistics.find('thead th[data-sorter="true"].sorted');
		$oldSortingThs.sort(function (first, second) {
			var firstValue = $(first).data('sorting-index');
			var secondValue = $(second).data('sorting-index');
			if (firstValue < secondValue) return -1;
			if (firstValue > secondValue) return 1;
			return 0;
		});
		return $.map($oldSortingThs, function (el) { return getSortingFunction($(el), $(el).data('order')); });
	};

	var groupingFunction = function ($row) { return $row.data('group'); };

	$('#grouping-by-group').change(function () {
		var $self = $(this);
		$self.attr('disabled', 'disabled');
		var isGrouppingEnabled = $self.is(':checked');
		var $loadingIcon = $self.closest('.checkbox').find('.loading-icon').show();
		var sortingFunctions = getCurrentSortingFunctions();
		setTimeout(function() {
			sortTable($courseStatistics, sortingFunctions, isGrouppingEnabled ? groupingFunction : undefined);
			$loadingIcon.hide();
			$self.removeAttr('disabled');
		}, 0);
	});

	$('#only-full-scores').change(function () {
		var $self = $(this);
		var $loadingIcon = $self.closest('.checkbox').find('.loading-icon').show();
		$self.attr('disabled', 'disabled');
		setTimeout(function() {
			$courseStatistics.toggleClass('only-full-scores', $self.is(':checked'));
			$courseStatistics.find('[data-not-only-full-value]').each(function() {
				var $self = $(this);
				var text = $self.text();
				$self.text($self.data('notOnlyFullValue'));
				$self.data('notOnlyFullValue', text);
			});
			$self.removeAttr('disabled');
			$loadingIcon.hide();
		},  0);
	});
	
	$courseStatistics.find('thead th[data-sorter="true"]').click(function (e) {
		/* Original $courseStatistics and it's pair from sticky header */
		var $courseStatisticsAll = $('.course-statistics');

		/* Cancel event if user clicked on link */
		if ($(e.target).closest('a').length > 0)
			return;

		e.preventDefault();
		window.getSelection().removeAllRanges();

		var $self = $(this);
		var $selfWithPair = $self.add($('.course-statistics thead th').filter('[data-random-id=' + $self.data('randomId') + ']'))
		/* Exclude current column from sorting */
		$selfWithPair.removeClass('sorted sorted-asc sorted-desc');

		var isGrouppingEnabled = $('#grouping-by-group').is(':checked');

		var sortingFunctions = [];
		if (e.shiftKey) {
			sortingFunctions = getCurrentSortingFunctions();
		} else {
			$courseStatisticsAll.find('thead th[data-sorter="true"].sorted').data('order', undefined).removeClass('sorted sorted-asc sorted-desc');
		}
		/* Find maximum of sorting index of current sorting columns */
		var maxSortingIndex = 0;
		$courseStatistics.find('thead th[data-sorter="true"].sorted').each(function() {
			if ($(this).data('sorting-index') > maxSortingIndex)
				maxSortingIndex = $(this).data('sorting-index');
		});

		var order = changeOrder($self.data('order'));
		sortingFunctions.push(getSortingFunction($self, order));
		$selfWithPair.data('order', order).data('sorting-index', maxSortingIndex + 1).addClass('sorted sorted-' + order);
		
		/* Run in another thread */
		setTimeout(function() {
			sortTable($courseStatistics, sortingFunctions, isGrouppingEnabled ? groupingFunction : undefined);
		}, 0);
	});

	if ($courseStatistics.length > 0) {
		disableScoringGroupFilterIfNeeded();

		setTimeout(function() {
			/* Init sticky header. Should be at the end of the file because all event listeners should be set already */
			rerenderStickyHeaderAndColumn($courseStatistics, documentHeaderHeight);

			$(document).scroll(function () {
				relocateStickyHeaderAndColumn($courseStatistics, $stickyHeader, $stickyColumn, documentHeaderHeight);
			});
		}, 0);
	}
});