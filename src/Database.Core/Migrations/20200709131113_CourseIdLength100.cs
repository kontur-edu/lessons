using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class CourseIdLength100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Visits",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserQuizSubmissions",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserQuestions",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserFlashcardsVisits",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserFlashcardsUnlocking",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UnitAppearances",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "TempCourses",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "TempCourseErrors",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "SlideRates",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "SlideHints",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "ManualQuizCheckings",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "ManualExerciseCheckings",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Groups",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "GraderClients",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseVersions",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseGitRepos",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseFiles",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseAccesses",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CommentsPolicies",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Comments",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CertificateTemplates",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "AutomaticQuizCheckings",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "AutomaticExerciseCheckings",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "AdditionalScores",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Visits",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserQuizSubmissions",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserQuestions",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserFlashcardsVisits",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UserFlashcardsUnlocking",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "UnitAppearances",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "TempCourses",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "TempCourseErrors",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "SlideRates",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "SlideHints",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "ManualQuizCheckings",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "ManualExerciseCheckings",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Groups",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "GraderClients",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseVersions",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseGitRepos",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseFiles",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CourseAccesses",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CommentsPolicies",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Comments",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "CertificateTemplates",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "AutomaticQuizCheckings",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "AutomaticExerciseCheckings",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "AdditionalScores",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100);
        }
    }
}
