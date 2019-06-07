using Microsoft.EntityFrameworkCore.Migrations;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class RenameSnippetsOccurences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SnippetOccurences_Snippets_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetOccurences");

            migrationBuilder.DropForeignKey(
                name: "FK_SnippetOccurences_Submissions_SubmissionId",
                schema: "antiplagiarism",
                table: "SnippetOccurences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SnippetOccurences",
                schema: "antiplagiarism",
                table: "SnippetOccurences");

            migrationBuilder.RenameTable(
                name: "SnippetOccurences",
                schema: "antiplagiarism",
                newName: "SnippetsOccurences");

            migrationBuilder.RenameIndex(
                name: "IX_SnippetOccurences_SubmissionId_Position",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                newName: "IX_SnippetsOccurences_SubmissionId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_SnippetOccurences_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                newName: "IX_SnippetsOccurences_SnippetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SnippetsOccurences",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SnippetsOccurences_Snippets_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                column: "SnippetId",
                principalSchema: "antiplagiarism",
                principalTable: "Snippets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SnippetsOccurences_Submissions_SubmissionId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                column: "SubmissionId",
                principalSchema: "antiplagiarism",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SnippetsOccurences_Snippets_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences");

            migrationBuilder.DropForeignKey(
                name: "FK_SnippetsOccurences_Submissions_SubmissionId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SnippetsOccurences",
                schema: "antiplagiarism",
                table: "SnippetsOccurences");

            migrationBuilder.RenameTable(
                name: "SnippetsOccurences",
                schema: "antiplagiarism",
                newName: "SnippetOccurences");

            migrationBuilder.RenameIndex(
                name: "IX_SnippetsOccurences_SubmissionId_Position",
                schema: "antiplagiarism",
                table: "SnippetOccurences",
                newName: "IX_SnippetOccurences_SubmissionId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_SnippetsOccurences_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetOccurences",
                newName: "IX_SnippetOccurences_SnippetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SnippetOccurences",
                schema: "antiplagiarism",
                table: "SnippetOccurences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SnippetOccurences_Snippets_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetOccurences",
                column: "SnippetId",
                principalSchema: "antiplagiarism",
                principalTable: "Snippets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SnippetOccurences_Submissions_SubmissionId",
                schema: "antiplagiarism",
                table: "SnippetOccurences",
                column: "SubmissionId",
                principalSchema: "antiplagiarism",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
