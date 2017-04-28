using System.Web.Optimization;

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
					"~/Scripts/jquery-{version}.js",
					"~/Scripts/jquery-ui.min.js",
					"~/Scripts/jquery.unobtrusive-ajax*",
					"~/Scripts/notify-custom.min.js",
					"~/Scripts/jquery.query-object.js",
					"~/Scripts/jquery.validate.js",
					"~/Scripts/jquery.validate-vsdoc.js",
					"~/Scripts/jquery.validate.unobtrusive.js",
					"~/Scripts/jquery.validate.date.js",
					"~/Scripts/jquery.datetimepicker.full.min.js",
					"~/Scripts/jquery.datetimepicker.unobtrusive.js",
					"~/Scripts/jquery.event.move.js",
					"~/Scripts/jquery.smooth-scrolling.js",
					"~/Scripts/js.cookie.js",
					"~/tablesorter-master/js/jquery.tablesorter.min.js",
					"~/tablesorter-master/js/jquery.tablesorter.widgets.min.js",
					"~/tablesorter-master/js/widgets/widget-grouping.js",
					"~/Scripts/table-configurator.js",
					"~/flexslider/jquery.flexslider.js",
					"~/katex/katex.min.js",
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
					"~/Scripts/bootstrap-select.min.js",
					"~/Scripts/bootstrap-select.additional.js",
					"~/Scripts/tooltips.js",
					"~/Scripts/clipboard.min.js",
					"~/Scripts/activate-clipboard.js",
					"~/Scripts/buttons.js",
					"~/Scripts/respond.js",
					"~/Scripts/slide-*",
					"~/Scripts/users-list.js",
					"~/Scripts/groups-editor.js",
					"~/Scripts/certificates-editor.js",
					"~/Scripts/analytics-highcharts.js",
					"~/Scripts/likely.js",
					"~/Scripts/analytics.js",
					"~/Scripts/course-statistics.js",
					"~/Scripts/ulearn-updates-invitation.js",
					"~/Scripts/diagnostics.js",
					"~/Scripts/forms.replace-action.js",
					"~/Scripts/grader.js"
				);
		}

		public static Bundle CssBundle()
		{
			return new StyleBundle("~/css").Include(
				"~/katex/katex.min.css",
				"~/Content/bootstrap.css",
				"~/Content/font-awesome.css",
				"~/Content/awesome-bootstrap-checkbox.css",
				"~/Content/bootstrap-select.min.css",
				"~/Content/bootstrap.refresh-animate.css",
				"~/Content/jquery.datetimepicker.min.css",
				"~/codemirror/lib/codemirror.css",
				"~/codemirror/theme/cobalt.css",
				"~/codemirror/addon/hint/show-hint.css",
//				"~/Jsdifflib/jsdifflib.css",
				"~/Content/tablesorter.css",
				"~/tablesorter-master/css/widget.grouping.css",
				"~/Content/ulearn.css",
				"~/Content/bounce.css",
				"~/flexslider/flexslider.css",
				"~/Content/jquery-ui.min.css",
				"~/Content/site.css",
				"~/Content/buttons.css",
				"~/Content/likely.css"
				);
		}
	}
}