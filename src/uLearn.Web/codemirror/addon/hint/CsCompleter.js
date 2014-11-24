function CsCompleter(keywords) {
	this.keywords = keywords;

	this.methods = (" WriteLine Write Format Substring Length Parse TryParse ToString Split Join Round Min Max Ceiling Floor"
	+ " Add Contains ContainsKey Directory FullName Extension ToLower"
	+ " Where Select SelectMany ToArray ToList ToDictionary ToLookup Join Zip Aggregate GroupBy OrderBy OrderByDescending ThenBy ThenByDescending ").split(" ");

	this.types = ['int', 'string', 'double', 'Console', 'Math', 'long', 'Boolean', 'double', 'Enumerable', 'Array', 'List', 'Dictionary', 'char'];

	this.dictWithStatic = [];
	this.dictWithStatic['int'] = ['MaxValue', 'MinValue', 'Parse', 'TryParse'];
	this.dictWithStatic['string'] = ['Compare', 'CompareOrdinal', 'Concat', 'Copy', 'Empty', 'Equals', 'Format', 'Intern', 'IsInterned', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'Join'];
	this.dictWithStatic['double'] = ['Epsilon', 'IsInfinity', 'IsNaN', 'IsNegativeInfinity', 'IsPositiveInfinity', 'MaxValue', 'MinValue', 'NaN', 'NegativeInfinity', 'Parse', 'PositiveInfinity', 'TryParse'];
	this.dictWithStatic['Console'] = ['BackgroundColor', 'Beep', 'BufferHeight', 'BufferWidth', 'CapsLock', 'Clear', 'CursorLeft', 'CursorSize', 'CursorTop', 'CursorVisible', 'Error', 'ForegroundColor', 'In', 'InputEncoding', 'IsErrorRedirected', 'IsInputRedirected', 'IsOutputRedirected', 'KeyAvailable', 'LargestWindowHeight', 'LargestWindowWidth', 'MoveBufferArea', 'NumberLock', 'OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput', 'Out', 'OutputEncoding', 'Read', 'ReadKey', 'ReadLine', 'ResetColor', 'SetBufferSize', 'SetCursorPosition', 'SetError', 'SetIn', 'SetOut', 'SetWindowPosition', 'SetWindowSize', 'Title', 'TreatControlCAsInput', 'WindowHeight', 'WindowLeft', 'WindowTop', 'WindowWidth', 'Write', 'WriteLine'];
	this.dictWithStatic['Math'] = ['Abs', 'Acos', 'Asin', 'Atan', 'Atan2', 'BigMul', 'Ceiling', 'Cos', 'Cosh', 'DivRem', 'E', 'Exp', 'Floor', 'IEEERemainder', 'Log', 'Log10', 'Max', 'Min', 'PI', 'Pow', 'Round', 'Sign', 'Sin', 'Sinh', 'Sqrt', 'Tan', 'Tanh', 'Truncate'];
	this.dictWithStatic['long'] = ['MaxValue', 'MinValue', 'Parse', 'TryParse'];
	this.dictWithStatic['Boolean'] = ['FalseString', 'Parse', 'TrueString', 'TryParse'];
	this.dictWithStatic['double'] = ['Epsilon', 'IsInfinity', 'IsNaN', 'IsNegativeInfinity', 'IsPositiveInfinity', 'MaxValue', 'MinValue', 'NaN', 'NegativeInfinity', 'Parse', 'PositiveInfinity', 'TryParse'];
	this.dictWithStatic['Enumerable'] = ['Aggregate', 'All', 'Any', 'AsEnumerable', 'Average', 'Cast', 'Concat', 'Contains', 'Count', 'DefaultIfEmpty', 'Distinct', 'ElementAt', 'ElementAtOrDefault', 'Empty', 'Except', 'First', 'FirstOrDefault', 'GroupBy', 'GroupJoin', 'Intersect', 'Join', 'Last', 'LastOrDefault', 'LongCount', 'Max', 'Min', 'OfType', 'OrderBy', 'OrderByDescending', 'Range', 'Repeat', 'Reverse', 'Select', 'SelectMany', 'SequenceEqual', 'Single', 'SingleOrDefault', 'Skip', 'SkipWhile', 'Sum', 'Take', 'TakeWhile', 'ThenBy', 'ThenByDescending', 'ToArray', 'ToDictionary', 'ToList', 'ToLookup', 'Union', 'Where', 'Zip'];
	this.dictWithStatic['Array'] = ['AsReadOnly', 'BinarySearch', 'Clear', 'ConstrainedCopy', 'ConvertAll', 'Copy', 'CreateInstance', 'Exists', 'Find', 'FindAll', 'FindIndex', 'FindLast', 'FindLastIndex', 'ForEach', 'IndexOf', 'LastIndexOf', 'Resize', 'Reverse', 'Sort', 'TrueForAll'];
	this.dictWithStatic['List'] = [];
	this.dictWithStatic['Dictionary'] = [];
	this.dictWithStatic['char'] = ['ConvertFromUtf32', 'ConvertToUtf32', 'GetNumericValue', 'GetUnicodeCategory', 'IsControl', 'IsDigit', 'IsHighSurrogate', 'IsLetter', 'IsLetterOrDigit', 'IsLower', 'IsLowSurrogate', 'IsNumber', 'IsPunctuation', 'IsSeparator', 'IsSurrogate', 'IsSurrogatePair', 'IsSymbol', 'IsUpper', 'IsWhiteSpace', 'MaxValue', 'MinValue', 'Parse', 'ToLower', 'ToLowerInvariant', 'ToString', 'ToUpper', 'ToUpperInvariant', 'TryParse'];


	this.dictWithNonStatic = [];
	this.dictWithNonStatic['int'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['string'] = ['Chars', 'Clone', 'CompareTo', 'Contains', 'CopyTo', 'EndsWith', 'Equals', 'GetEnumerator', 'GetHashCode', 'GetType', 'GetTypeCode', 'IndexOf', 'IndexOfAny', 'Insert', 'IsNormalized', 'LastIndexOf', 'LastIndexOfAny', 'Length', 'Normalize', 'PadLeft', 'PadRight', 'Remove', 'Replace', 'Split', 'StartsWith', 'Substring', 'ToCharArray', 'ToLower', 'ToLowerInvariant', 'ToString', 'ToUpper', 'ToUpperInvariant', 'Trim', 'TrimEnd', 'TrimStart'];
	this.dictWithNonStatic['double'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['Console'] = ['Equals', 'GetHashCode', 'GetType', 'ToString'];
	this.dictWithNonStatic['Math'] = ['Equals', 'GetHashCode', 'GetType', 'ToString'];
	this.dictWithNonStatic['long'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['Boolean'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['double'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];
	this.dictWithNonStatic['Enumerable'] = ['Equals', 'GetHashCode', 'GetType', 'ToString'];
	this.dictWithNonStatic['Array'] = ['Clone', 'CopyTo', 'Equals', 'GetEnumerator', 'GetHashCode', 'GetLength', 'GetLongLength', 'GetLowerBound', 'GetType', 'GetUpperBound', 'GetValue', 'Initialize', 'IsFixedSize', 'IsReadOnly', 'IsSynchronized', 'Length', 'LongLength', 'Rank', 'SetValue', 'SyncRoot', 'ToString'];
	this.dictWithNonStatic['List'] = ['Add', 'AddRange', 'AsReadOnly', 'BinarySearch', 'Capacity', 'Clear', 'Contains', 'ConvertAll', 'CopyTo', 'Count', 'Equals', 'Exists', 'Find', 'FindAll', 'FindIndex', 'FindLast', 'FindLastIndex', 'ForEach', 'GetEnumerator', 'GetHashCode', 'GetRange', 'GetType', 'IndexOf', 'Insert', 'InsertRange', 'Item', 'LastIndexOf', 'Remove', 'RemoveAll', 'RemoveAt', 'RemoveRange', 'Reverse', 'Sort', 'ToArray', 'ToString', 'TrimExcess', 'TrueForAll'];
	this.dictWithNonStatic['Dictionary'] = ['Add', 'Clear', 'Comparer', 'ContainsKey', 'ContainsValue', 'Count', 'Equals', 'GetEnumerator', 'GetHashCode', 'GetObjectData', 'GetType', 'Item', 'Keys', 'OnDeserialization', 'Remove', 'ToString', 'TryGetValue', 'Values'];
	this.dictWithNonStatic['char'] = ['CompareTo', 'Equals', 'GetHashCode', 'GetType', 'GetTypeCode', 'ToString'];


	this.returnTypeDict = [];
	this.returnTypeDict['int'] = ['Parse', 'MaxValue', 'MinValue', 'Compare', 'CompareOrdinal', 'Read', 'BufferHeight', 'BufferWidth', 'WindowHeight', 'WindowWidth', 'LargestWindowWidth', 'LargestWindowHeight', 'WindowLeft', 'WindowTop', 'CursorLeft', 'CursorTop', 'CursorSize', 'Abs', 'Max', 'Min', 'Sign', 'DivRem', 'Count', 'Sum', 'BinarySearch', 'FindIndex', 'FindLastIndex', 'IndexOf', 'LastIndexOf', 'ConvertToUtf32', 'CompareTo', 'GetHashCode', 'IndexOfAny', 'LastIndexOfAny', 'Length', 'GetLength', 'GetUpperBound', 'GetLowerBound', 'Rank', 'RemoveAll', 'Capacity'];
	this.returnTypeDict['Boolean'] = ['TryParse', 'Equals', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'IsInfinity', 'IsPositiveInfinity', 'IsNegativeInfinity', 'IsNaN', 'IsInputRedirected', 'IsOutputRedirected', 'IsErrorRedirected', 'CursorVisible', 'KeyAvailable', 'NumberLock', 'CapsLock', 'TreatControlCAsInput', 'Parse', 'SequenceEqual', 'Any', 'All', 'Contains', 'Exists', 'TrueForAll', 'IsDigit', 'IsLetter', 'IsWhiteSpace', 'IsUpper', 'IsLower', 'IsPunctuation', 'IsLetterOrDigit', 'IsControl', 'IsNumber', 'IsSeparator', 'IsSurrogate', 'IsSymbol', 'IsHighSurrogate', 'IsLowSurrogate', 'IsSurrogatePair', 'IsNormalized', 'EndsWith', 'StartsWith', 'IsReadOnly', 'IsFixedSize', 'IsSynchronized', 'Remove', 'ContainsKey', 'TryGetValue', 'ContainsValue'];
	this.returnTypeDict['string'] = ['Join', 'Format', 'Copy', 'Concat', 'Intern', 'IsInterned', 'Empty', 'ReadLine', 'Title', 'TrueString', 'FalseString', 'ToString', 'ConvertFromUtf32', 'Split', 'Substring', 'Trim', 'TrimStart', 'TrimEnd', 'Normalize', 'PadLeft', 'PadRight', 'ToLower', 'ToLowerInvariant', 'ToUpper', 'ToUpperInvariant', 'Insert', 'Replace', 'Remove'];
	this.returnTypeDict['double'] = ['Parse', 'MinValue', 'MaxValue', 'Epsilon', 'NegativeInfinity', 'PositiveInfinity', 'NaN', 'Acos', 'Asin', 'Atan', 'Atan2', 'Ceiling', 'Cos', 'Cosh', 'Floor', 'Sin', 'Tan', 'Sinh', 'Tanh', 'Round', 'Truncate', 'Sqrt', 'Log', 'Log10', 'Exp', 'Pow', 'IEEERemainder', 'Abs', 'Max', 'Min', 'PI', 'E', 'Sum', 'Average', 'GetNumericValue'];
	this.returnTypeDict['Void'] = ['Beep', 'Clear', 'ResetColor', 'MoveBufferArea', 'SetBufferSize', 'SetWindowSize', 'SetWindowPosition', 'SetCursorPosition', 'SetIn', 'SetOut', 'SetError', 'WriteLine', 'Write', 'Resize', 'Copy', 'ConstrainedCopy', 'ForEach', 'Reverse', 'Sort', 'CopyTo', 'SetValue', 'Initialize', 'Add', 'AddRange', 'Insert', 'InsertRange', 'RemoveAt', 'RemoveRange', 'TrimExcess', 'GetObjectData', 'OnDeserialization'];
	this.returnTypeDict['ConsoleKeyInfo'] = ['ReadKey'];
	this.returnTypeDict['IO.Stream'] = ['OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput'];
	this.returnTypeDict['IO.TextReader'] = ['In'];
	this.returnTypeDict['IO.TextWriter'] = ['Out', 'Error'];
	this.returnTypeDict['Text.Encoding'] = ['InputEncoding', 'OutputEncoding'];
	this.returnTypeDict['ConsoleColor'] = ['BackgroundColor', 'ForegroundColor'];
	this.returnTypeDict['Decimal'] = ['Ceiling', 'Floor', 'Round', 'Truncate', 'Abs', 'Max', 'Min', 'Sum', 'Average'];
	this.returnTypeDict['SByte'] = ['Abs', 'Max', 'Min'];
	this.returnTypeDict['long'] = ['Abs', 'Max', 'Min', 'BigMul', 'DivRem', 'Parse', 'MaxValue', 'MinValue', 'LongCount', 'Sum', 'GetLongLength', 'LongLength'];
	this.returnTypeDict['Byte'] = ['Max', 'Min'];
	this.returnTypeDict['Enumerable'] = ['Sum', 'Min', 'Max', 'Average', 'Except', 'Reverse', 'AsEnumerable', 'ToArray', 'ToLookup', 'DefaultIfEmpty', 'OfType', 'Cast', 'Range', 'Repeat', 'Empty', 'Where', 'Select', 'SelectMany', 'Take', 'TakeWhile', 'Skip', 'SkipWhile', 'Join', 'GroupJoin', 'OrderBy', 'OrderByDescending', 'ThenBy', 'ThenByDescending', 'GroupBy', 'Concat', 'Zip', 'Distinct', 'Union', 'Intersect', 'AsReadOnly', 'ConvertAll', 'FindAll', 'Comparer'];
	this.returnTypeDict['TSource'] = ['Min', 'Max', 'First', 'FirstOrDefault', 'Last', 'LastOrDefault', 'Single', 'SingleOrDefault', 'ElementAt', 'ElementAtOrDefault', 'Aggregate'];
	this.returnTypeDict['TResult'] = ['Min', 'Max', 'Aggregate'];
	this.returnTypeDict['List'] = ['ToList', 'ConvertAll', 'FindAll', 'GetEnumerator', 'GetRange'];
	this.returnTypeDict['Dictionary'] = ['ToDictionary', 'GetEnumerator', 'Keys', 'Values'];
	this.returnTypeDict['TAccumulate'] = ['Aggregate'];
	this.returnTypeDict['Array'] = ['CreateInstance'];
	this.returnTypeDict['T'] = ['Find', 'FindLast', 'Item'];
	this.returnTypeDict['char'] = ['Parse', 'ToUpper', 'ToUpperInvariant', 'ToLower', 'ToLowerInvariant', 'MaxValue', 'MinValue', 'ToCharArray', 'GetEnumerator', 'Chars'];
	this.returnTypeDict['Globalization.UnicodeCategory'] = ['GetUnicodeCategory'];
	this.returnTypeDict['TypeCode'] = ['GetTypeCode'];
	this.returnTypeDict['Type'] = ['GetType'];
	this.returnTypeDict['Object'] = ['Clone', 'GetValue', 'SyncRoot'];
	this.returnTypeDict['Collections.IEnumerator'] = ['GetEnumerator'];
	this.returnTypeDict['TValue'] = ['Item'];


	this.getCompletions = function (beforeDot, start, afterDot) {
		var res;
		if (afterDot) {
			res = this.AutocompliteAfterDotWords(beforeDot, start);
		} else {
			if (beforeDot != undefined) {
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
	this.prefix = typeof this.prefix == "undefined" ? '' : prefix.toLowerCase();
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
