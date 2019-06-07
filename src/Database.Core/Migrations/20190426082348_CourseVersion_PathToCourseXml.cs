using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class CourseVersion_PathToCourseXml : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PathToCourseXml",
                table: "CourseVersions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathToCourseXml",
                table: "CourseVersions");
        }
    }
}
