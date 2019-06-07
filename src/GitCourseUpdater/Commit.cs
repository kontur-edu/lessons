using System;
using System.Runtime.Serialization;

namespace GitCourseUpdater
{
	[DataContract]
	public class CommitInfo
	{
		[DataMember(Name = "hash")]public string Hash;
		[DataMember(Name = "message")]public string Message;
		[DataMember(Name = "authorName")]public string AuthorName;
		[DataMember(Name = "authorEmail")]public string AuthorEmail;
		[DataMember(Name = "time")]public DateTimeOffset Time;
	}
}