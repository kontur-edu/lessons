﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace uLearn.Web.Views.Course
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using Database.Models;
    using Ulearn.Core.Courses;
    using Ulearn.Core.Courses.Slides;
    using Ulearn.Core.Courses.Slides.Exercises;
    using Ulearn.Core.Courses.Slides.Exercises.Blocks;
    using Ulearn.Core.Courses.Slides.Quizzes;
    using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
    using uLearn.Web;
    using uLearn.Web.Models;
    using uLearn.Web.Views.Course;
    using uLearn.Web.Views.SlideNavigation;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    public class StandaloneLayout : System.Web.WebPages.HelperPage
    {

#line default
#line hidden
public static System.Web.WebPages.HelperResult Page(Course course, Slide slide, TocModel toc, IEnumerable<string> cssFiles, IEnumerable<string> jsFiles)
{
#line default
#line hidden
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {
 

WriteLiteralTo(__razor_helper_writer, "\t<html>\r\n\t<head>\r\n\t\t<title>Preview: ");

WriteTo(__razor_helper_writer, course.Title);

WriteLiteralTo(__razor_helper_writer, " — ");

          WriteTo(__razor_helper_writer, slide.Title);

WriteLiteralTo(__razor_helper_writer, "</title>\r\n\t\t<link");

WriteLiteralTo(__razor_helper_writer, " rel=\"shortcut icon\"");

WriteLiteralTo(__razor_helper_writer, " href=\"renderer/favicon.ico?v=1\"");

WriteLiteralTo(__razor_helper_writer, "/>\r\n\t\t<meta");

WriteLiteralTo(__razor_helper_writer, " charset=\'UTF-8\'");

WriteLiteralTo(__razor_helper_writer, ">\r\n");

		
         foreach (var cssFile in cssFiles)
		{

WriteLiteralTo(__razor_helper_writer, "\t\t\t<link");

WriteAttributeTo(__razor_helper_writer, "href", Tuple.Create(" href=\'", 763), Tuple.Create("\'", 778)
, Tuple.Create(Tuple.Create("", 770), Tuple.Create<System.Object, System.Int32>(cssFile
, 770), false)
);

WriteLiteralTo(__razor_helper_writer, " rel=\'stylesheet\'");

WriteLiteralTo(__razor_helper_writer, "/>\r\n");

		}

WriteLiteralTo(__razor_helper_writer, "\t</head>\r\n\t<body");

WriteLiteralTo(__razor_helper_writer, " class=\"without-topbar legacy-page\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n\t\t<div");

WriteLiteralTo(__razor_helper_writer, " class=\'side-bar navbar-collapse collapse navbar-nav container\'");

WriteLiteralTo(__razor_helper_writer, ">\r\n");

WriteLiteralTo(__razor_helper_writer, "\t\t\t");

WriteTo(__razor_helper_writer, TableOfContents.Toc(toc));

WriteLiteralTo(__razor_helper_writer, "\r\n\t\t</div>\r\n\r\n\t\t<div");

WriteLiteralTo(__razor_helper_writer, " class=\"slide-container\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n\t\t\t<div");

WriteLiteralTo(__razor_helper_writer, " class=\"container body-content\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n\t\t\t\t<div");

WriteLiteralTo(__razor_helper_writer, " class=\"row\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n");

					
                      
						var blockData = slide.Blocks.Select(
							(b, i) => b is AbstractExerciseBlock
								? new ExerciseBlockData(course.Id, (ExerciseSlide)slide) { RunSolutionUrl = "/" + slide.Index.ToString("000") + ".html?query=submit", DebugView = true, IsGuest = false }
								: b is AbstractQuestionBlock
									? new QuizBlockData(new QuizModel
									{
										AnswersToQuizzes = slide.Blocks.OfType<AbstractQuestionBlock>().ToDictionary(x => x.Id, x => new List<UserQuizAnswer>()),
										Slide = (QuizSlide)slide
									}, i, new QuizState(QuizStatus.ReadyToSend, 0, 0, slide.MaxScore), debugView: true)
									: (dynamic)null
							).ToArray();
					
WriteLiteralTo(__razor_helper_writer, "\r\n");

					
                      
						var baseUrl = "/"+slide.Info.Directory.FullName;
					
WriteLiteralTo(__razor_helper_writer, "\r\n");

WriteLiteralTo(__razor_helper_writer, "\t\t\t\t\t");

WriteTo(__razor_helper_writer, SlideHtml.Slide(new BlockRenderContext(course, slide, baseUrl, 
						blockData,
						isGuest: false,
						revealHidden: true
						),
						null));

WriteLiteralTo(__razor_helper_writer, "\r\n\t\t\t\t\t<div");

WriteLiteralTo(__razor_helper_writer, " style=\"margin-bottom: 40px;\"");

WriteLiteralTo(__razor_helper_writer, ">\r\n\t\t\t\t\t\t<a");

WriteAttributeTo(__razor_helper_writer, "href", Tuple.Create(" href=\"", 2063), Tuple.Create("\"", 2122)
, Tuple.Create(Tuple.Create("", 2070), Tuple.Create("/", 2070), true)
, Tuple.Create(Tuple.Create("", 2071), Tuple.Create<System.Object, System.Int32>(slide.Index.ToString("000")
, 2071), false)
, Tuple.Create(Tuple.Create("", 2101), Tuple.Create(".html?query=addLesson", 2101), true)
);

WriteLiteralTo(__razor_helper_writer, " class=\"btn btn-default\"");

WriteLiteralTo(__razor_helper_writer, ">Добавить слайд</a>\r\n\t\t\t\t\t\t<a");

WriteAttributeTo(__razor_helper_writer, "href", Tuple.Create(" href=\"", 2176), Tuple.Create("\"", 2233)
, Tuple.Create(Tuple.Create("", 2183), Tuple.Create("/", 2183), true)
, Tuple.Create(Tuple.Create("", 2184), Tuple.Create<System.Object, System.Int32>(slide.Index.ToString("000")
, 2184), false)
, Tuple.Create(Tuple.Create("", 2214), Tuple.Create(".html?query=addQuiz", 2214), true)
);

WriteLiteralTo(__razor_helper_writer, " class=\"btn btn-default\"");

WriteLiteralTo(__razor_helper_writer, ">Добавить тест</a>\r\n\t\t\t\t\t</div>\r\n\t\t\t\t</div>\r\n\t\t\t</div>\r\n\t\t</div>\r\n\r\n\r\n");

		
         foreach (var jsFile in jsFiles)
		{

WriteLiteralTo(__razor_helper_writer, "\t\t\t<script");

WriteAttributeTo(__razor_helper_writer, "src", Tuple.Create(" src=\'", 2379), Tuple.Create("\'", 2392)
, Tuple.Create(Tuple.Create("", 2385), Tuple.Create<System.Object, System.Int32>(jsFile
, 2385), false)
);

WriteLiteralTo(__razor_helper_writer, "></script>\r\n");

		}

WriteLiteralTo(__razor_helper_writer, "        <script>\r\n            for (var i = 0; i < window.documentReadyFunctions.l" +
"ength; i++) {\r\n                var f = window.documentReadyFunctions[i];\r\n      " +
"          f();\r\n            }\r\n        </script>\r\n\t</body>\r\n\t</html>\r\n");


});

#line default
#line hidden
}
#line default
#line hidden

    }
}
#pragma warning restore 1591
