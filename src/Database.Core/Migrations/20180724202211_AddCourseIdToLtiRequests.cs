using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddCourseIdToLtiRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LtiSlideRequests_SlideId_UserId",
                table: "LtiSlideRequests");

            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "LtiSlideRequests",
                nullable: false,
				maxLength: 100,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LtiSlideRequests_CourseId_SlideId_UserId",
                table: "LtiSlideRequests",
                columns: new[] { "CourseId", "SlideId", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LtiSlideRequests_CourseId_SlideId_UserId",
                table: "LtiSlideRequests");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "LtiSlideRequests");

            migrationBuilder.CreateIndex(
                name: "IX_LtiSlideRequests_SlideId_UserId",
                table: "LtiSlideRequests",
                columns: new[] { "SlideId", "UserId" });
        }
    }
}
