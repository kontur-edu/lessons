using System;

namespace uLearn
{
	public class SlideAttribute : Attribute
	{
		public SlideAttribute(string title, string guid)
		{
			Title = title;
			Guid = guid;
		}

		public string Title { get; set; }
		public string Guid { get; set; }
	}
}