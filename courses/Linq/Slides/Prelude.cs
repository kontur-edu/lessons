// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

public class SlideTestBase { }
public class Assert
{
	public static void AreEqual(object expected, object actual, string message = null)
	{
		if (ReferenceEquals(expected, actual))
			return;
		if (!Equals(expected, actual))
			throw new Exception(string.Format("Expected '{0}' but was '{1}'. {2}", expected, actual, message));
	}


	public static void IsTrue(bool actual)
	{
		if (!actual)
			throw new Exception("Expected true, but was false");
	}
	
	public static void NotNull(object actual)
	{
		if (actual == null)
			throw new Exception("Expected not null");
	}


	public static void IsFalse(bool actual)
	{
		if (actual)
			throw new Exception("Expected false, but was true");
	}

	public static void AreEqual(int expected, int actual)
	{
		if (expected != actual)
			throw new Exception(string.Format("Expected '{0}' but was '{1}'", expected, actual));
	}

	public static void AreEqual(double expected, double actual)
	{
		if (expected != actual)
			throw new Exception(string.Format("Expected '{0}' but was '{1}'", expected, actual));
	}
}