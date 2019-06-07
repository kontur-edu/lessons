using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddCourseIdToSlideHintIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SlideHints_SlideId_HintId_UserId_IsHintHelped",
                table: "SlideHints");

            migrationBuilder.CreateIndex(
                name: "IX_SlideHints_CourseId_SlideId_HintId_UserId_IsHintHelped",
                table: "SlideHints",
                columns: new[] { "CourseId", "SlideId", "HintId", "UserId", "IsHintHelped" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SlideHints_CourseId_SlideId_HintId_UserId_IsHintHelped",
                table: "SlideHints");

            migrationBuilder.CreateIndex(
                name: "IX_SlideHints_SlideId_HintId_UserId_IsHintHelped",
                table: "SlideHints",
                columns: new[] { "SlideId", "HintId", "UserId", "IsHintHelped" });
        }
    }
}
