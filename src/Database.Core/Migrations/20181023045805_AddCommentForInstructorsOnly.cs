using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddCommentForInstructorsOnly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_GroupId",
                table: "Notifications",
                newName: "IX_Notifications_GroupId2");

            migrationBuilder.AddColumn<int>(
                name: "CreatedGroupNotification_GroupId",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserDescriptions",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserIds",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CertificateId",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemMessageNotification_Text",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForInstructorsOnly",
                table: "Comments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedGroupNotification_GroupId",
                table: "Notifications",
                column: "CreatedGroupNotification_GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId1",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CertificateId",
                table: "Notifications",
                column: "CertificateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_CreatedGroupNotification_GroupId",
                table: "Notifications",
                column: "CreatedGroupNotification_GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId2",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Certificates_CertificateId",
                table: "Notifications",
                column: "CertificateId",
                principalTable: "Certificates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_CreatedGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId2",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Certificates_CertificateId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupId1",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CertificateId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserDescriptions",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserIds",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CertificateId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SystemMessageNotification_Text",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsForInstructorsOnly",
                table: "Comments");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_GroupId2",
                table: "Notifications",
                newName: "IX_Notifications_GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
