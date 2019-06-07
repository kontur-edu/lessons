using System;
using System.Linq;
using System.Reflection;

namespace Database.Extensions
{
	public static class IQueryableExtensions
	{
		public static IQueryable<T> DynamicOfType<T>(this IQueryable<T> input, Type type)
		{
			var ofType = typeof(Queryable).GetMethod("OfType", BindingFlags.Static | BindingFlags.Public);
			var ofTypeGenericMethod = ofType.MakeGenericMethod(type);
			return (IQueryable<T>)ofTypeGenericMethod.Invoke(null, new object[] { input });
		}
	}
}