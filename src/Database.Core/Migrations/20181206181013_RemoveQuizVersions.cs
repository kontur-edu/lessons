using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class RemoveQuizVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizs_QuizVersions_QuizVersionId",
                table: "UserQuizs");

            migrationBuilder.DropTable(
                name: "QuizVersions");

            migrationBuilder.DropIndex(
                name: "IX_UserQuizs_QuizVersionId",
                table: "UserQuizs");

            migrationBuilder.DropColumn(
                name: "QuizVersionId",
                table: "UserQuizs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizVersionId",
                table: "UserQuizs",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuizVersions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<string>(maxLength: 64, nullable: false),
                    LoadingTime = table.Column<DateTime>(nullable: false),
                    NormalizedXml = table.Column<string>(nullable: false),
                    SlideId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_QuizVersionId",
                table: "UserQuizs",
                column: "QuizVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizVersions_SlideId",
                table: "QuizVersions",
                column: "SlideId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizVersions_SlideId_LoadingTime",
                table: "QuizVersions",
                columns: new[] { "SlideId", "LoadingTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizs_QuizVersions_QuizVersionId",
                table: "UserQuizs",
                column: "QuizVersionId",
                principalTable: "QuizVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
