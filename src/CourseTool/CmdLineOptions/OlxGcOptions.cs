using System;
using System.IO;
using CommandLine;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-gc", HelpText = "Remove not existent items from course")]
	public class OlxGcOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			var loadOptions = new EdxLoadOptions();
			loadOptions.FailOnNonExistingItem = false;
			loadOptions.HandleNonExistentItemTypeName = (type, url) => Console.WriteLine($"Skipped non existent item type:{type} urlName:{url}");
			var folderName = Path.Combine(Dir, "olx");
			Console.WriteLine("Loading");
			var course = EdxCourse.Load(folderName, loadOptions);
			Console.WriteLine("Saving");
			course.Save(folderName);
		}
	}
}