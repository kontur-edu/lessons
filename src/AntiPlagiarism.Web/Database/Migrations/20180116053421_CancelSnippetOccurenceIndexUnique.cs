using Microsoft.EntityFrameworkCore.Migrations;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class CancelSnippetOccurenceIndexUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex",
                schema: "antiplagiarism",
                table: "SnippetsOccurences");

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                columns: new[] { "SubmissionId", "FirstTokenIndex" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex",
                schema: "antiplagiarism",
                table: "SnippetsOccurences");

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                columns: new[] { "SubmissionId", "FirstTokenIndex" },
                unique: true);
        }
    }
}
