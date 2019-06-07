using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class CourseVersionGit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommitHash",
                table: "CourseVersions",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CourseVersions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepoUrl",
                table: "CourseVersions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommitHash",
                table: "CourseVersions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CourseVersions");

            migrationBuilder.DropColumn(
                name: "RepoUrl",
                table: "CourseVersions");
        }
    }
}
