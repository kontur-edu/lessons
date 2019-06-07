using System.Xml.Serialization;

namespace Ulearn.Core.Courses
{
	public class ScoringGroup
	{
		private const int defaultMaxAdditionalScore = 0;
		private const bool defaultEnabledForEveryone = false;

		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("abbr")]
		public string Abbreviation { get; set; }

		[XmlAttribute("description")]
		public string Description { get; set; }

		[XmlIgnore]
		public bool CanBeSetByInstructor => MaxAdditionalScore > 0;

		[XmlAttribute("maxAdditionalScore")]
		public string _maxAdditionalScore { get; set; }
		
		[XmlIgnore]
		public int MaxAdditionalScore
		{
			get
			{
				if (string.IsNullOrEmpty(_maxAdditionalScore) || _maxAdditionalScore.Trim().Length == 0)
					return defaultMaxAdditionalScore;

				int result;
				return int.TryParse(_maxAdditionalScore, out result) ? result : defaultMaxAdditionalScore;
			}
			set => _maxAdditionalScore = value.ToString();
		}
		
		[XmlIgnore]
		public bool IsMaxAdditionalScoreSpecified => !string.IsNullOrEmpty(_maxAdditionalScore);

		[XmlIgnore]
		/* Calculates automatically by slides's scores */
		public int MaxNotAdditionalScore { get; set; }

		[XmlAttribute("enableForEveryone")]
		public string _enabledForEveryone;

		[XmlIgnore]
		public bool EnabledForEveryone
		{
			get
			{
				if (string.IsNullOrEmpty(_enabledForEveryone) || _enabledForEveryone.Trim().Length == 0)
					return defaultEnabledForEveryone;

				return bool.TryParse(_enabledForEveryone, out bool value) ? value : defaultEnabledForEveryone;
			}
			set => _enabledForEveryone = value.ToString();
		}

		[XmlIgnore]
		public bool IsEnabledForEveryoneSpecified => !string.IsNullOrEmpty(_enabledForEveryone);

		[XmlText]
		public string Name { get; set; }

		public void CopySettingsFrom(ScoringGroup otherScoringGroup)
		{
			_maxAdditionalScore = string.IsNullOrEmpty(_maxAdditionalScore) ? otherScoringGroup._maxAdditionalScore : _maxAdditionalScore;
			_enabledForEveryone = string.IsNullOrEmpty(_enabledForEveryone) ? otherScoringGroup._enabledForEveryone : _enabledForEveryone;
			Abbreviation = Abbreviation ?? otherScoringGroup.Abbreviation;
			Name = string.IsNullOrEmpty(Name) ? otherScoringGroup.Name : Name;
			Description = string.IsNullOrEmpty(Description) ? otherScoringGroup.Description : Description;
		}
	}
}