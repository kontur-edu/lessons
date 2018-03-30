using System.Web.Mvc;

namespace uLearn.Web.Controllers
{
	public class ErrorsController : Controller
	{
		public ActionResult Error404()
		{
			return View("404");
		}
		
		public ActionResult Error500()
		{
			return View("Error");
		}
	}
}