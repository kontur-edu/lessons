using System.Text;
using System.Web.Mvc;

namespace uLearn.Web.Controllers
{
	public class JsonDataContractController : Controller
	{
		/* Replace JsonResult with JsonDataContractResult */
		protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
		{
			return new JsonDataContractResult
			{
				Data = data,
				ContentType = contentType,
				ContentEncoding = contentEncoding,
				JsonRequestBehavior = behavior
			};
		}
	}
}