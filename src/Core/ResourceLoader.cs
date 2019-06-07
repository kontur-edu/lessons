using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Common.Extensions;

namespace Ulearn.Core
{
	public class ResourceFile
	{
		public ResourceFile(string filename, string fullName, Func<byte[]> getContent)
		{
			Filename = filename;
			FullName = fullName;
			this.getContent = getContent;
		}

		public string Filename;
		public string FullName;
		private readonly Func<byte[]> getContent;

		public byte[] GetContent()
		{
			return getContent();
		}

		public string GetUtf8Content()
		{
			return getContent().AsUtf8();
		}
	}

	public class ResourceLoader
	{
		private readonly Type type;

		public ResourceLoader(Type type)
		{
			this.type = type;
		}

		public IEnumerable<ResourceFile> EnumerateResourcesFrom(string ns)
		{
			if (!ns.EndsWith("."))
				ns = ns + ".";
			return type.Assembly.GetManifestResourceNames()
				.Where(name => name.StartsWith(ns, StringComparison.InvariantCultureIgnoreCase))
				.Select(name => new ResourceFile(name.Substring(ns.Length), name, () => LoadResource(name)));
		}

		public byte[] LoadResource(string name)
		{
			var buffer = new MemoryStream();
			Stream stream = type.Assembly
				.GetManifestResourceStream(name);
			if (stream == null)
				throw new Exception("No resource " + name);
			stream.CopyTo(buffer);
			return buffer.ToArray();
		}
	}
}