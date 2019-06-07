using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class SynchronizeWithNotCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_CreatedGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SystemMessageNotification_Text",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupId",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "CreatedGroupNotification_GroupId",
                table: "Notifications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemMessageNotification_Text",
                table: "Notifications",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedGroupNotification_GroupId",
                table: "Notifications",
                column: "CreatedGroupNotification_GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_CreatedGroupNotification_GroupId",
                table: "Notifications",
                column: "CreatedGroupNotification_GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
