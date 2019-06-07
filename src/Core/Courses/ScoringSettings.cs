using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;

namespace Ulearn.Core.Courses
{
	public class ScoringSettings
	{
		public ScoringSettings()
		{
			_groups = new ScoringGroup[0];
		}

		[XmlAttribute("defaultQuiz")]
		public string _defaultScoringGroupForQuiz { get; set; }

		[XmlIgnore]
		public string DefaultScoringGroupForQuiz =>
			string.IsNullOrEmpty(_defaultScoringGroupForQuiz) ? DefaultScoringGroup : _defaultScoringGroupForQuiz;

		[XmlAttribute("defaultExercise")]
		public string _defaultScoringGroupForExercise { get; set; }

		[XmlIgnore]
		public string DefaultScoringGroupForExercise =>
			string.IsNullOrEmpty(_defaultScoringGroupForExercise) ? DefaultScoringGroup : _defaultScoringGroupForExercise;

		[XmlAttribute("default")]
		public string DefaultScoringGroup { get; set; } = "";

		[XmlElement("group")]
		public ScoringGroup[] _groups { get; set; }

		private SortedDictionary<string, ScoringGroup> groupsCache;

		[XmlIgnore]
		public SortedDictionary<string, ScoringGroup> Groups
		{
			get { return groupsCache ?? (groupsCache = _groups.ToDictionary(g => g.Id, g => g).ToSortedDictionary()); }
		}

		public void CopySettingsFrom(ScoringSettings otherScoringSettings)
		{
			_defaultScoringGroupForQuiz = string.IsNullOrEmpty(_defaultScoringGroupForQuiz) && string.IsNullOrEmpty(DefaultScoringGroup)
				? otherScoringSettings.DefaultScoringGroupForQuiz
				: _defaultScoringGroupForQuiz;
			_defaultScoringGroupForExercise = string.IsNullOrEmpty(_defaultScoringGroupForExercise) && string.IsNullOrEmpty(DefaultScoringGroup)
				? otherScoringSettings.DefaultScoringGroupForExercise
				: _defaultScoringGroupForExercise;
			DefaultScoringGroup = string.IsNullOrEmpty(DefaultScoringGroup) ? otherScoringSettings.DefaultScoringGroup : DefaultScoringGroup;

			/* Copy missing scoring groups */
			foreach (var scoringGroupId in otherScoringSettings.Groups.Keys)
				if (!Groups.ContainsKey(scoringGroupId))
					Groups[scoringGroupId] = otherScoringSettings.Groups[scoringGroupId];
		}

		public int GetMaxAdditionalScore()
		{
			return Groups.Values.Where(g => g.CanBeSetByInstructor).Sum(g => g.MaxAdditionalScore);
		}
	}
}