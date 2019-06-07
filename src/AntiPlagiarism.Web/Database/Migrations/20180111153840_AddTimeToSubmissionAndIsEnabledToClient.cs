using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class AddTimeToSubmissionAndIsEnabledToClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddingTime",
                schema: "antiplagiarism",
                table: "Submissions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                schema: "antiplagiarism",
                table: "Clients",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddingTime",
                schema: "antiplagiarism",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                schema: "antiplagiarism",
                table: "Clients");
        }
    }
}
