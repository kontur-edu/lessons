using Database;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Ulearn.Common.Extensions;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/account")]
	public class AccountController : BaseController
	{
		public AccountController(ILogger logger, WebCourseManager courseManager)
			: base(logger, courseManager)
		{
		}

		[HttpGet]
		public IActionResult Me()
		{
			return Json(new {
				userId = User.GetUserId()
			});
		}
	}
}