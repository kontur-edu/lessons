using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddIndiciesToUserQuizzes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_ItemId",
                table: "UserQuizs",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_CourseId_SlideId_QuizId",
                table: "UserQuizs",
                columns: new[] { "CourseId", "SlideId", "QuizId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserQuizs_ItemId",
                table: "UserQuizs");

            migrationBuilder.DropIndex(
                name: "IX_UserQuizs_CourseId_SlideId_QuizId",
                table: "UserQuizs");
        }
    }
}
