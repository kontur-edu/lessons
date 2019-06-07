using System.Threading.Tasks;
using Database.Repos.Groups;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Ulearn.Core.Courses;
using Ulearn.Web.Api.Controllers.Groups;
using Ulearn.Web.Api.Models.Parameters.Groups;

namespace Web.Api.Tests.Controllers.Groups
{
	[TestFixture]
	public class GroupsControllerTests : BaseControllerTests
	{
		private GroupsController groupsController;
		private IGroupsRepo groupsRepo;

		[SetUp]
		public void SetUp()
		{
			SetupTestInfrastructureAsync(services => services.AddScoped<GroupsController>()).GetAwaiter().GetResult();

			groupsController = GetController<GroupsController>();
			groupsRepo = serviceProvider.GetService<IGroupsRepo>();
		}

		[Test]
		public async Task GroupsList()
		{
			var course = new Mock<ICourse>();
			course.Setup(c => c.Id).Returns("courseId");

			await groupsRepo.CreateGroupAsync("courseId", "Test group", TestUsers.Admin.Id).ConfigureAwait(false);
			
			await AuthenticateUserInControllerAsync(groupsController, TestUsers.Admin).ConfigureAwait(false);

			var result = await groupsController.GroupsList(new GroupsListParameters { CourseId = "courseId" }).ConfigureAwait(false);
			Assert.AreEqual(1, result.Value.Groups.Count);
		}
	}
}