using Microsoft.EntityFrameworkCore.Migrations;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class AddLanguageIndexToSubmissionsAndSnippetIndexToOccurence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_Language_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "Language", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsOccurences_SubmissionId_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                columns: new[] { "SubmissionId", "SnippetId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_ClientId_TaskId_Language_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_SnippetsOccurences_SubmissionId_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences");
        }
    }
}
