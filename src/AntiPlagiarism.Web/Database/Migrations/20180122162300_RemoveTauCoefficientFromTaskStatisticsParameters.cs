using Microsoft.EntityFrameworkCore.Migrations;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class RemoveTauCoefficientFromTaskStatisticsParameters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TauCoefficient",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TauCoefficient",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
