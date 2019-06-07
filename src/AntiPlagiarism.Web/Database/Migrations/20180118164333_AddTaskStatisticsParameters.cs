using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class AddTaskStatisticsParameters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TasksStatisticsParameters",
                schema: "antiplagiarism",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(nullable: false),
                    Deviation = table.Column<double>(nullable: false),
                    Mean = table.Column<double>(nullable: false),
                    TauCoefficient = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TasksStatisticsParameters", x => x.TaskId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TasksStatisticsParameters",
                schema: "antiplagiarism");
        }
    }
}
