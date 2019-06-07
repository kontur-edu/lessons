using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddCertificateTemplateArchive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ArchiveName",
                table: "CertificateTemplates",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateTable(
                name: "CertificateTemplateArchives",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    Content = table.Column<byte[]>(nullable: false),
                    CertificateTemplateId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplateArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateTemplateArchives_CertificateTemplates_CertificateTemplateId",
                        column: x => x.CertificateTemplateId,
                        principalTable: "CertificateTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplateArchives_CertificateTemplateId",
                table: "CertificateTemplateArchives",
                column: "CertificateTemplateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateTemplateArchives");

            migrationBuilder.AlterColumn<string>(
                name: "ArchiveName",
                table: "CertificateTemplates",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 128);
        }
    }
}
