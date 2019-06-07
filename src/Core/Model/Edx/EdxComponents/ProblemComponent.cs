using System;
using System.Xml;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	public class ProblemComponent : Component
	{
		[XmlIgnore]
		public override string SubfolderName
		{
			get { return "problem"; }
		}

		public override EdxReference GetReference()
		{
			return new ProblemComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			var xmlString = this.XmlSerialize();
			return xmlString.Substring("<problem>".Length, xmlString.Length - "<problem></problem>".Length);
		}
	}

	[XmlRoot("problem")]
	public class SlideProblemComponent : Component
	{
		[XmlIgnore]
		public Component[] Subcomponents;

//		[XmlElement("p")]
//		public XmlElement[] XmlSubcomponents;

		[XmlIgnore]
		public override string SubfolderName
		{
			get { return "problem"; }
		}

		public override EdxReference GetReference()
		{
			return new ProblemComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			throw new NotSupportedException();
		}

		public override void SaveAdditional(string folderName)
		{
			if (Subcomponents != null)
				foreach (var subcomponent in Subcomponents)
					subcomponent.SaveAdditional(folderName);
		}

		public static SlideProblemComponent Load(string folderName, string urlName, EdxLoadOptions options)
			=> Load<SlideProblemComponent>(folderName, "problem", urlName, options);
	}

	[XmlRoot("problem")]
	public class TextInputComponent : ProblemComponent
	{
		[XmlElement("p")]
		public string Title;

		[XmlElement("stringresponse")]
		public StringResponse StringResponse;

		[XmlElement("solution")]
		public Solution Solution;
	}

	public class StringResponse
	{
		[XmlAttribute("answer")]
		public string Answer;

		[XmlAttribute("type")]
		public string Type;

		[XmlElement("additional_answer")]
		public Answer[] AdditionalAnswers;

		[XmlElement("textline")]
		public Textline Textline;
	}

	public class Answer
	{
		[XmlText]
		public string Text;
	}

	public class Textline
	{
		[XmlAttribute("label")]
		public string Label;

		[XmlAttribute("size")]
		public int Size;
	}

	[XmlRoot("problem")]
	public class MultipleChoiceComponent : ProblemComponent
	{
		[XmlElement("p")]
		public string Title;

		[XmlElement("multiplechoiceresponse", Type = typeof(MultipleChoiceResponse))]
		[XmlElement("choiceresponse", Type = typeof(ChoiceResponse))]
		public ChoiceResponse ChoiceResponse;

		[XmlElement("solution")]
		public Solution Solution;
	}

	public class Solution
	{
		//		[XmlText]
		public XmlElement Text;

		public Solution()
		{
		}

		public Solution(string text)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				var doc = new XmlDocument();
				doc.LoadXml(string.Format("<div class=\"detailed-solution\"><p>Explanation</p><p>{0}</p></div>", text));
				Text = doc.DocumentElement;
			}
		}
	}

	public class ChoiceResponse
	{
		[XmlElement("choicegroup", Type = typeof(MultipleChoiceGroup))]
		[XmlElement("checkboxgroup", Type = typeof(CheckboxGroup))]
		public ChoiceGroup ChoiceGroup;
	}

	public class MultipleChoiceResponse : ChoiceResponse
	{
	}

	public class ChoiceGroup
	{
		[XmlAttribute("label")]
		public string Label;

		[XmlElement("choice")]
		public Choice[] Choices;
	}

	public class MultipleChoiceGroup : ChoiceGroup
	{
		[XmlAttribute("type")]
		public string Type;
	}

	public class CheckboxGroup : ChoiceGroup
	{
		[XmlAttribute("direction")]
		public string Direction;
	}

	public class Choice
	{
		[XmlAttribute("correct")]
		public bool Correct;

		[XmlText]
		public string Text;
	}
}