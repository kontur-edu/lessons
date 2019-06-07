using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class RemoveRequiredAutomaticChecking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExerciseSubmissions_AutomaticExerciseCheckings_AutomaticCheckingId",
                table: "UserExerciseSubmissions");

            migrationBuilder.AlterColumn<int>(
                name: "AutomaticCheckingId",
                table: "UserExerciseSubmissions",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(short));

            migrationBuilder.AddForeignKey(
                name: "FK_UserExerciseSubmissions_AutomaticExerciseCheckings_AutomaticCheckingId",
                table: "UserExerciseSubmissions",
                column: "AutomaticCheckingId",
                principalTable: "AutomaticExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserExerciseSubmissions_AutomaticExerciseCheckings_AutomaticCheckingId",
                table: "UserExerciseSubmissions");

            migrationBuilder.AlterColumn<int>(
                name: "AutomaticCheckingId",
                table: "UserExerciseSubmissions",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserExerciseSubmissions_AutomaticExerciseCheckings_AutomaticCheckingId",
                table: "UserExerciseSubmissions",
                column: "AutomaticCheckingId",
                principalTable: "AutomaticExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
