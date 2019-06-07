using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddUserQuizSubmission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
		{
            migrationBuilder.DropTable(
                name: "UserQuizs");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ManualQuizCheckings",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AutomaticQuizCheckings",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateTable(
                name: "UserQuizSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 40, nullable: false),
                    CourseId = table.Column<string>(maxLength: 40, nullable: false),
                    SlideId = table.Column<Guid>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizSubmissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubmissionId = table.Column<int>(nullable: false),
                    BlockId = table.Column<string>(maxLength: 64, nullable: true),
                    ItemId = table.Column<string>(maxLength: 64, nullable: true),
                    Text = table.Column<string>(maxLength: 8192, nullable: true),
                    IsRightAnswer = table.Column<bool>(nullable: false),
                    QuizBlockScore = table.Column<int>(nullable: false),
                    QuizBlockMaxScore = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizAnswers_UserQuizSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "UserQuizSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAnswers_ItemId",
                table: "UserQuizAnswers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAnswers_SubmissionId_BlockId",
                table: "UserQuizAnswers",
                columns: new[] { "SubmissionId", "BlockId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_UserId",
                table: "UserQuizSubmissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_CourseId_SlideId",
                table: "UserQuizSubmissions",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_CourseId_SlideId_Timestamp",
                table: "UserQuizSubmissions",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_CourseId_SlideId_UserId",
                table: "UserQuizSubmissions",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AutomaticQuizCheckings_UserQuizSubmissions_Id",
                table: "AutomaticQuizCheckings",
                column: "Id",
                principalTable: "UserQuizSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ManualQuizCheckings_UserQuizSubmissions_Id",
                table: "ManualQuizCheckings",
                column: "Id",
                principalTable: "UserQuizSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
		{
            migrationBuilder.DropForeignKey(
                name: "FK_AutomaticQuizCheckings_UserQuizSubmissions_Id",
                table: "AutomaticQuizCheckings");

            migrationBuilder.DropForeignKey(
                name: "FK_ManualQuizCheckings_UserQuizSubmissions_Id",
                table: "ManualQuizCheckings");

            migrationBuilder.DropTable(
                name: "UserQuizAnswers");

            migrationBuilder.DropTable(
                name: "UserQuizSubmissions");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ManualQuizCheckings",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AutomaticQuizCheckings",
                nullable: false,
                oldClrType: typeof(int))
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateTable(
                name: "UserQuizs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<string>(maxLength: 64, nullable: false),
                    IsRightAnswer = table.Column<bool>(nullable: false),
                    ItemId = table.Column<string>(maxLength: 64, nullable: true),
                    QuizBlockMaxScore = table.Column<int>(nullable: false),
                    QuizBlockScore = table.Column<int>(nullable: false),
                    QuizId = table.Column<string>(maxLength: 64, nullable: true),
                    SlideId = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(maxLength: 8192, nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(maxLength: 64, nullable: false),
                    isDropped = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_ItemId",
                table: "UserQuizs",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_UserId",
                table: "UserQuizs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_SlideId_Timestamp",
                table: "UserQuizs",
                columns: new[] { "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_CourseId_SlideId_QuizId",
                table: "UserQuizs",
                columns: new[] { "CourseId", "SlideId", "QuizId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizs_CourseId_UserId_SlideId_isDropped_QuizId_ItemId",
                table: "UserQuizs",
                columns: new[] { "CourseId", "UserId", "SlideId", "isDropped", "QuizId", "ItemId" });
        }
    }
}
