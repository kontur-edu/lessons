using System;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using NUnit.Framework;

namespace uLearn.Web
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(JsBundle());

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/modernizr-js").Include("~/Scripts/modernizr-*"));

			bundles.Add(CssBundle());
		}

		public static Bundle JsBundle()
		{
			return new ScriptBundle("~/main-js").Include(
				new string[]
				{
					"~/Scripts/jquery-{version}.js",
					"~/Scripts/jquery.unobtrusive-ajax*",
					"~/Scripts/notify-custom.min.js",
					"~/Scripts/jquery.validate*",
					"~/flexslider/jquery.flexslider.js",
					"~/katex/katex.min.js",
//					"~/Jsdifflib/jsdifflib.js",
					"~/codemirror/lib/codemirror.js",
					"~/codemirror/mode/clike/clike.js",
					"~/codemirror/mode/python/python.js",
					"~/codemirror/addon/hint/show-hint.js",
					"~/codemirror/addon/hint/cscompleter.js",
					"~/codemirror/addon/hint/csharp-hint.js",
					"~/codemirror/addon/edit/closebrackets.js",
					"~/codemirror/addon/edit/matchbrackets.js",
					"~/codemirror/addon/selection/active-line.js",
					"~/Scripts/bootstrap.js",
					"~/Scripts/bootstrap.file-input.js",
					"~/Scripts/buttons.js",
					"~/Scripts/respond.js",
					"~/Scripts/slide-*",
					"~/Scripts/users-list.js"
				}
				);
		}

		public static Bundle CssBundle()
		{
			return new StyleBundle("~/css").Include(
				"~/katex/katex.min.css",
				"~/Content/bootstrap.css",
				"~/codemirror/lib/codemirror.css",
				"~/codemirror/theme/cobalt.css",
				"~/codemirror/addon/hint/show-hint.css",
//				"~/Jsdifflib/jsdifflib.css",
				"~/Content/tablesorter.css",
				"~/Content/ulearn.css",
				"~/Content/bounce.css",
				"~/flexslider/flexslider.css",
				"~/Content/site.css");
		}
	}
}