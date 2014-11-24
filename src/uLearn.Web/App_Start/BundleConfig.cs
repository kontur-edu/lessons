using System.Web.Optimization;

namespace uLearn.Web
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/jquery-js").Include(
				"~/Scripts/jquery-{version}.js",
				"~/Scripts/jquery.unobtrusive-ajax*",
				"~/Scripts/notify-custom.min.js"));

			bundles.Add(new ScriptBundle("~/jqueryval-js").Include(
				"~/Scripts/jquery.validate*"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/modernizr-js").Include(
				"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/slide-js").Include(
				"~/katex/katex.min.js",
				"~/Jsdifflib/jsdifflib.js",
				"~/Scripts/slide-*"
				));

			bundles.Add(new ScriptBundle("~/codemirror-js").Include(
				"~/codemirror/lib/codemirror.js",
				"~/codemirror/mode/clike/clike.js",
				"~/codemirror/addon/hint/show-hint.js",
				"~/codemirror/addon/hint/cscompleter.js",
				"~/codemirror/addon/hint/csharp-hint.js",
				"~/codemirror/addon/edit/closebrackets.js",
				"~/codemirror/addon/edit/matchbrackets.js",
				"~/codemirror/addon/selection/active-line.js"
				));

			bundles.Add(new ScriptBundle("~/bootstrap-js").Include(
				"~/Scripts/bootstrap.js",
				"~/Scripts/buttons.js",
				"~/Scripts/respond.js"));

			bundles.Add(new StyleBundle("~/css").Include(
				"~/katex/katex.min.css",
				"~/Content/bootstrap.css",
				"~/codemirror/lib/codemirror.css",
				"~/codemirror/theme/cobalt.css",
				"~/codemirror/addon/hint/show-hint.css",
				"~/Jsdifflib/jsdifflib.css",
				"~/Content/tablesorter.css",
				"~/Content/ulearn.css",
				"~/Content/bounce.css",
				"~/Content/site.css"));
			
		}
	}
}