using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class InitPostgres : Migration
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManualSuspicionLevels",
                schema: "antiplagiarism",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    FaintSuspicion = table.Column<double>(type: "double precision", nullable: true),
                    StrongSuspicion = table.Column<double>(type: "double precision", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualSuspicionLevels", x => x.TaskId);
                });

            migrationBuilder.CreateTable(
                name: "OldSubmissionsInfluenceBorder",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldSubmissionsInfluenceBorder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Snippets",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TokensCount = table.Column<int>(type: "integer", nullable: false),
                    SnippetType = table.Column<short>(type: "smallint", nullable: false),
                    Hash = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snippets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TasksStatisticsParameters",
                schema: "antiplagiarism",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mean = table.Column<double>(type: "double precision", nullable: false),
                    Deviation = table.Column<double>(type: "double precision", nullable: false),
                    SubmissionsCount = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TasksStatisticsParameters", x => x.TaskId);
                });

            migrationBuilder.CreateTable(
                name: "WorkQueueItems",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    TakeAfterTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkQueueItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<int>(type: "integer", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AddingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Language = table.Column<short>(type: "smallint", nullable: false),
                    TokensCount = table.Column<int>(type: "integer", nullable: false),
                    ClientSubmissionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "SnippetsStatistics",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SnippetId = table.Column<int>(type: "integer", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    AuthorsCount = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "MostSimilarSubmissions",
                schema: "antiplagiarism",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    SimilarSubmissionId = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MostSimilarSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_MostSimilarSubmissions_Submissions_SimilarSubmissionId",
                        column: x => x.SimilarSubmissionId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MostSimilarSubmissions_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SnippetsOccurences",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    SnippetId = table.Column<int>(type: "integer", nullable: false),
                    FirstTokenIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnippetsOccurences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnippetsOccurences_Snippets_SnippetId",
                        column: x => x.SnippetId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Snippets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SnippetsOccurences_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "antiplagiarism",
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskStatisticsSourceData",
                schema: "antiplagiarism",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Submission1Id = table.Column<int>(type: "integer", nullable: false),
                    Submission2Id = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStatisticsSourceData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskStatisticsSourceData_Submissions_Submission1Id",
                        column: x => x.Submission1Id,
                        principalSchema: "antiplagiarism",
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskStatisticsSourceData_Submissions_Submission2Id",
                        column: x => x.Submission2Id,
                        principalSchema: "antiplagiarism",
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Token",
                schema: "antiplagiarism",
                table: "Clients",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Token_IsEnabled",
                schema: "antiplagiarism",
                table: "Clients",
                columns: new[] { "Token", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_MostSimilarSubmissions_SimilarSubmissionId",
                schema: "antiplagiarism",
                table: "MostSimilarSubmissions",
                column: "SimilarSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MostSimilarSubmissions_Timestamp",
                schema: "antiplagiarism",
                table: "MostSimilarSubmissions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Snippets_TokensCount_SnippetType_Hash",
                schema: "antiplagiarism",
                table: "Snippets",
                columns: new[] { "TokensCount", "SnippetType", "Hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsOccurences_SnippetId_SubmissionId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                columns: new[] { "SnippetId", "SubmissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsOccurences_SubmissionId_FirstTokenIndex",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                columns: new[] { "SubmissionId", "FirstTokenIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsOccurences_SubmissionId_SnippetId",
                schema: "antiplagiarism",
                table: "SnippetsOccurences",
                columns: new[] { "SubmissionId", "SnippetId" });

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

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AddingTime",
                schema: "antiplagiarism",
                table: "Submissions",
                column: "AddingTime");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_ClientSubmissionId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "ClientSubmissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_AddingTime_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "AddingTime", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_AddingTime_Language_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "AddingTime", "Language", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_Language_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "Language", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ProgramId",
                schema: "antiplagiarism",
                table: "Submissions",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatisticsSourceData_Submission1Id",
                schema: "antiplagiarism",
                table: "TaskStatisticsSourceData",
                column: "Submission1Id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatisticsSourceData_Submission2Id",
                schema: "antiplagiarism",
                table: "TaskStatisticsSourceData",
                column: "Submission2Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkQueueItems_QueueId_TakeAfterTime",
                schema: "antiplagiarism",
                table: "WorkQueueItems",
                columns: new[] { "QueueId", "TakeAfterTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManualSuspicionLevels",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "MostSimilarSubmissions",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "OldSubmissionsInfluenceBorder",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "SnippetsOccurences",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "SnippetsStatistics",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "TasksStatisticsParameters",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "TaskStatisticsSourceData",
                schema: "antiplagiarism");

            migrationBuilder.DropTable(
                name: "WorkQueueItems",
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
