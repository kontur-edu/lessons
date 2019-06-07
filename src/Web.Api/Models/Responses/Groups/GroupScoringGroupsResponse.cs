using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace Ulearn.Web.Api.Models.Responses.Groups
{
	[DataContract]
	public class GroupScoringGroupsResponse : SuccessResponse
	{
		[DataMember]
		public List<GroupScoringGroupInfo> Scores { get; set; }
	}

	[DataContract]
	public abstract class AbstractScoringGroupInfo
	{
		[DataMember]
		public string Id { get; set; }
		
		[DataMember]
		public string Name { get; set; }
		
		[DataMember]
		public string Abbreviation { get; set; }
		
		[DataMember]
		public string Description { get; set; }
	}
	
	[DataContract]
	public class GroupScoringGroupInfo : AbstractScoringGroupInfo
	{
		[DataMember]
		public bool AreAdditionalScoresEnabledForAllGroups { get; set; }
		
		[DataMember]
		public bool CanInstructorSetAdditionalScoreInSomeUnit { get; set; }
		
		[DataMember]
		public bool? AreAdditionalScoresEnabledInThisGroup { get; set; }
	}

	[DataContract]
	public class UnitScoringGroupInfo : AbstractScoringGroupInfo
	{
		[DataMember]
		public bool CanInstructorSetAdditionalScore { get; set; }
		
		[DataMember]
		public int MaxAdditionalScore { get; set; }
	}
}