﻿using System.Xml.Serialization;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	[XmlRoot("video")]
	public class VideoComponent : Component
	{
		public override bool ShouldSerializeUrlName()
		{
			return true;
		}

		[XmlAttribute("youtube")]
		public string VideoId;

		[XmlAttribute("youtube_id_1_0")]
		public string NormalSpeedVideoId;
		[XmlAttribute("download_video")]
		public bool DownloadVideo;

		[XmlIgnore]
		public override string SubfolderName => "video";

		public VideoComponent()
		{
		}

		public VideoComponent(string urlName, string displayName, string videoId)
		{
			UrlName = urlName;
			DisplayName = displayName;
			VideoId = "1.00:" + videoId;
			NormalSpeedVideoId = videoId;
		}

		public override EdxReference GetReference()
		{
			return new VideoComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			return string.Format(@"<iframe class=""embedded-video"" width=""100%"" height=""530"" src=""//www.youtube.com/embed/{0}"" frameborder=""0"" allowfullscreen=""\""></iframe>", NormalSpeedVideoId);
		}

		public static VideoComponent Load(string folderName, string urlName, EdxLoadOptions options)
		{
			return Load<VideoComponent>(folderName, "video", urlName, options);
		}
	}
}