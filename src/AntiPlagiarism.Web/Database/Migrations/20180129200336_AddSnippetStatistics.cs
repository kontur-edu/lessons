using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class AddSnippetStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorsCount",
                schema: "antiplagiarism",
                table: "Snippets");

            migrationBuilder.CreateTable(
                name: "SnippetsStatistics",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthorsCount = table.Column<int>(nullable: false),
                    ClientId = table.Column<int>(nullable: false),
                    SnippetId = table.Column<int>(nullable: false),
                    TaskId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnippetsStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnippetsStatistics_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SnippetsStatistics_Snippets_SnippetId",
                        column: x => x.SnippetId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Snippets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsStatistics_ClientId",
                schema: "antiplagiarism",
                table: "SnippetsStatistics",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsStatistics_SnippetId_TaskId_ClientId",
                schema: "antiplagiarism",
                table: "SnippetsStatistics",
                columns: new[] { "SnippetId", "TaskId", "ClientId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SnippetsStatistics",
                schema: "antiplagiarism");

            migrationBuilder.AddColumn<int>(
                name: "AuthorsCount",
                schema: "antiplagiarism",
                table: "Snippets",
                nullable: false,
                defaultValue: 0);
        }
    }
}
