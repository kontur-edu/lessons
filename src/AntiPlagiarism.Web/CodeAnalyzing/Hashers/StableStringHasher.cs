﻿using System;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.Web.CodeAnalyzing.Hashers
{
	public class StableStringHasher : IObjectHasher
	{
		public int GetHashCode(object o)
		{
			if (! (o is string))
				throw new ArgumentException("Object should be string for stable hashing", nameof(o));

			return ((string)o).GetStableHashCode();
		}
	}
}