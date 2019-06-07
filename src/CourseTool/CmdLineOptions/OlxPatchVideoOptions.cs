using System.IO;
using System.Linq;
using CommandLine;
using Newtonsoft.Json;
using uLearn.CourseTool.Json;
using Ulearn.Core;
using Ulearn.Core.Model.Edx;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-patch-video", HelpText = "Patch Edx course with videos from json file")]
	class OlxPatchVideoOptions : OlxAbstractPatchOptions
	{
		public override void Patch(OlxPatcher patcher, Config config, Profile profile, EdxCourse edxCourse)
		{
			var videoJson = Path.Combine(Dir, config.Video);
			var video = File.Exists(videoJson)
				? JsonConvert.DeserializeObject<Video>(File.ReadAllText(videoJson))
				: new Video { Records = new Record[0] };
			VideoHistory.UpdateHistory(Dir, video);
			var guids = Guids == null ? null : Guids.Split(',').Select(Utils.GetNormalizedGuid).ToList();
			if (config.Video != null && File.Exists(Path.Combine(Dir, config.Video)))
			{
				var videoComponents = video
					.Records
					.Where(x => guids == null || guids.Contains(x.Guid.GetNormalizedGuid()))
					.Select(x => new VideoComponent(x.Guid.GetNormalizedGuid(), x.Data.Name, x.Data.Id));

				patcher.PatchComponents(
					edxCourse,
					videoComponents,
					guids != null || !SkipExistingGuids
				);
			}
		}
	}
}