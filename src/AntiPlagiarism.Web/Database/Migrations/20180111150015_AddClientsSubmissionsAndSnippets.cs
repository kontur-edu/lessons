using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class AddClientsSubmissionsAndSnippets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "antiplagiarism");

            migrationBuilder.CreateTable(
                name: "Clients",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Token = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Codes",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Snippets",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Hash = table.Column<int>(nullable: false),
                    SnippetType = table.Column<short>(nullable: false),
                    TokensCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snippets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdditionalInfo = table.Column<string>(maxLength: 500, nullable: true),
                    AuthorId = table.Column<Guid>(nullable: false),
                    ClientId = table.Column<int>(nullable: false),
                    ProgramId = table.Column<int>(nullable: false),
                    TaskId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submissions_Codes_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SnippetOccurences",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Position = table.Column<int>(nullable: false),
                    SnippetId = table.Column<int>(nullable: false),
                    SubmissionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnippetOccurences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnippetOccurences_Snippets_SnippetId",
                        column: x => x.SnippetId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Snippets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SnippetOccurences_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Token",
                schema: "antiplagiarism",
                table: "Clients",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnippetOccurences_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetOccurences",
                column: "SnippetId");

            migrationBuilder.CreateIndex(
                name: "IX_SnippetOccurences_SubmissionId_Position",
                schema: "antiplagiarism",
                table: "SnippetOccurences",
                columns: new[] { "SubmissionId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Snippets_TokensCount_SnippetType_Hash",
                schema: "antiplagiarism",
                table: "Snippets",
                columns: new[] { "TokensCount", "SnippetType", "Hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ProgramId",
                schema: "antiplagiarism",
                table: "Submissions",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "AuthorId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SnippetOccurences",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "Snippets",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "Submissions",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "Clients",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "Codes",
                schema: "antiplagiarism");
        }
    }
}
