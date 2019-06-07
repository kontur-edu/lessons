using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class DeleteNotificationDeliveryOnDeleteNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationDeliveries_Notifications_NotificationId",
                table: "NotificationDeliveries");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationDeliveries_Notifications_NotificationId",
                table: "NotificationDeliveries",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationDeliveries_Notifications_NotificationId",
                table: "NotificationDeliveries");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationDeliveries_Notifications_NotificationId",
                table: "NotificationDeliveries",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
