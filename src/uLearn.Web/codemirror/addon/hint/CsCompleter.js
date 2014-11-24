function CsCompleter(keywords) {
	this.keywords = keywords;

	this.methods = (" WriteLine Write Format Substring Length Parse TryParse ToString Split Join Round Min Max Ceiling Floor"
	+ " Add Contains ContainsKey Directory FullName Extension ToLower"
	+ " Where Select SelectMany ToArray ToList ToDictionary ToLookup Join Zip Aggregate GroupBy OrderBy OrderByDescending ThenBy ThenByDescending ").split(" ");

	this.types = ['int', 'string', 'double', 'Console', 'Math', 'long', 'Boolean', 'double', 'Enumerable', 'Array', 'List'];

	this.dictWithStaticMethods = [];
	this.dictWithStaticMethods['int'] = ['Parse', 'TryParse'];
	this.dictWithStaticMethods['string'] = ['Join', 'Equals', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'Compare', 'CompareOrdinal', 'Format', 'Copy', 'Concat', 'Intern', 'IsInterned'];
	this.dictWithStaticMethods['double'] = ['IsInfinity', 'IsPositiveInfinity', 'IsNegativeInfinity', 'IsNaN', 'Parse', 'TryParse'];
	this.dictWithStaticMethods['Console'] = ['Beep', 'Clear', 'ResetColor', 'MoveBufferArea', 'SetBufferSize', 'SetWindowSize', 'SetWindowPosition', 'SetCursorPosition', 'ReadKey', 'OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput', 'SetIn', 'SetOut', 'SetError', 'Read', 'ReadLine', 'WriteLine', 'Write'];
	this.dictWithStaticMethods['Math'] = ['Acos', 'Asin', 'Atan', 'Atan2', 'Ceiling', 'Cos', 'Cosh', 'Floor', 'Sin', 'Tan', 'Sinh', 'Tanh', 'Round', 'Truncate', 'Sqrt', 'Log', 'Log10', 'Exp', 'Pow', 'IEEERemainder', 'Abs', 'Max', 'Min', 'Sign', 'BigMul', 'DivRem'];
	this.dictWithStaticMethods['long'] = ['Parse', 'TryParse'];
	this.dictWithStaticMethods['Boolean'] = ['Parse', 'TryParse'];
	this.dictWithStaticMethods['double'] = ['IsInfinity', 'IsPositiveInfinity', 'IsNegativeInfinity', 'IsNaN', 'Parse', 'TryParse'];
	this.dictWithStaticMethods['Enumerable'] = ['Sum', 'Min', 'Max', 'Average', 'Except', 'Reverse', 'SequenceEqual', 'AsEnumerable', 'ToArray', 'ToList', 'ToDictionary', 'ToLookup', 'DefaultIfEmpty', 'OfType', 'Cast', 'First', 'FirstOrDefault', 'Last', 'LastOrDefault', 'Single', 'SingleOrDefault', 'ElementAt', 'ElementAtOrDefault', 'Range', 'Repeat', 'Empty', 'Any', 'All', 'Count', 'LongCount', 'Contains', 'Aggregate', 'Where', 'Select', 'SelectMany', 'Take', 'TakeWhile', 'Skip', 'SkipWhile', 'Join', 'GroupJoin', 'OrderBy', 'OrderByDescending', 'ThenBy', 'ThenByDescending', 'GroupBy', 'Concat', 'Zip', 'Distinct', 'Union', 'Intersect'];
	this.dictWithStaticMethods['Array'] = ['AsReadOnly', 'Resize', 'CreateInstance', 'Copy', 'ConstrainedCopy', 'Clear', 'BinarySearch', 'ConvertAll', 'Exists', 'Find', 'FindAll', 'FindIndex', 'FindLast', 'FindLastIndex', 'ForEach', 'IndexOf', 'LastIndexOf', 'Reverse', 'Sort', 'TrueForAll'];


	this.dictWithNonStaticMethods = [];
	this.dictWithNonStaticMethods['int'] = ['CompareTo', 'Equals', 'GetHashCode', 'ToString', 'GetTypeCode', 'GetType'];
	this.dictWithNonStaticMethods['string'] = ['Equals', 'CopyTo', 'ToCharArray', 'GetHashCode', 'Split', 'Substring', 'Trim', 'TrimStart', 'TrimEnd', 'IsNormalized', 'Normalize', 'CompareTo', 'Contains', 'EndsWith', 'IndexOf', 'IndexOfAny', 'LastIndexOf', 'LastIndexOfAny', 'PadLeft', 'PadRight', 'StartsWith', 'ToLower', 'ToLowerInvariant', 'ToUpper', 'ToUpperInvariant', 'ToString', 'Clone', 'Insert', 'Replace', 'Remove', 'GetTypeCode', 'GetEnumerator', 'GetType'];
	this.dictWithNonStaticMethods['double'] = ['CompareTo', 'Equals', 'GetHashCode', 'ToString', 'GetTypeCode', 'GetType'];
	this.dictWithNonStaticMethods['Console'] = ['ToString', 'Equals', 'GetHashCode', 'GetType'];
	this.dictWithNonStaticMethods['Math'] = ['ToString', 'Equals', 'GetHashCode', 'GetType'];
	this.dictWithNonStaticMethods['long'] = ['CompareTo', 'Equals', 'GetHashCode', 'ToString', 'GetTypeCode', 'GetType'];
	this.dictWithNonStaticMethods['Boolean'] = ['GetHashCode', 'ToString', 'Equals', 'CompareTo', 'GetTypeCode', 'GetType'];
	this.dictWithNonStaticMethods['double'] = ['CompareTo', 'Equals', 'GetHashCode', 'ToString', 'GetTypeCode', 'GetType'];
	this.dictWithNonStaticMethods['Enumerable'] = ['ToString', 'Equals', 'GetHashCode', 'GetType'];
	this.dictWithNonStaticMethods['Array'] = ['GetValue', 'SetValue', 'GetLength', 'GetLongLength', 'GetUpperBound', 'GetLowerBound', 'Clone', 'CopyTo', 'GetEnumerator', 'Initialize', 'ToString', 'Equals', 'GetHashCode', 'GetType'];
	this.dictWithNonStaticMethods['List'] = ['Add', 'AddRange', 'AsReadOnly', 'BinarySearch', 'Clear', 'Contains', 'ConvertAll', 'CopyTo', 'Find', 'FindAll', 'FindIndex', 'ForEach', 'GetEnumerator', 'GetRange', 'IndexOf', 'Insert', 'InsertRange', 'Remove', 'RemoveAll', 'RemoveAt', 'RemoveRange', 'Reverse', 'Sort', 'ToArray', 'TrimExcess', 'Exists', 'FindLast', 'FindLastIndex', 'LastIndexOf', 'TrueForAll', 'ToString', 'Equals', 'GetHashCode', 'GetType'];


	this.dictWithProperties = [];
	this.dictWithProperties['string'] = ['Chars', 'Length'];
	this.dictWithProperties['Console'] = ['IsInputRedirected', 'IsOutputRedirected', 'IsErrorRedirected', 'In', 'Out', 'Error', 'InputEncoding', 'OutputEncoding', 'BackgroundColor', 'ForegroundColor', 'BufferHeight', 'BufferWidth', 'WindowHeight', 'WindowWidth', 'LargestWindowWidth', 'LargestWindowHeight', 'WindowLeft', 'WindowTop', 'CursorLeft', 'CursorTop', 'CursorSize', 'CursorVisible', 'Title', 'KeyAvailable', 'NumberLock', 'CapsLock', 'TreatControlCAsInput'];
	this.dictWithProperties['Array'] = ['Length', 'LongLength', 'Rank', 'SyncRoot', 'IsReadOnly', 'IsFixedSize', 'IsSynchronized'];
	this.dictWithProperties['List'] = ['Capacity', 'Count', 'Item'];


	this.dictWithConstants = [];
	this.dictWithConstants['int'] = ['MaxValue', 'MinValue'];
	this.dictWithConstants['string'] = ['Empty'];
	this.dictWithConstants['double'] = ['MinValue', 'MaxValue', 'Epsilon', 'NegativeInfinity', 'PositiveInfinity', 'NaN'];
	this.dictWithConstants['Math'] = ['PI', 'E'];
	this.dictWithConstants['long'] = ['MaxValue', 'MinValue'];
	this.dictWithConstants['Boolean'] = ['TrueString', 'FalseString'];
	this.dictWithConstants['double'] = ['MinValue', 'Epsilon', 'MaxValue', 'PositiveInfinity', 'NegativeInfinity', 'NaN'];


	this.returnTypeDict = [];
	this.returnTypeDict['int'] = ['Parse', 'Compare', 'CompareOrdinal', 'Read', 'Abs', 'Max', 'Min', 'Sign', 'DivRem', 'Count', 'Sum', 'BinarySearch', 'FindIndex', 'FindLastIndex', 'IndexOf', 'LastIndexOf', 'CompareTo', 'GetHashCode', 'IndexOfAny', 'LastIndexOfAny', 'GetLength', 'GetUpperBound', 'GetLowerBound', 'RemoveAll', 'Length', 'BufferHeight', 'BufferWidth', 'WindowHeight', 'WindowWidth', 'LargestWindowWidth', 'LargestWindowHeight', 'WindowLeft', 'WindowTop', 'CursorLeft', 'CursorTop', 'CursorSize', 'Rank', 'Capacity', 'MaxValue', 'MinValue'];
	this.returnTypeDict['Boolean'] = ['TryParse', 'Equals', 'IsNullOrEmpty', 'IsNullOrWhiteSpace', 'IsInfinity', 'IsPositiveInfinity', 'IsNegativeInfinity', 'IsNaN', 'Parse', 'SequenceEqual', 'Any', 'All', 'Contains', 'Exists', 'TrueForAll', 'IsNormalized', 'EndsWith', 'StartsWith', 'Remove', 'IsInputRedirected', 'IsOutputRedirected', 'IsErrorRedirected', 'CursorVisible', 'KeyAvailable', 'NumberLock', 'CapsLock', 'TreatControlCAsInput', 'IsReadOnly', 'IsFixedSize', 'IsSynchronized'];
	this.returnTypeDict['string'] = ['Join', 'Format', 'Copy', 'Concat', 'Intern', 'IsInterned', 'ReadLine', 'ToString', 'Split', 'Substring', 'Trim', 'TrimStart', 'TrimEnd', 'Normalize', 'PadLeft', 'PadRight', 'ToLower', 'ToLowerInvariant', 'ToUpper', 'ToUpperInvariant', 'Insert', 'Replace', 'Remove', 'Title', 'Empty', 'TrueString', 'FalseString'];
	this.returnTypeDict['double'] = ['Parse', 'Acos', 'Asin', 'Atan', 'Atan2', 'Ceiling', 'Cos', 'Cosh', 'Floor', 'Sin', 'Tan', 'Sinh', 'Tanh', 'Round', 'Truncate', 'Sqrt', 'Log', 'Log10', 'Exp', 'Pow', 'IEEERemainder', 'Abs', 'Max', 'Min', 'Sum', 'Average', 'MinValue', 'MaxValue', 'Epsilon', 'NegativeInfinity', 'PositiveInfinity', 'NaN', 'PI', 'E'];
	this.returnTypeDict['Void'] = ['Beep', 'Clear', 'ResetColor', 'MoveBufferArea', 'SetBufferSize', 'SetWindowSize', 'SetWindowPosition', 'SetCursorPosition', 'SetIn', 'SetOut', 'SetError', 'WriteLine', 'Write', 'Resize', 'Copy', 'ConstrainedCopy', 'ForEach', 'Reverse', 'Sort', 'CopyTo', 'SetValue', 'Initialize', 'Add', 'AddRange', 'Insert', 'InsertRange', 'RemoveAt', 'RemoveRange', 'TrimExcess'];
	this.returnTypeDict['ConsoleKeyInfo'] = ['ReadKey'];
	this.returnTypeDict['IO.Stream'] = ['OpenStandardError', 'OpenStandardInput', 'OpenStandardOutput'];
	this.returnTypeDict['Decimal'] = ['Ceiling', 'Floor', 'Round', 'Truncate', 'Abs', 'Max', 'Min', 'Sum', 'Average'];
	this.returnTypeDict['SByte'] = ['Abs', 'Max', 'Min'];
	this.returnTypeDict['long'] = ['Abs', 'Max', 'Min', 'BigMul', 'DivRem', 'Parse', 'LongCount', 'Sum', 'GetLongLength', 'LongLength', 'MaxValue', 'MinValue'];
	this.returnTypeDict['Byte'] = ['Max', 'Min'];
	this.returnTypeDict['Enumerable'] = ['Sum', 'Min', 'Max', 'Average', 'Except', 'Reverse', 'AsEnumerable', 'ToArray', 'ToDictionary', 'ToLookup', 'DefaultIfEmpty', 'OfType', 'Cast', 'Range', 'Repeat', 'Empty', 'Where', 'Select', 'SelectMany', 'Take', 'TakeWhile', 'Skip', 'SkipWhile', 'Join', 'GroupJoin', 'OrderBy', 'OrderByDescending', 'ThenBy', 'ThenByDescending', 'GroupBy', 'Concat', 'Zip', 'Distinct', 'Union', 'Intersect', 'AsReadOnly', 'ConvertAll', 'FindAll', 'ToCharArray'];
	this.returnTypeDict['TSource'] = ['Min', 'Max', 'First', 'FirstOrDefault', 'Last', 'LastOrDefault', 'Single', 'SingleOrDefault', 'ElementAt', 'ElementAtOrDefault', 'Aggregate'];
	this.returnTypeDict['TResult'] = ['Min', 'Max', 'Aggregate'];
	this.returnTypeDict['List'] = ['ToList', 'ConvertAll', 'FindAll', 'GetEnumerator', 'GetRange'];
	this.returnTypeDict['TAccumulate'] = ['Aggregate'];
	this.returnTypeDict['Array'] = ['CreateInstance'];
	this.returnTypeDict['T'] = ['Find', 'FindLast', 'Item'];
	this.returnTypeDict['TypeCode'] = ['GetTypeCode'];
	this.returnTypeDict['Type'] = ['GetType'];
	this.returnTypeDict['Object'] = ['Clone', 'GetValue', 'SyncRoot'];
	this.returnTypeDict['CharEnumerator'] = ['GetEnumerator'];
	this.returnTypeDict['Collections.IEnumerator'] = ['GetEnumerator'];
	this.returnTypeDict['Char'] = ['Chars'];
	this.returnTypeDict['IO.TextReader'] = ['In'];
	this.returnTypeDict['IO.TextWriter'] = ['Out', 'Error'];
	this.returnTypeDict['Text.Encoding'] = ['InputEncoding', 'OutputEncoding'];
	this.returnTypeDict['ConsoleColor'] = ['BackgroundColor', 'ForegroundColor'];

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
		var isWasFound = false;
		if (arrayContains(this.types, beforeDot)) {
			if (this.dictWithStaticMethods[beforeDot] != undefined) {
				console.log(beforeDot);
				found.AddAll(this.dictWithStaticMethods[beforeDot]);
				isWasFound = true;
			}
			if (this.dictWithProperties[beforeDot] != undefined) {
				found.AddAll(this.dictWithProperties[beforeDot]);
				isWasFound = true;
			}
			if (this.dictWithConstants[beforeDot] != undefined) {
				found.AddAll(this.dictWithConstants[beforeDot]);
				isWasFound = true;
			}
			if (!isWasFound) {
				found.AddAll(this.methods);
			}
		} else {
			for (var type in this.returnTypeDict) {
				var isNeedBreak = false;
				for (var element in this.returnTypeDict[type]) {
					if (beforeDot == this.returnTypeDict[type][element]) {
						if (type == "Void") {
							isWasFound = true;
							isNeedBreak = true;
							break;
						}
						if (this.dictWithNonStaticMethods[type] != undefined) {
							found.AddAll(this.dictWithNonStaticMethods[type]);
							isWasFound = true;
						}
						if (this.dictWithStaticMethods[type] != undefined) {
							found.AddAll(this.dictWithStaticMethods[type]);
							isWasFound = true;
						}
						if (this.dictWithProperties[type] != undefined) {
							found.AddAll(this.dictWithProperties[type]);
							isWasFound = true;
						}
						if (this.dictWithConstants[type] != undefined) {
							found.AddAll(this.dictWithConstants[type]);
							isWasFound = true;
						}
						isNeedBreak = true;
						break;
					}
				}
				if (isNeedBreak)
					break;
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
