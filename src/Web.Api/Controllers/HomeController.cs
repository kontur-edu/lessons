using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Vostok.Tracing;

namespace Ulearn.Web.Api.Controllers
{
    [Route("/home")]
    public class HomeController : Controller
    {
	   [HttpGet("{*url}")]
        public object Echo()
		{
			return Json(new
            {
			    url = Request.GetUri(),
                traceUrl = $"http://localhost:6301/{TraceContext.Current.TraceId}",
                traceId = TraceContext.Current.TraceId
            });
        }
    }
}
