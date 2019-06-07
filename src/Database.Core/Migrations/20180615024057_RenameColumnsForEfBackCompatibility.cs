using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class RenameColumnsForEfBackCompatibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_CommentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_NewCommentNotification_CommentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_RepliedToYourCommentNotification_CommentId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_NewCommentNotification_CommentId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "NewCommentNotification_CommentId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "RepliedToYourCommentNotification_CommentId",
                table: "Notifications",
                newName: "CommentId1");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RepliedToYourCommentNotification_CommentId",
                table: "Notifications",
                newName: "IX_Notifications_CommentId11");

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(short));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CommentId1",
                table: "Notifications",
                column: "CommentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications",
                column: "CommentId1",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_CommentId11",
                table: "Notifications",
                column: "CommentId1",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ExerciseCodeReviewComments_CommentId",
                table: "Notifications",
                column: "CommentId",
                principalTable: "ExerciseCodeReviewComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_CommentId11",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ExerciseCodeReviewComments_CommentId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CommentId1",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "CommentId1",
                table: "Notifications",
                newName: "RepliedToYourCommentNotification_CommentId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_CommentId11",
                table: "Notifications",
                newName: "IX_Notifications_RepliedToYourCommentNotification_CommentId");

            migrationBuilder.AddColumn<int>(
                name: "NewCommentNotification_CommentId",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NewCommentNotification_CommentId",
                table: "Notifications",
                column: "NewCommentNotification_CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_CommentId",
                table: "Notifications",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_NewCommentNotification_CommentId",
                table: "Notifications",
                column: "NewCommentNotification_CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_RepliedToYourCommentNotification_CommentId",
                table: "Notifications",
                column: "RepliedToYourCommentNotification_CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
