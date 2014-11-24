function CsCompleter(keywords) {
	this.keywords = keywords;

	this.methods = (" WriteLine Write Format Substring Length Parse TryParse ToString Split Join Round Min Max Ceiling Floor"
	+ " Add Contains ContainsKey Directory FullName Extension ToLower"
	+ " Where Select SelectMany ToArray ToList ToDictionary ToLookup Join Zip Aggregate GroupBy OrderBy OrderByDescending ThenBy ThenByDescending ").split(" ");

	this.types = ['int', 'string', 'double', 'Console', 'Math', 'long', 'bool', 'double', 'Enumerable', 'Array', 'List', 'Dictionary', 'char'];

	this.synonym = { 'ReadOnlyCollection': 'Enumerable', 'Lookup': 'Enumerable', 'Int16': 'int', 'Int32': 'int', 'Int64': 'long', 'String': 'string', 'Single': 'double', 'Double': 'double', 'Decimal': 'double', 'Boolean': 'bool', 'Char': 'char', '[]': 'Array' };

	this.dictWithStatic = [];
	this.dictWithStatic['int'] = ['MaxValue', 'MinValue', 'Parse', 'TryParse'];
	this.dictWithStatic['string'] = ['Compare', 'CompareOrdinal', 'Concat', 'Copy', 'Empty', 'Equals', 'Format', 'Intern', 'IsInterned', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'Join'];
	this.dictWithStatic['double'] = ['Epsilon', 'IsInfinity', 'IsNaN', 'IsNegativeInfinity', 'IsPositiveInfinity', 'MaxValue', 'MinValue', 'NaN', 'NegativeInfinity', 'Parse', 'PositiveInfinity', 'TryParse'];
	this.dictWithStatic['Console'] = ['BackgroundColor', 'Beep', 'BufferHeight', 'BufferWidth', 'CapsLock', 'Clear', 'CursorLeft', 'CursorSize', 'CursorTop', 'CursorVisible', 'Error', 'ForegroundColor', 'In', 'InputEncoding', 'IsErrorRedirected', 'IsInputRedirected', 'IsOutputRedirected', 'KeyAvailable', 'LargestWindowHeight', 'LargestWindowWidth', 'MoveBufferArea', 'NumberLock', 'OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput', 'Out', 'OutputEncoding', 'Read', 'ReadKey', 'ReadLine', 'ResetColor', 'SetBufferSize', 'SetCursorPosition', 'SetError', 'SetIn', 'SetOut', 'SetWindowPosition', 'SetWindowSize', 'Title', 'TreatControlCAsInput', 'WindowHeight', 'WindowLeft', 'WindowTop', 'WindowWidth', 'Write', 'WriteLine'];
	this.dictWithStatic['Math'] = ['Abs', 'Acos', 'Asin', 'Atan', 'Atan2', 'BigMul', 'Ceiling', 'Cos', 'Cosh', 'DivRem', 'E', 'Exp', 'Floor', 'IEEERemainder', 'Log', 'Log10', 'Max', 'Min', 'PI', 'Pow', 'Round', 'Sign', 'Sin', 'Sinh', 'Sqrt', 'Tan', 'Tanh', 'Truncate'];
	this.dictWithStatic['long'] = ['MaxValue', 'MinValue', 'Parse', 'TryParse'];
	this.dictWithStatic['bool'] = ['FalseString', 'Parse', 'TrueString', 'TryParse'];
	this.dictWithStatic['double'] = ['Epsilon', 'IsInfinity', 'IsNaN', 'IsNegativeInfinity', 'IsPositiveInfinity', 'MaxValue', 'MinValue', 'NaN', 'NegativeInfinity', 'Parse', 'PositiveInfinity', 'TryParse'];
	this.dictWithStatic['Enumerable'] = ['Aggregate', 'All', 'Any', 'AsEnumerable', 'Average', 'Cast', 'Concat', 'Contains', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Empty', 'Except', 'First', 'FirstOrDefault', 'GroupBy', 'GroupJoin', 'Intersect', 'Join', 'Last', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OrderBy', 'OrderByDescending', 'Range', 'Repeat', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sum', 'Take', 'TakeWhile', 'ThenBy', 'ThenByDescending', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'Union', 'Where', 'Zip'];
	this.dictWithStatic['Array'] = ['AsReadOnly', 'BinarySearch', 'Clear', 'ConstrainedCopy', 'ConvertAll', 'Copy', 'CreateInstance', 'Exists', 'Find', 'FindAll', 'FindIndex', 'FindLast', 'FindLastIndex', 'ForEach', 'IndexOf', 'LastIndexOf', 'Resize', 'Reverse', 'Sort', 'TrueForAll'];
	this.dictWithStatic['List'] = [];
	this.dictWithStatic['Dictionary'] = [];
	this.dictWithStatic['char'] = ['ConvertFromUtf32', 'ConvertToUtf32', 'GetNumericValue', 'GetUnicodeCategory', 'IsControl', 'IsDigit', 'IsHighSurrogate', 'IsLetter', 'IsLetterOrDigit', 'IsLower', 'IsLowSurrogate', 'IsNumber', 'IsPunctuation', 'IsSeparator', 'IsSurrogate', 'IsSurrogatePair', 'IsSymbol', 'IsUpper', 'IsWhiteSpace', 'MaxValue', 'MinValue', 'Parse', 'ToLower', 'ToLowerInvariant', 'ToString', 'ToUpper', 'ToUpperInvariant', 'TryParse'];


	this.dictWithNonStatic = [];
	this.dictWithNonStatic['int'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['string'] = ['Aggregate', 'All', 'Any', 'AsEnumerable', 'AsParallel', 'AsQueryable', 'Average', 'Cast', 'Chars', 'Clone', 'CompareTo', 'Concat', 'Contains', 'CopyTo', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'EndsWith', 'Equals', 'Except', 'First', 'FirstOrDefault', 'GetEnumerator', 'GetHashCode', 'GetType', 'GetTypeCode', 'GroupBy', 'GroupJoin', 'IndexOf', 'IndexOfAny', 'Insert', 'Intersect', 'IsNormalized', 'Join', 'Last', 'LastIndexOf', 'LastIndexOfAny', 'LastOrDefault', 'Length', 'LongCount', 'Max', 'Min', 'Normalize', 'OfType', 'OrderBy', 'OrderByDescending', 'PadLeft', 'PadRight', 'Remove', 'Replace', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Split', 'StartsWith', 'Substring', 'Sum', 'Take', 'TakeWhile', 'ToArray', 'ToCharArray', 'ToDictionary', 'ToList', 'ToLookup', 'ToLower', 'ToLowerInvariant', 'ToString', 'ToUpper', 'ToUpperInvariant', 'Trim', 'TrimEnd', 'TrimStart', 'Union', 'Where', 'Zip'];
	this.dictWithNonStatic['double'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['Console'] = ['Equals', 'GetHashCode', 'GetType', 'ToString'];
	this.dictWithNonStatic['Math'] = ['Equals', 'GetHashCode', 'GetType', 'ToString'];
	this.dictWithNonStatic['long'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['bool'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['double'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['Enumerable'] = ['Equals', 'GetHashCode', 'GetType', 'ToString'];
	this.dictWithNonStatic['Array'] = ['AsParallel', 'AsQueryable', 'Cast', 'Clone', 'CopyTo', 'Equals', 'GetEnumerator', 'GetHashCode', 'GetLength', 'GetLongLength', 'GetLowerBound', 'GetType', 'GetUpperBound', 'GetValue', 'Initialize', 'IsFixedSize', 'IsReadOnly', 'IsSynchronized', 'Length', 'LongLength', 'OfType', 'Rank', 'SetValue', 'SyncRoot', 'ToString'];
	this.dictWithNonStatic['List'] = ['Add', 'AddRange', 'Aggregate', 'All', 'Any', 'AsEnumerable', 'AsParallel', 'AsQueryable', 'AsReadOnly', 'Average', 'BinarySearch', 'Capacity', 'Cast', 'Clear', 'Concat', 'Contains', 'ConvertAll', 'CopyTo', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Equals', 'Except', 'Exists', 'Find', 'FindAll', 'FindIndex', 'FindLast', 'FindLastIndex', 'First', 'FirstOrDefault', 'ForEach', 'GetEnumerator', 'GetHashCode', 'GetRange', 'GetType', 'GroupBy', 'GroupJoin', 'IndexOf', 'Insert', 'InsertRange', 'Intersect', 'Item', 'Join', 'Last', 'LastIndexOf', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OrderBy', 'OrderByDescending', 'Remove', 'RemoveAll', 'RemoveAt', 'RemoveRange', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sort', 'Sum', 'Take', 'TakeWhile', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'ToString', 'TrimExcess', 'TrueForAll', 'Union', 'Where', 'Zip'];
	this.dictWithNonStatic['Dictionary'] = ['Add', 'Aggregate', 'All', 'Any', 'AsEnumerable', 'AsParallel', 'AsQueryable', 'Average', 'Cast', 'Clear', 'Comparer', 'Concat', 'Contains', 'ContainsKey', 'ContainsValue', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Equals', 'Except', 'First', 'FirstOrDefault', 'GetEnumerator', 'GetHashCode', 'GetObjectData', 'GetType', 'GroupBy', 'GroupJoin', 'Intersect', 'Item', 'Join', 'Keys', 'Last', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OnDeserialization', 'OrderBy', 'OrderByDescending', 'Remove', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sum', 'Take', 'TakeWhile', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'ToString', 'TryGetValue', 'Union', 'Values', 'Where', 'Zip'];
	this.dictWithNonStatic['char'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];


	this.returnTypeDict = [];
	this.returnTypeDict['int'] = ['Parse', 'MaxValue', 'MinValue', 'Compare', 'CompareOrdinal', 'Read', 'BufferHeight', 'BufferWidth', 'WindowHeight', 'WindowWidth', 'LargestWindowWidth', 'LargestWindowHeight', 'WindowLeft', 'WindowTop', 'CursorLeft', 'CursorTop', 'CursorSize', 'Abs', 'Max', 'Min', 'Sign', 'DivRem', 'Count', 'Sum', 'BinarySearch', 'FindIndex', 'FindLastIndex', 'IndexOf', 'LastIndexOf', 'ConvertToUtf32', 'CompareTo', 'GetHashCode', 'IndexOfAny', 'LastIndexOfAny', 'Length', 'GetLength', 'GetUpperBound', 'GetLowerBound', 'Rank', 'RemoveAll', 'Capacity'];
	this.returnTypeDict['bool'] = ['TryParse', 'Equals', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'IsInfinity', 'IsPositiveInfinity', 'IsNegativeInfinity', 'IsNaN', 'IsInputRedirected', 'IsOutputRedirected', 'IsErrorRedirected', 'CursorVisible', 'KeyAvailable', 'NumberLock', 'CapsLock', 'TreatControlCAsInput', 'Parse', 'SequenceEqual', 'Any', 'All', 'Contains', 'Exists', 'TrueForAll', 'IsDigit', 'IsLetter', 'IsWhiteSpace', 'IsUpper', 'IsLower', 'IsPunctuation', 'IsLetterOrDigit', 'IsControl', 'IsNumber', 'IsSeparator', 'IsSurrogate', 'IsSymbol', 'IsHighSurrogate', 'IsLowSurrogate', 'IsSurrogatePair', 'IsNormalized', 'EndsWith', 'StartsWith', 'IsReadOnly', 'IsFixedSize', 'IsSynchronized', 'Remove', 'ContainsKey', 'TryGetValue', 'ContainsValue'];
	this.returnTypeDict['string'] = ['Join', 'Format', 'Copy', 'Concat', 'Intern', 'IsInterned', 'Empty', 'ReadLine', 'Title', 'TrueString', 'FalseString', 'ToString', 'ConvertFromUtf32', 'Split', 'Substring', 'Trim', 'TrimStart', 'TrimEnd', 'Normalize', 'PadLeft', 'PadRight', 'ToLower', 'ToLowerInvariant', 'ToUpper', 'ToUpperInvariant', 'Insert', 'Replace', 'Remove'];
	this.returnTypeDict['double'] = ['Parse', 'MinValue', 'MaxValue', 'Epsilon', 'NegativeInfinity', 'PositiveInfinity', 'NaN', 'Acos', 'Asin', 'Atan', 'Atan2', 'Ceiling', 'Cos', 'Cosh', 'Floor', 'Sin', 'Tan', 'Sinh', 'Tanh', 'Round', 'Truncate', 'Sqrt', 'Log', 'Log10', 'Exp', 'Pow', 'IEEERemainder', 'Abs', 'Max', 'Min', 'PI', 'E', 'Sum', 'Average', 'GetNumericValue'];
	this.returnTypeDict['Void'] = ['Beep', 'Clear', 'ResetColor', 'MoveBufferArea', 'SetBufferSize', 'SetWindowSize', 'SetWindowPosition', 'SetCursorPosition', 'SetIn', 'SetOut', 'SetError', 'WriteLine', 'Write', 'Resize', 'Copy', 'ConstrainedCopy', 'ForEach', 'Reverse', 'Sort', 'CopyTo', 'SetValue', 'Initialize', 'Add', 'AddRange', 'Insert', 'InsertRange', 'RemoveAt', 'RemoveRange', 'TrimExcess', 'GetObjectData', 'OnDeserialization'];
	this.returnTypeDict['ConsoleKeyInfo'] = ['ReadKey'];
	this.returnTypeDict['IO.Stream'] = ['OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput'];
	this.returnTypeDict['IO.TextReader'] = ['In'];
	this.returnTypeDict['IO.TextWriter'] = ['Out', 'Error'];
	this.returnTypeDict['Text.Encoding'] = ['InputEncoding', 'OutputEncoding'];
	this.returnTypeDict['ConsoleColor'] = ['BackgroundColor', 'ForegroundColor'];
	this.returnTypeDict['SByte'] = ['Abs', 'Max', 'Min'];
	this.returnTypeDict['long'] = ['Abs', 'Max', 'Min', 'BigMul', 'DivRem', 'Parse', 'MaxValue', 'MinValue', 'LongCount', 'Sum', 'GetLongLength', 'LongLength'];
	this.returnTypeDict['Byte'] = ['Max', 'Min'];
	this.returnTypeDict['TSource'] = ['Min', 'Max', 'First', 'FirstOrDefault', 'Last', 'LastOrDefault', 'Single', 'SingleOrDefault', 'ElementAt', 'ElementAtOrDefault', 'Aggregate'];
	this.returnTypeDict['TResult'] = ['Min', 'Max', 'Aggregate'];
	this.returnTypeDict['Enumerable'] = ['Except', 'Reverse', 'AsEnumerable', 'ToLookup', 'DefaultIfEmpty', 'OfType', 'Cast', 'Range', 'Repeat', 'Empty', 'Where', 'Select', 'SelectMany', 'Take', 'TakeWhile', 'Skip', 'SkipWhile', 'Join', 'GroupJoin', 'OrderBy', 'OrderByDescending', 'ThenBy', 'ThenByDescending', 'GroupBy', 'Concat', 'Zip', 'Distinct', 'Union', 'Intersect', 'AsReadOnly'];
	this.returnTypeDict['Array'] = ['ToArray', 'CreateInstance', 'ConvertAll', 'FindAll'];
	this.returnTypeDict['List'] = ['ToList', 'ConvertAll', 'FindAll', 'GetEnumerator', 'GetRange'];
	this.returnTypeDict['Dictionary'] = ['ToDictionary', 'GetEnumerator', 'Keys', 'Values'];
	this.returnTypeDict['TAccumulate'] = ['Aggregate'];
	this.returnTypeDict['T'] = ['Find', 'FindLast', 'Item'];
	this.returnTypeDict['char'] = ['Parse', 'ToUpper', 'ToUpperInvariant', 'ToLower', 'ToLowerInvariant', 'MaxValue', 'MinValue', 'ToCharArray', 'GetEnumerator', 'Chars'];
	this.returnTypeDict['Globalization.UnicodeCategory'] = ['GetUnicodeCategory'];
	this.returnTypeDict['TypeCode'] = ['GetTypeCode'];
	this.returnTypeDict['Type'] = ['GetType'];
	this.returnTypeDict['Object'] = ['Clone', 'GetValue', 'SyncRoot'];
	this.returnTypeDict['ParallelQuery'] = ['AsParallel'];
	this.returnTypeDict['Queryable'] = ['AsQueryable'];
	this.returnTypeDict['IEnumerator'] = ['GetEnumerator'];
	this.returnTypeDict['IEqualityComparer'] = ['Comparer'];
	this.returnTypeDict['TValue'] = ['Item'];


	this.getCompletions = function (beforeDot, start, afterDot) {
		var res;
		if (afterDot) {
			if (typeof this.synonym[beforeDot] != "undefined")
				beforeDot = this.synonym[beforeDot];
			res = this.AutocompliteAfterDotWords(beforeDot, start);
		} else {
			if (beforeDot != undefined) {
				if (typeof this.synonym[beforeDot] != "undefined")
					beforeDot = this.synonym[beforeDot];
				res = this.AutocompliteAfterDotWords(beforeDot, start);
			} else {
				res = new SamePrefixArray('');
				res.AddAll(this.types);
				res.AddAll(this.keywords);
			}
		}
		return res.getArray().sort();
	}

	this.AutocompliteAfterDotWords = function (beforeDot, start) {
		var found = new SamePrefixArray(start);
		if (arrayContains(this.types, beforeDot)) {
			if (this.dictWithStatic[beforeDot] != undefined) {
				console.log(beforeDot);
				found.AddAll(this.dictWithStatic[beforeDot]);
			}
			else {
				found.AddAll(this.methods);
			}
		} else {
			var isWasFound = false;
			for (var type in this.returnTypeDict) {
				for (var element in this.returnTypeDict[type]) {
					if (beforeDot == this.returnTypeDict[type][element]) {
						if (type == "Void") {
							isWasFound = true;
							break;
						}
						if (this.dictWithNonStatic[type] != undefined) {
							found.AddAll(this.dictWithNonStatic[type]);
							isWasFound = true;
						}
					}
				}
			}
			if (!isWasFound) {
				found.AddAll(this.methods);
			}
		}
		return found;
	}
}

function arrayContains(arr, item) {
	if (!Array.prototype.indexOf) {
		var i = arr.length;
		while (i--) {
			if (arr[i] === item) {
				return true;
			}
		}
		return false;
	}
	return arr.indexOf(item) != -1;
}

function SamePrefixArray(prefix) {
	this.prefix = typeof prefix == "undefined" ? '' : prefix.toLowerCase();
	this.container = [];

	this.AddAll = function(list) {
		for (var i = 0; i < list.length; ++i)
			this.Add(list[i]);
	}
	this.Add = function(str) {
		if (str.toLowerCase().lastIndexOf(this.prefix, 0) == 0 && !arrayContains(this.container, str))
			this.container.push(str);
	}
	this.getArray = function() {
		return this.container;
	}
}
