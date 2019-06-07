using System;
using System.Collections.Generic;

namespace GiftsGranter
{
	public class CourseSettings
	{
		public int maxScore;
		public int masterScore;
		public int passScore;
		public string masterTitle;
		public string passTitle;
		public string message;
		public string giftImagePath;
		public string subtitle;
		public List<Guid> requiredSlides;
	}
}