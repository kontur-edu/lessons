using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using RunCsJob.Api;
using Ulearn.Core;

namespace RunCsJob
{
	[TestFixture]
	public class SubmitionSerialization_Test
	{
		[Test]
		public void Test_Serialization_and_Deserialization()
		{
			var inputList = new List<RunnerSubmission>
			{
				new ProjRunnerSubmission
				{
					Id = "E26C2109-F074-4117-B53F-0799E4140DEF",
					Input = "",
					NeedRun = true,
					ProjectFileName = "proj",
					ZipFileData = new byte[] { 1, 2, 3, 4, 5, 61, 23, 4, 3 }
				},
				new FileRunnerSubmission
				{
					Code = "code",
					Id = "E9D8C168-A3D9-48CC-AF60-CE3B8A1D8314",
					Input = "",
					NeedRun = true
				}
			};
			var json = JsonConvert.SerializeObject(inputList, JsonConfig.GetSettings());
			var deserializedList = JsonConvert.DeserializeObject<List<RunnerSubmission>>(json, JsonConfig.GetSettings());
			deserializedList.ShouldAllBeEquivalentTo(inputList);
		}

		[Test]
		public void Test_Serialization()
		{
			var list = new List<RunnerSubmission>
			{
				new ProjRunnerSubmission
				{
					Id = "E26C2109-F074-4117-B53F-0799E4140DEF",
					Input = "",
					NeedRun = true,
					ProjectFileName = "proj",
					ZipFileData = new byte[] { 1, 2, 3, 4, 5, 61, 23, 4, 3 }
				}
			};

			var json = JsonConvert.SerializeObject(list, JsonConfig.GetSettings());
			Assert.That(json, Is.EqualTo("[{\"$type\":\"proj\",\"ZipFileData\":\"AQIDBAU9FwQD\",\"ProjectFileName\":\"proj\",\"Id\":\"E26C2109-F074-4117-B53F-0799E4140DEF\",\"Input\":\"\",\"NeedRun\":true}]"));
		}

		[Test]
		public
			void Test
			()
		{
			const string json = @"[{""$type"":""file"",""Code"":""code"",""Id"":""1029"",""Input"":"""",""NeedRun"":true}]";
			var list = JsonConvert.DeserializeObject<List<RunnerSubmission>>(json, JsonConfig.GetSettings());
			Assert.That(list.Count, Is.EqualTo(1));
			Assert.That(list[0], Is.InstanceOf<FileRunnerSubmission>());
		}
	}
}