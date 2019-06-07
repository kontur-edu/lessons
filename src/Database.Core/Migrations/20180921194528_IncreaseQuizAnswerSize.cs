using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class IncreaseQuizAnswerSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "UserQuizs",
                maxLength: 8192,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1024,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "UserQuizs",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 8192,
                oldNullable: true);
        }
    }
}
