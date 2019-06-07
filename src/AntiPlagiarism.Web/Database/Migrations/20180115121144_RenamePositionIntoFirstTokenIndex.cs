using Microsoft.EntityFrameworkCore.Migrations;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class RenamePositionIntoFirstTokenIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Position",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                newName: "FirstTokenIndex");

            migrationBuilder.RenameIndex(
                name: "IX_SnippetsOccurences_SubmissionId_Position",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                newName: "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstTokenIndex",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                newName: "Position");

            migrationBuilder.RenameIndex(
                name: "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                newName: "IX_SnippetsOccurences_SubmissionId_Position");
        }
    }
}
