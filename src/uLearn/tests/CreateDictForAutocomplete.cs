using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace uLearn.tests
{
	class CreateDictForAutocomplete
	{
		public static Dictionary<string, List<string>> ReturnTypeDictionary = new Dictionary<string, List<string>>();

		public static int TotalWordCount = 0;

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
				typeof (List<>)
			};
			Console.WriteLine("this.types = [{0}];\n", ToArrayString(myTypes.Select(ToPrettyString)));
			WalkThroughTypes(myTypes);
		}

		private static void WalkThroughTypes(Type[] myTypes)
		{
			const string staticMethodDict = "dictWithStaticMethods";
			VisitTypesElements(staticMethodDict, GetStaticMethods, myTypes);

			const string nonStaticMethodDict = "dictWithNonStaticMethods";
			VisitTypesElements(nonStaticMethodDict, GetNonStaticMethods, myTypes);

			const string propertiesDict = "dictWithProperties";
			VisitTypesElements(propertiesDict, GetProperties, myTypes);

			const string constantDict = "dictWithConstants";
			VisitTypesElements(constantDict, GetConstants, myTypes);
			
			PrintReturnTypeDictionary();
			Console.WriteLine("Total word count in all dictionary: {0}", TotalWordCount);
		}

		private static void PrintReturnTypeDictionary()
		{
			Console.WriteLine("this.returnTypeDict = [];");
			foreach (
				var type in
					ReturnTypeDictionary.Keys/*.Where(
						type => !type.ToString().Contains("TSource") && !type.ToString().Contains("TResult") && myTypes.Contains(type))*/)
			{
				Console.WriteLine("this.returnTypeDict['{0}'] = [{1}];", type,
					ToArrayString(ReturnTypeDictionary[type].Distinct()));
				TotalWordCount += ReturnTypeDictionary[type].Distinct().Count();
			}
			Console.WriteLine();
		}

		private static void VisitTypesElements(string dictName, Func<Type, IEnumerable<string>> func, IEnumerable<Type> myTypes)
		{
			Console.WriteLine("this.{0} = [];", dictName);
			foreach (var myType in myTypes)
			{
				var collection = func(myType).ToList();
				TotalWordCount += collection.Count();
				if (!collection.Any())
					continue;
				Console.WriteLine("this.{0}['{1}'] = [{2}];", dictName, ToPrettyString(myType), ToArrayString(collection));
			}
			Console.WriteLine();
		}

		private static string ToPrettyString(Type myType)
		{
			var type = myType.ToString().Replace("System.", "").Replace("Linq.", "");
			if (type.Contains("List"))
				return "List";
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
			if (type.Contains("["))
				return "Enumerable";
			return type;
		}

		private static string ToArrayString(IEnumerable<string> collection)
		{
			return "'" + string.Join("', '", collection) + "'";
		}

		private static IEnumerable<string> GetStaticMethods(Type myType)
		{
			return GetMethods(myType, true);
		}

		private static IEnumerable<string> GetNonStaticMethods(Type myType)
		{
			return GetMethods(myType, false);
		}

		private static IEnumerable<string> GetMethods(Type myType, bool isNeedStatic)
		{
			var methods = myType.GetMethods().Where(y => y.Name[0] != y.Name.ToLower()[0]).Where(x => isNeedStatic ? x.IsStatic : !x.IsStatic).ToArray();
			foreach (var methodInfo in methods)
			{
				var type = ToPrettyString(methodInfo.ReturnType);
				if (!ReturnTypeDictionary.ContainsKey(type))
					ReturnTypeDictionary[type] = new List<string>();
				ReturnTypeDictionary[type].Add(methodInfo.Name);
			}
			return methods.Select(x => x.Name).Distinct();
		}

		private static IEnumerable<string> GetProperties(Type myType)
		{
			var properties = myType.GetProperties().Where(y => y.Name[0] != y.Name.ToLower()[0]).ToArray();
			foreach (var propertyInfo in properties)
			{
				var type = ToPrettyString(propertyInfo.PropertyType);
				if (!ReturnTypeDictionary.ContainsKey(type))
					ReturnTypeDictionary[type] = new List<string>();
				ReturnTypeDictionary[type].Add(propertyInfo.Name);
			}
			return properties.Select(x => x.Name).Distinct();
		}

		private static IEnumerable<string> GetConstants(Type myType)
		{
			var fields = myType.GetFields().Where(y => y.Name[0] != y.Name.ToLower()[0]).ToArray();
			foreach (var fieldInfo in fields)
			{
				var type = ToPrettyString(fieldInfo.FieldType);
				if (!ReturnTypeDictionary.ContainsKey(type))
					ReturnTypeDictionary[type] = new List<string>();
				ReturnTypeDictionary[type].Add(fieldInfo.Name);
			}
			return fields.Select(x => x.Name).Distinct();
		}
	}
}
