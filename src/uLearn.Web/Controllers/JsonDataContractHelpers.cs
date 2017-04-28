using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace uLearn.Web.Controllers
{
	public class JsonDataContractResult : JsonResult
	{
		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
				string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Get is not allowed");
			}

			var response = context.HttpContext.Response;

			response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
			if (ContentEncoding != null)
				response.ContentEncoding = ContentEncoding;
			
			if (Data != null)
			{
				// Use the DataContractJsonSerializer instead of the JavaScriptSerializer 
				var serializer = new DataContractJsonSerializer(Data.GetType());
				serializer.WriteObject(response.OutputStream, Data);
			}
		}
	}

	public class JsonDataContractModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			// Ignore non-JSON request
			if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
				return null;

			var request = controllerContext.HttpContext.Request;
			request.InputStream.Position = 0;
			var incomingData = new StreamReader(request.InputStream).ReadToEnd();

			// If no JSON data
			if (string.IsNullOrEmpty(incomingData))
				return null;

			return JsonConvert.DeserializeObject(incomingData, bindingContext.ModelType);
		}
	}

	public class JsonDataContractModelBinderAttribute : CustomModelBinderAttribute
	{
		public override IModelBinder GetBinder()
		{
			return new JsonDataContractModelBinder();
		}
	}
}