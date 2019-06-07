using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddIpAddressToVisit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Visits",
                nullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(short));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Visits");

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);
        }
    }
}
