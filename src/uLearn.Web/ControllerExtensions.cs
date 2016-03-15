using System.IO;
using System.Web.Mvc;

namespace uLearn.Web
{
	public static class ControllerExtensions
	{
		public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
		{
			var vd = new ViewDataDictionary();
			foreach (var item in controller.ViewData.Keys)
			{
				vd.Add(item, controller.ViewData[item]);
			}
			vd.Model = model;
			using (var sw = new StringWriter())
			{
				var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
				var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, vd, controller.TempData, sw);
				viewResult.View.Render(viewContext, sw);

				return sw.GetStringBuilder().ToString();
			}

		}
	}
}