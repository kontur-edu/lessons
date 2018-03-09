using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Database.Migrations
{
    public partial class AddExerciseCodeReviewComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AddingTime",
                table: "ExerciseCodeReviews",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ExerciseCodeReviewComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddingTime = table.Column<DateTime>(nullable: false),
                    AuthorId = table.Column<string>(maxLength: 64, nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ReviewId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseCodeReviewComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseCodeReviewComments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseCodeReviewComments_ExerciseCodeReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "ExerciseCodeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviewComments_AddingTime",
                table: "ExerciseCodeReviewComments",
                column: "AddingTime");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviewComments_AuthorId",
                table: "ExerciseCodeReviewComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviewComments_ReviewId_IsDeleted",
                table: "ExerciseCodeReviewComments",
                columns: new[] { "ReviewId", "IsDeleted" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseCodeReviewComments");

            migrationBuilder.DropColumn(
                name: "AddingTime",
                table: "ExerciseCodeReviews");
        }
    }
}
