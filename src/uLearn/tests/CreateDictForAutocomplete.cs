using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace uLearn.tests
{
	static class CreateDictForAutocomplete
	{
		private static readonly Dictionary<string, List<string>> returnTypeDictionary = new Dictionary<string, List<string>>();

		private static int totalWordCount;

		[Explicit]
		[Test]
		public static void CreateDict()
		{
			var myTypes = new[]
			{
				typeof (int),
				typeof (string),
				typeof (double),
				typeof (Console),
				typeof (Math),
				typeof (long),
				typeof (Boolean),
				typeof (float),
				typeof (Enumerable),
				typeof (Array),
				typeof (List<>),
				typeof (Dictionary<,>),
				typeof (char),
			};
			Console.WriteLine("this.types = [{0}];\n", ToArrayString(myTypes.Select(ToPrettyString)));
			WalkThroughTypes(myTypes);
		}

		private static void WalkThroughTypes(Type[] myTypes)
		{
			const string staticDict = "dictWithStatic";
			VisitTypesElements(staticDict, GetStatic, myTypes);

			const string nonStaticDict = "dictWithNonStatic";
			VisitTypesElements(nonStaticDict, GetNonStatic, myTypes);

			PrintReturnTypeDictionary();
			Console.WriteLine("Total word count in all dictionary: {0}", totalWordCount);
		}

		private static void PrintReturnTypeDictionary()
		{
			Console.WriteLine("this.returnTypeDict = [];");
			foreach (
				var type in
					returnTypeDictionary.Keys/*.Where(
						type => !type.ToString().Contains("TSource") && !type.ToString().Contains("TResult") && myTypes.Contains(type))*/)
			{
				Console.WriteLine("this.returnTypeDict['{0}'] = [{1}];", type,
					ToArrayString(returnTypeDictionary[type].Distinct()));
				totalWordCount += returnTypeDictionary[type].Distinct().Count();
			}
			Console.WriteLine();
		}

		private static void VisitTypesElements(string dictName, Func<Type, IEnumerable<string>> func, IEnumerable<Type> myTypes)
		{
			Console.WriteLine("this.{0} = [];", dictName);
			foreach (var myType in myTypes)
			{
				var collection = func(myType).ToList();
				totalWordCount += collection.Count();
				Console.WriteLine("this.{0}['{1}'] = [{2}];", dictName, ToPrettyString(myType), collection.Any() ? ToArrayString(collection) : "");
			}
			Console.WriteLine();
		}

		private static string ToPrettyString(Type myType)
		{
			var type = myType.ToString().Replace("System.", "").Replace("Linq.", "");
			if (type.Contains("List"))
				return "List";
			if (type.Contains("Dictionary"))
				return "Dictionary";
			if (type.Contains("Enumerable"))
				return "Enumerable";
			if (type.Contains("List"))
				return "List";
			if (type.Contains("Int32") || type.Contains("Int16"))
				return "int";
			if (type.Contains("Int64"))
				return "long";
			if (type.Contains("String"))
				return "string";
			if (type.Contains("Double"))
				return "double";
			if (type.Contains("Single"))
				return "double";
			if (type.Contains("Char"))
				return "char";
			if (type.Contains("["))
				return "Enumerable";
			return type;
		}

		private static string ToArrayString(IEnumerable<string> collection)
		{
			return "'" + string.Join("', '", collection) + "'";
		}

		private static IEnumerable<string> GetStatic(Type myType)
		{
			return GetMembers(myType, true);
		}

		private static IEnumerable<string> GetNonStatic(Type myType)
		{
			return GetMembers(myType, false);
		}

		private static IEnumerable<string> GetMembers(IReflect myType, bool isNeedStatic)
		{
			var flags = BindingFlags.Public;
			flags |= isNeedStatic ? BindingFlags.Static : BindingFlags.Instance;
			return GetMethods(myType, flags)
				.Concat(GetProperties(myType, flags))
				.Concat(GetConstants(myType, flags))
				.Distinct()
				.OrderBy(s => s);
		}

		private static IEnumerable<string> GetMethods(IReflect myType, BindingFlags flags)
		{
			var methods = myType.GetMethods(flags).Where(info => !info.IsSpecialName).ToList();
			foreach (var methodInfo in methods)
			{
				var type = ToPrettyString(methodInfo.ReturnType);
				if (!returnTypeDictionary.ContainsKey(type))
					returnTypeDictionary[type] = new List<string>();
				returnTypeDictionary[type].Add(methodInfo.Name);
			}
			return methods.Select(x => x.Name).Distinct();
		}

		private static IEnumerable<string> GetProperties(IReflect myType, BindingFlags flags)
		{
			var properties = myType.GetProperties(flags);
			foreach (var propertyInfo in properties)
			{
				var type = ToPrettyString(propertyInfo.PropertyType);
				if (!returnTypeDictionary.ContainsKey(type))
					returnTypeDictionary[type] = new List<string>();
				returnTypeDictionary[type].Add(propertyInfo.Name);
			}
			return properties.Select(x => x.Name).Distinct();
		}

		private static IEnumerable<string> GetConstants(IReflect myType, BindingFlags flags)
		{
			var fields = myType.GetFields(flags);
			foreach (var fieldInfo in fields)
			{
				var type = ToPrettyString(fieldInfo.FieldType);
				if (!returnTypeDictionary.ContainsKey(type))
					returnTypeDictionary[type] = new List<string>();
				returnTypeDictionary[type].Add(fieldInfo.Name);
			}
			return fields.Select(x => x.Name).Distinct();
		}

	}
}
