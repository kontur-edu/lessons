using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class CascadeDeletionForGroupAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_GrantedById",
                table: "GroupAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_UserId",
                table: "GroupAccesses");

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(short));

            migrationBuilder.AddForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_GrantedById",
                table: "GroupAccesses",
                column: "GrantedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_UserId",
                table: "GroupAccesses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_GrantedById",
                table: "GroupAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_UserId",
                table: "GroupAccesses");

            migrationBuilder.AlterColumn<short>(
                name: "Gender",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_GrantedById",
                table: "GroupAccesses",
                column: "GrantedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupAccesses_AspNetUsers_UserId",
                table: "GroupAccesses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
