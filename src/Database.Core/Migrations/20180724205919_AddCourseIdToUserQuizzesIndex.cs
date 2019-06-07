using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddCourseIdToUserQuizzesIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserQuizs_UserId_SlideId_isDropped_QuizId_ItemId",
                table: "UserQuizs");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_UserId",
                table: "UserQuizs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_CourseId_UserId_SlideId_isDropped_QuizId_ItemId",
                table: "UserQuizs",
                columns: new[] { "CourseId", "UserId", "SlideId", "isDropped", "QuizId", "ItemId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserQuizs_UserId",
                table: "UserQuizs");

            migrationBuilder.DropIndex(
                name: "IX_UserQuizs_CourseId_UserId_SlideId_isDropped_QuizId_ItemId",
                table: "UserQuizs");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_UserId_SlideId_isDropped_QuizId_ItemId",
                table: "UserQuizs",
                columns: new[] { "UserId", "SlideId", "isDropped", "QuizId", "ItemId" });
        }
    }
}
