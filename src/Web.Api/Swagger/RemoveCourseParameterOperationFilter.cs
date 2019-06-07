using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Ulearn.Core.Courses;

namespace Ulearn.Web.Api.Swagger
{
	public class RemoveCourseParameterOperationFilter : IOperationFilter
	{
		public void Apply(Operation operation, OperationFilterContext context)
		{
			var parametersWithCourseType = context.ApiDescription
				.ParameterDescriptions
				.Where(desc => desc.ParameterDescriptor?.ParameterType == typeof(Course) || desc.ParameterDescriptor?.ParameterType == typeof(ICourse))
				.ToList();
				
			parametersWithCourseType
				.ForEach(param =>
				{
					var toRemove = operation.Parameters.SingleOrDefault(p => p.Name == param.Name);

					if (toRemove != null)
						operation.Parameters.Remove(toRemove);
				});
		}
	}
}