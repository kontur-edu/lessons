using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddSubmissionsLanguageAndAgentName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Language",
                table: "UserExerciseSubmissions",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "CheckingAgentName",
                table: "AutomaticExerciseCheckings",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_Language",
                table: "UserExerciseSubmissions",
                column: "Language");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserExerciseSubmissions_Language",
                table: "UserExerciseSubmissions");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "UserExerciseSubmissions");

            migrationBuilder.DropColumn(
                name: "CheckingAgentName",
                table: "AutomaticExerciseCheckings");
        }
    }
}
