using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class RemoveUserExerciseSubmissionsApplicationUserField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExerciseSubmissions_AspNetUsers_ApplicationUserId",
                table: "UserExerciseSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_UserExerciseSubmissions_ApplicationUserId",
                table: "UserExerciseSubmissions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "UserExerciseSubmissions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "UserExerciseSubmissions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_ApplicationUserId",
                table: "UserExerciseSubmissions",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserExerciseSubmissions_AspNetUsers_ApplicationUserId",
                table: "UserExerciseSubmissions",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
