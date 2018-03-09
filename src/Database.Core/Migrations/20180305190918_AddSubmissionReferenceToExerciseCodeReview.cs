using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Database.Migrations
{
    public partial class AddSubmissionReferenceToExerciseCodeReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckingId",
                table: "ExerciseCodeReviews");

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseCheckingId",
                table: "ExerciseCodeReviews",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "SubmissionId",
                table: "ExerciseCodeReviews",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviews_SubmissionId",
                table: "ExerciseCodeReviews",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckingId",
                table: "ExerciseCodeReviews",
                column: "ExerciseCheckingId",
                principalTable: "ManualExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCodeReviews_UserExerciseSubmissions_SubmissionId",
                table: "ExerciseCodeReviews",
                column: "SubmissionId",
                principalTable: "UserExerciseSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckingId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCodeReviews_UserExerciseSubmissions_SubmissionId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseCodeReviews_SubmissionId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropColumn(
                name: "SubmissionId",
                table: "ExerciseCodeReviews");

            migrationBuilder.AlterColumn<int>(
                name: "ExerciseCheckingId",
                table: "ExerciseCodeReviews",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckingId",
                table: "ExerciseCodeReviews",
                column: "ExerciseCheckingId",
                principalTable: "ManualExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
