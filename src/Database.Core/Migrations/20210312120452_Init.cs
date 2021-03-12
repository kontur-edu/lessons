using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Database.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:case_insensitive", "und@colStrength=secondary,und@colStrength=secondary,icu,False");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    FirstName = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LastName = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Registered = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastEdit = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: true),
                    TelegramChatTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    KonturLogin = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LastConfirmationEmailTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Gender = table.Column<short>(type: "smallint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Names = table.Column<string>(type: "text", nullable: true, computedColumnSql: "immutable_concat_ws(' ', nullif(\"UserName\", ''), nullif(\"FirstName\",''), nullif(\"LastName\",''), nullif(\"FirstName\",''))", stored: true, collation: "default"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CertificateTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Name = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ArchiveName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommentsPolicies",
                columns: table => new
                {
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsCommentsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ModerationPolicy = table.Column<int>(type: "integer", nullable: false),
                    OnlyInstructorsCanReply = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentsPolicies", x => x.CourseId);
                });

            migrationBuilder.CreateTable(
                name: "CourseGitRepos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    RepoUrl = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Branch = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    PublicKey = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    PrivateKey = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsWebhookEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PathToCourseXml = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseGitRepos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LtiConsumers",
                columns: table => new
                {
                    ConsumerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Secret = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LtiConsumers", x => x.ConsumerId);
                });

            migrationBuilder.CreateTable(
                name: "LtiSlideRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Request = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LtiSlideRequests", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "StepikExportSlideAndStepMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UlearnCourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    StepikCourseId = table.Column<int>(type: "integer", nullable: false),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepId = table.Column<int>(type: "integer", nullable: false),
                    SlideXml = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepikExportSlideAndStepMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StyleErrorSettings",
                columns: table => new
                {
                    ErrorType = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleErrorSettings", x => x.ErrorType);
                });

            migrationBuilder.CreateTable(
                name: "TempCourseErrors",
                columns: table => new
                {
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Error = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempCourseErrors", x => x.CourseId);
                });

            migrationBuilder.CreateTable(
                name: "TextBlobs",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Text = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextBlobs", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "UnitAppearances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    PublishTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitAppearances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkQueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    TakeAfterTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkQueueItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ClaimType = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScoringGroupId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    InstructorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalScores_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdditionalScores_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ClaimType = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ProviderKey = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserId = table.Column<string>(type: "character varying(64)", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(64)", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(64)", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LoginProvider = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Name = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Value = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    PublishTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsCorrectAnswer = table.Column<bool>(type: "boolean", nullable: false),
                    IsPinnedToTop = table.Column<bool>(type: "boolean", nullable: false),
                    IsForInstructorsOnly = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    GrantedById = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AccessType = table.Column<short>(type: "smallint", nullable: false),
                    GrantTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseAccesses_AspNetUsers_GrantedById",
                        column: x => x.GrantedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LoadingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PublishTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    RepoUrl = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CommitHash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    PathToCourseXml = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseVersions_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GraderClients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GraderClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GraderClients_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupLabels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ColorHex = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupLabels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupLabels_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    OwnerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    InviteHash = table.Column<Guid>(type: "uuid", nullable: false),
                    IsInviteLinkEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsManualCheckingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsManualCheckingEnabledForOldSolutions = table.Column<bool>(type: "boolean", nullable: false),
                    CanUsersSeeGroupProgress = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultProhibitFutherReview = table.Column<bool>(type: "boolean", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LastVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LastVisits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTransports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTransports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTransports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestoreRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestoreRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestoreRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlideHints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    HintId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsHintHelped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlideHints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlideHints_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlideRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Rate = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlideRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlideRates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StepikAccessTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AccessToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AddedTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepikAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepikAccessTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StepikExportProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UlearnCourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    StepikCourseId = table.Column<int>(type: "integer", nullable: false),
                    StepikCourseTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    IsInitialExport = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    Log = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    OwnerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FinishTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StepikExportProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StepikExportProcesses_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    GrantedById = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AccessType = table.Column<short>(type: "smallint", nullable: false),
                    GrantTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemAccesses_AspNetUsers_GrantedById",
                        column: x => x.GrantedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TempCourses",
                columns: table => new
                {
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LoadingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempCourses", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_TempCourses_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFlashcardsUnlocking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlashcardsUnlocking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFlashcardsUnlocking_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFlashcardsVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlashcardId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Rate = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlashcardsVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFlashcardsVisits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SlideTitle = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Question = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UnitName = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_UserQuestions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizSubmissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    GrantedById = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    GrantTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    HasManualChecking = table.Column<bool>(type: "boolean", nullable: false),
                    AttemptsCount = table.Column<int>(type: "integer", nullable: false),
                    IsSkipped = table.Column<bool>(type: "boolean", nullable: false),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XQueueWatchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    BaseUrl = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    QueueName = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserName = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Password = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XQueueWatchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XQueueWatchers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    InstructorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Parameters = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPreview = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_CertificateTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CertificateTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CertificateTemplateArchives",
                columns: table => new
                {
                    ArchiveName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    CertificateTemplateId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplateArchives", x => x.ArchiveName);
                    table.ForeignKey(
                        name: "FK_CertificateTemplateArchives_CertificateTemplates_Certificat~",
                        column: x => x.CertificateTemplateId,
                        principalTable: "CertificateTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomaticExerciseCheckings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Elapsed = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsRightAnswer = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompilationError = table.Column<bool>(type: "boolean", nullable: false),
                    CompilationErrorHash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    OutputHash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    DebugLogsHash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ExecutionServiceName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CheckingAgentName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Points = table.Column<float>(type: "real", nullable: true),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticExerciseCheckings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomaticExerciseCheckings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomaticExerciseCheckings_TextBlobs_CompilationErrorHash",
                        column: x => x.CompilationErrorHash,
                        principalTable: "TextBlobs",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomaticExerciseCheckings_TextBlobs_DebugLogsHash",
                        column: x => x.DebugLogsHash,
                        principalTable: "TextBlobs",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomaticExerciseCheckings_TextBlobs_OutputHash",
                        column: x => x.OutputHash,
                        principalTable: "TextBlobs",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommentLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CommentId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentLikes_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    File = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseFiles_CourseVersions_CourseVersionId",
                        column: x => x.CourseVersionId,
                        principalTable: "CourseVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnabledAdditionalScoringGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    ScoringGroupId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnabledAdditionalScoringGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnabledAdditionalScoringGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    GrantedById = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AccessType = table.Column<short>(type: "smallint", nullable: false),
                    GrantTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupAccesses_AspNetUsers_GrantedById",
                        column: x => x.GrantedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupAccesses_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AddingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabelOnGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    LabelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelOnGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabelOnGroups_GroupLabels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "GroupLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabelOnGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedViewTimestamps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    TransportId = table.Column<int>(type: "integer", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedViewTimestamps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedViewTimestamps_NotificationTransports_TransportId",
                        column: x => x.TransportId,
                        principalTable: "NotificationTransports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTransportSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationTransportId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    NotificationType = table.Column<short>(type: "smallint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTransportSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTransportSettings_NotificationTransports_Notifi~",
                        column: x => x.NotificationTransportId,
                        principalTable: "NotificationTransports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AutomaticQuizCheckings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    IgnoreInAttemptsCount = table.Column<bool>(type: "boolean", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomaticQuizCheckings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomaticQuizCheckings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomaticQuizCheckings_UserQuizSubmissions_Id",
                        column: x => x.Id,
                        principalTable: "UserQuizSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManualQuizCheckings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    IgnoreInAttemptsCount = table.Column<bool>(type: "boolean", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LockedUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LockedById = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsChecked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualQuizCheckings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManualQuizCheckings_AspNetUsers_LockedById",
                        column: x => x.LockedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManualQuizCheckings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManualQuizCheckings_UserQuizSubmissions_Id",
                        column: x => x.Id,
                        principalTable: "UserQuizSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    BlockId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ItemId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Text = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsRightAnswer = table.Column<bool>(type: "boolean", nullable: false),
                    QuizBlockScore = table.Column<int>(type: "integer", nullable: false),
                    QuizBlockMaxScore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizAnswers_UserQuizSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "UserQuizSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserExerciseSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SolutionCodeHash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CodeHash = table.Column<int>(type: "integer", nullable: false),
                    AutomaticCheckingId = table.Column<int>(type: "integer", nullable: true),
                    AutomaticCheckingIsRightAnswer = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<short>(type: "smallint", nullable: false),
                    Sandbox = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AntiPlagiarismSubmissionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExerciseSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserExerciseSubmissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserExerciseSubmissions_AutomaticExerciseCheckings_Automati~",
                        column: x => x.AutomaticCheckingId,
                        principalTable: "AutomaticExerciseCheckings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserExerciseSubmissions_TextBlobs_SolutionCodeHash",
                        column: x => x.SolutionCodeHash,
                        principalTable: "TextBlobs",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSolutionByGraders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    ClientUserId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSolutionByGraders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseSolutionByGraders_GraderClients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "GraderClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseSolutionByGraders_UserExerciseSubmissions_Submissio~",
                        column: x => x.SubmissionId,
                        principalTable: "UserExerciseSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_UserExerciseSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "UserExerciseSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ManualExerciseCheckings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    ProhibitFurtherManualCheckings = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Percent = table.Column<int>(type: "integer", nullable: true),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LockedUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LockedById = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsChecked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualExerciseCheckings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManualExerciseCheckings_AspNetUsers_LockedById",
                        column: x => x.LockedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManualExerciseCheckings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ManualExerciseCheckings_UserExerciseSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "UserExerciseSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XQueueExerciseSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubmissionId = table.Column<int>(type: "integer", nullable: false),
                    WatcherId = table.Column<int>(type: "integer", nullable: false),
                    XQueueHeader = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsResultSent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XQueueExerciseSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XQueueExerciseSubmissions_UserExerciseSubmissions_Submissio~",
                        column: x => x.SubmissionId,
                        principalTable: "UserExerciseSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_XQueueExerciseSubmissions_XQueueWatchers_WatcherId",
                        column: x => x.WatcherId,
                        principalTable: "XQueueWatchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseCodeReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExerciseCheckingId = table.Column<int>(type: "integer", nullable: true),
                    SubmissionId = table.Column<int>(type: "integer", nullable: true),
                    StartLine = table.Column<int>(type: "integer", nullable: false),
                    StartPosition = table.Column<int>(type: "integer", nullable: false),
                    FinishLine = table.Column<int>(type: "integer", nullable: false),
                    FinishPosition = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    HiddenFromTopComments = table.Column<bool>(type: "boolean", nullable: false),
                    AddingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseCodeReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseCodeReviews_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~",
                        column: x => x.ExerciseCheckingId,
                        principalTable: "ManualExerciseCheckings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseCodeReviews_UserExerciseSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "UserExerciseSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseCodeReviewComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    AddingTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseCodeReviewComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseCodeReviewComments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseCodeReviewComments_ExerciseCodeReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "ExerciseCodeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    InitiatedById = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AreDeliveriesCreated = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CommentId1 = table.Column<int>(type: "integer", nullable: true),
                    LikedUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    AddedUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ProcessId = table.Column<int>(type: "integer", nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    AccessId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "character varying(64)", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    GroupMemberHasBeenRemovedNotification_GroupId = table.Column<int>(type: "integer", nullable: true),
                    UserIds = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UserDescriptions = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Text = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    JoinedToYourGroupNotification_GroupId = table.Column<int>(type: "integer", nullable: true),
                    JoinedUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    NotUploadedPackageNotification_CommitHash = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    NotUploadedPackageNotification_RepoUrl = table.Column<string>(type: "text", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CheckingId = table.Column<int>(type: "integer", nullable: true),
                    IsRecheck = table.Column<bool>(type: "boolean", nullable: true),
                    PassedManualQuizCheckingNotification_CheckingId = table.Column<int>(type: "integer", nullable: true),
                    CourseVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScoreId = table.Column<int>(type: "integer", nullable: true),
                    CertificateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommentId = table.Column<int>(type: "integer", nullable: true),
                    RevokedAccessToGroupNotification_AccessId = table.Column<int>(type: "integer", nullable: true),
                    UploadedPackageNotification_CourseVersionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AdditionalScores_ScoreId",
                        column: x => x.ScoreId,
                        principalTable: "AdditionalScores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_AddedUserId",
                        column: x => x.AddedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_InitiatedById",
                        column: x => x.InitiatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_JoinedUserId",
                        column: x => x.JoinedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_LikedUserId",
                        column: x => x.LikedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Certificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "Certificates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Comments_CommentId1",
                        column: x => x.CommentId1,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_CourseVersions_CourseVersionId",
                        column: x => x.CourseVersionId,
                        principalTable: "CourseVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_CourseVersions_UploadedPackageNotification_Co~",
                        column: x => x.UploadedPackageNotification_CourseVersionId,
                        principalTable: "CourseVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_ExerciseCodeReviewComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "ExerciseCodeReviewComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_GroupAccesses_AccessId",
                        column: x => x.AccessId,
                        principalTable: "GroupAccesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~",
                        column: x => x.RevokedAccessToGroupNotification_AccessId,
                        principalTable: "GroupAccesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Groups_GroupId1",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Groups_GroupId2",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Groups_GroupId3",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Groups_GroupMemberHasBeenRemovedNotification_~",
                        column: x => x.GroupMemberHasBeenRemovedNotification_GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId",
                        column: x => x.JoinedToYourGroupNotification_GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_ManualExerciseCheckings_CheckingId",
                        column: x => x.CheckingId,
                        principalTable: "ManualExerciseCheckings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~",
                        column: x => x.PassedManualQuizCheckingNotification_CheckingId,
                        principalTable: "ManualQuizCheckings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_StepikExportProcesses_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "StepikExportProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationId = table.Column<int>(type: "integer", nullable: false),
                    NotificationTransportId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NextTryTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FailsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationDeliveries_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationDeliveries_NotificationTransports_NotificationT~",
                        column: x => x.NotificationTransportId,
                        principalTable: "NotificationTransports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalScores_CourseId_UserId",
                table: "AdditionalScores",
                columns: new[] { "CourseId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalScores_CourseId_UserId_UnitId_ScoringGroupId",
                table: "AdditionalScores",
                columns: new[] { "CourseId", "UserId", "UnitId", "ScoringGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalScores_InstructorId",
                table: "AdditionalScores",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalScores_UnitId",
                table: "AdditionalScores",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalScores_UserId",
                table: "AdditionalScores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted",
                table: "AspNetUsers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Names",
                table: "AspNetUsers",
                column: "Names")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TelegramChatId",
                table: "AspNetUsers",
                column: "TelegramChatId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_CompilationErrorHash",
                table: "AutomaticExerciseCheckings",
                column: "CompilationErrorHash");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_CourseId_SlideId",
                table: "AutomaticExerciseCheckings",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_CourseId_SlideId_Timestamp",
                table: "AutomaticExerciseCheckings",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_CourseId_SlideId_UserId",
                table: "AutomaticExerciseCheckings",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_CourseId_UserId",
                table: "AutomaticExerciseCheckings",
                columns: new[] { "CourseId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_DebugLogsHash",
                table: "AutomaticExerciseCheckings",
                column: "DebugLogsHash");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_IsRightAnswer",
                table: "AutomaticExerciseCheckings",
                column: "IsRightAnswer");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_OutputHash",
                table: "AutomaticExerciseCheckings",
                column: "OutputHash");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticExerciseCheckings_UserId",
                table: "AutomaticExerciseCheckings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticQuizCheckings_CourseId_SlideId",
                table: "AutomaticQuizCheckings",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticQuizCheckings_CourseId_SlideId_Timestamp",
                table: "AutomaticQuizCheckings",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticQuizCheckings_CourseId_SlideId_UserId",
                table: "AutomaticQuizCheckings",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticQuizCheckings_CourseId_UserId",
                table: "AutomaticQuizCheckings",
                columns: new[] { "CourseId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AutomaticQuizCheckings_UserId",
                table: "AutomaticQuizCheckings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_InstructorId",
                table: "Certificates",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_TemplateId",
                table: "Certificates",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_UserId",
                table: "Certificates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplateArchives_CertificateTemplateId",
                table: "CertificateTemplateArchives",
                column: "CertificateTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTemplates_CourseId",
                table: "CertificateTemplates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_CommentId",
                table: "CommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_UserId_CommentId",
                table: "CommentLikes",
                columns: new[] { "UserId", "CommentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId_PublishTime",
                table: "Comments",
                columns: new[] { "AuthorId", "PublishTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_SlideId",
                table: "Comments",
                column: "SlideId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccesses_CourseId",
                table: "CourseAccesses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccesses_CourseId_IsEnabled",
                table: "CourseAccesses",
                columns: new[] { "CourseId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccesses_CourseId_UserId_IsEnabled",
                table: "CourseAccesses",
                columns: new[] { "CourseId", "UserId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccesses_GrantedById",
                table: "CourseAccesses",
                column: "GrantedById");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccesses_GrantTime",
                table: "CourseAccesses",
                column: "GrantTime");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAccesses_UserId",
                table: "CourseAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseFiles_CourseVersionId",
                table: "CourseFiles",
                column: "CourseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVersions_AuthorId",
                table: "CourseVersions",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVersions_CourseId_LoadingTime",
                table: "CourseVersions",
                columns: new[] { "CourseId", "LoadingTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseVersions_CourseId_PublishTime",
                table: "CourseVersions",
                columns: new[] { "CourseId", "PublishTime" });

            migrationBuilder.CreateIndex(
                name: "IX_EnabledAdditionalScoringGroups_GroupId",
                table: "EnabledAdditionalScoringGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviewComments_AddingTime",
                table: "ExerciseCodeReviewComments",
                column: "AddingTime");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviewComments_AuthorId",
                table: "ExerciseCodeReviewComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviewComments_ReviewId_IsDeleted",
                table: "ExerciseCodeReviewComments",
                columns: new[] { "ReviewId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviews_AuthorId",
                table: "ExerciseCodeReviews",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviews_ExerciseCheckingId",
                table: "ExerciseCodeReviews",
                column: "ExerciseCheckingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviews_SubmissionId",
                table: "ExerciseCodeReviews",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSolutionByGraders_ClientId",
                table: "ExerciseSolutionByGraders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSolutionByGraders_SubmissionId",
                table: "ExerciseSolutionByGraders",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedViewTimestamps_Timestamp",
                table: "FeedViewTimestamps",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_FeedViewTimestamps_TransportId",
                table: "FeedViewTimestamps",
                column: "TransportId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedViewTimestamps_UserId",
                table: "FeedViewTimestamps",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedViewTimestamps_UserId_TransportId",
                table: "FeedViewTimestamps",
                columns: new[] { "UserId", "TransportId" });

            migrationBuilder.CreateIndex(
                name: "IX_GraderClients_UserId",
                table: "GraderClients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccesses_GrantedById",
                table: "GroupAccesses",
                column: "GrantedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccesses_GrantTime",
                table: "GroupAccesses",
                column: "GrantTime");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccesses_GroupId",
                table: "GroupAccesses",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccesses_GroupId_IsEnabled",
                table: "GroupAccesses",
                columns: new[] { "GroupId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccesses_GroupId_UserId_IsEnabled",
                table: "GroupAccesses",
                columns: new[] { "GroupId", "UserId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupAccesses_UserId",
                table: "GroupAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLabels_OwnerId",
                table: "GroupLabels",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLabels_OwnerId_IsDeleted",
                table: "GroupLabels",
                columns: new[] { "OwnerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId",
                table: "GroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CourseId",
                table: "Groups",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_InviteHash",
                table: "Groups",
                column: "InviteHash");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelOnGroups_GroupId",
                table: "LabelOnGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_LabelOnGroups_GroupId_LabelId",
                table: "LabelOnGroups",
                columns: new[] { "GroupId", "LabelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabelOnGroups_LabelId",
                table: "LabelOnGroups",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_LastVisits_CourseId_UserId",
                table: "LastVisits",
                columns: new[] { "CourseId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LastVisits_UserId",
                table: "LastVisits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_SubmissionId",
                table: "Likes",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId_SubmissionId",
                table: "Likes",
                columns: new[] { "UserId", "SubmissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_LtiConsumers_Key",
                table: "LtiConsumers",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_LtiSlideRequests_CourseId_SlideId_UserId",
                table: "LtiSlideRequests",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_CourseId_SlideId",
                table: "ManualExerciseCheckings",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_CourseId_SlideId_Timestamp",
                table: "ManualExerciseCheckings",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_CourseId_SlideId_UserId_ProhibitFur~",
                table: "ManualExerciseCheckings",
                columns: new[] { "CourseId", "SlideId", "UserId", "ProhibitFurtherManualCheckings" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_CourseId_UserId",
                table: "ManualExerciseCheckings",
                columns: new[] { "CourseId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_LockedById",
                table: "ManualExerciseCheckings",
                column: "LockedById");

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_SubmissionId",
                table: "ManualExerciseCheckings",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_UserId",
                table: "ManualExerciseCheckings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualQuizCheckings_CourseId_SlideId",
                table: "ManualQuizCheckings",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualQuizCheckings_CourseId_SlideId_Timestamp",
                table: "ManualQuizCheckings",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualQuizCheckings_CourseId_SlideId_UserId",
                table: "ManualQuizCheckings",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualQuizCheckings_CourseId_UserId",
                table: "ManualQuizCheckings",
                columns: new[] { "CourseId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualQuizCheckings_LockedById",
                table: "ManualQuizCheckings",
                column: "LockedById");

            migrationBuilder.CreateIndex(
                name: "IX_ManualQuizCheckings_UserId",
                table: "ManualQuizCheckings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_CreateTime",
                table: "NotificationDeliveries",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_NextTryTime",
                table: "NotificationDeliveries",
                column: "NextTryTime");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_NotificationId_NotificationTransport~",
                table: "NotificationDeliveries",
                columns: new[] { "NotificationId", "NotificationTransportId" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_NotificationTransportId",
                table: "NotificationDeliveries",
                column: "NotificationTransportId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AccessId",
                table: "Notifications",
                column: "AccessId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AddedUserId",
                table: "Notifications",
                column: "AddedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AreDeliveriesCreated",
                table: "Notifications",
                column: "AreDeliveriesCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CertificateId",
                table: "Notifications",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CheckingId",
                table: "Notifications",
                column: "CheckingId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CommentId",
                table: "Notifications",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CommentId1",
                table: "Notifications",
                column: "CommentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CourseId",
                table: "Notifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CourseVersionId",
                table: "Notifications",
                column: "CourseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreateTime",
                table: "Notifications",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId1",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId2",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId3",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupMemberHasBeenRemovedNotification_GroupId",
                table: "Notifications",
                column: "GroupMemberHasBeenRemovedNotification_GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_InitiatedById",
                table: "Notifications",
                column: "InitiatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_JoinedToYourGroupNotification_GroupId",
                table: "Notifications",
                column: "JoinedToYourGroupNotification_GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_JoinedUserId",
                table: "Notifications",
                column: "JoinedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_LikedUserId",
                table: "Notifications",
                column: "LikedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ParentCommentId",
                table: "Notifications",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PassedManualQuizCheckingNotification_Checking~",
                table: "Notifications",
                column: "PassedManualQuizCheckingNotification_CheckingId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ProcessId",
                table: "Notifications",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RevokedAccessToGroupNotification_AccessId",
                table: "Notifications",
                column: "RevokedAccessToGroupNotification_AccessId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ScoreId",
                table: "Notifications",
                column: "ScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UploadedPackageNotification_CourseVersionId",
                table: "Notifications",
                column: "UploadedPackageNotification_CourseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTransports_UserId",
                table: "NotificationTransports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTransports_UserId_IsDeleted",
                table: "NotificationTransports",
                columns: new[] { "UserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTransportSettings_CourseId",
                table: "NotificationTransportSettings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTransportSettings_CourseId_NotificationType",
                table: "NotificationTransportSettings",
                columns: new[] { "CourseId", "NotificationType" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTransportSettings_NotificationTransportId",
                table: "NotificationTransportSettings",
                column: "NotificationTransportId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTransportSettings_NotificationType",
                table: "NotificationTransportSettings",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_RestoreRequests_UserId",
                table: "RestoreRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SlideHints_CourseId_SlideId_HintId_UserId_IsHintHelped",
                table: "SlideHints",
                columns: new[] { "CourseId", "SlideId", "HintId", "UserId", "IsHintHelped" });

            migrationBuilder.CreateIndex(
                name: "IX_SlideHints_UserId",
                table: "SlideHints",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SlideRates_SlideId_UserId",
                table: "SlideRates",
                columns: new[] { "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SlideRates_UserId",
                table: "SlideRates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StepikAccessTokens_AddedTime",
                table: "StepikAccessTokens",
                column: "AddedTime");

            migrationBuilder.CreateIndex(
                name: "IX_StepikAccessTokens_UserId",
                table: "StepikAccessTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StepikExportProcesses_OwnerId",
                table: "StepikExportProcesses",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StepikExportSlideAndStepMaps_UlearnCourseId",
                table: "StepikExportSlideAndStepMaps",
                column: "UlearnCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StepikExportSlideAndStepMaps_UlearnCourseId_SlideId",
                table: "StepikExportSlideAndStepMaps",
                columns: new[] { "UlearnCourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_StepikExportSlideAndStepMaps_UlearnCourseId_StepikCourseId",
                table: "StepikExportSlideAndStepMaps",
                columns: new[] { "UlearnCourseId", "StepikCourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccesses_GrantedById",
                table: "SystemAccesses",
                column: "GrantedById");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccesses_GrantTime",
                table: "SystemAccesses",
                column: "GrantTime");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccesses_IsEnabled",
                table: "SystemAccesses",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccesses_UserId",
                table: "SystemAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccesses_UserId_IsEnabled",
                table: "SystemAccesses",
                columns: new[] { "UserId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_TempCourses_AuthorId",
                table: "TempCourses",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitAppearances_CourseId_PublishTime",
                table: "UnitAppearances",
                columns: new[] { "CourseId", "PublishTime" });

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_AntiPlagiarismSubmissionId",
                table: "UserExerciseSubmissions",
                column: "AntiPlagiarismSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_AutomaticCheckingId",
                table: "UserExerciseSubmissions",
                column: "AutomaticCheckingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_AutomaticCheckingIsRightAnswer",
                table: "UserExerciseSubmissions",
                column: "AutomaticCheckingIsRightAnswer");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_CourseId_AutomaticCheckingIsRightAn~",
                table: "UserExerciseSubmissions",
                columns: new[] { "CourseId", "AutomaticCheckingIsRightAnswer" });

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_CourseId_SlideId",
                table: "UserExerciseSubmissions",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_CourseId_SlideId_AutomaticCheckingI~",
                table: "UserExerciseSubmissions",
                columns: new[] { "CourseId", "SlideId", "AutomaticCheckingIsRightAnswer" });

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_CourseId_SlideId_Timestamp",
                table: "UserExerciseSubmissions",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_CourseId_SlideId_UserId",
                table: "UserExerciseSubmissions",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_Language",
                table: "UserExerciseSubmissions",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_Sandbox",
                table: "UserExerciseSubmissions",
                column: "Sandbox");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_SolutionCodeHash",
                table: "UserExerciseSubmissions",
                column: "SolutionCodeHash");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_Timestamp",
                table: "UserExerciseSubmissions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseSubmissions_UserId",
                table: "UserExerciseSubmissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardsUnlocking_UserId_CourseId_UnitId",
                table: "UserFlashcardsUnlocking",
                columns: new[] { "UserId", "CourseId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardsVisits_UserId_CourseId_UnitId_FlashcardId",
                table: "UserFlashcardsVisits",
                columns: new[] { "UserId", "CourseId", "UnitId", "FlashcardId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestions_UserId",
                table: "UserQuestions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAnswers_ItemId",
                table: "UserQuizAnswers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAnswers_SubmissionId_BlockId",
                table: "UserQuizAnswers",
                columns: new[] { "SubmissionId", "BlockId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_CourseId_SlideId",
                table: "UserQuizSubmissions",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_CourseId_SlideId_Timestamp",
                table: "UserQuizSubmissions",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_CourseId_SlideId_UserId",
                table: "UserQuizSubmissions",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizSubmissions_UserId",
                table: "UserQuizSubmissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CourseId_SlideId_UserId",
                table: "Visits",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SlideId_Timestamp",
                table: "Visits",
                columns: new[] { "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SlideId_UserId",
                table: "Visits",
                columns: new[] { "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId",
                table: "Visits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_XQueueExerciseSubmissions_SubmissionId",
                table: "XQueueExerciseSubmissions",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_XQueueExerciseSubmissions_WatcherId",
                table: "XQueueExerciseSubmissions",
                column: "WatcherId");

            migrationBuilder.CreateIndex(
                name: "IX_XQueueWatchers_UserId",
                table: "XQueueWatchers",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AutomaticQuizCheckings");

            migrationBuilder.DropTable(
                name: "CertificateTemplateArchives");

            migrationBuilder.DropTable(
                name: "CommentLikes");

            migrationBuilder.DropTable(
                name: "CommentsPolicies");

            migrationBuilder.DropTable(
                name: "CourseAccesses");

            migrationBuilder.DropTable(
                name: "CourseFiles");

            migrationBuilder.DropTable(
                name: "CourseGitRepos");

            migrationBuilder.DropTable(
                name: "EnabledAdditionalScoringGroups");

            migrationBuilder.DropTable(
                name: "ExerciseSolutionByGraders");

            migrationBuilder.DropTable(
                name: "FeedViewTimestamps");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "LabelOnGroups");

            migrationBuilder.DropTable(
                name: "LastVisits");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "LtiConsumers");

            migrationBuilder.DropTable(
                name: "LtiSlideRequests");

            migrationBuilder.DropTable(
                name: "NotificationDeliveries");

            migrationBuilder.DropTable(
                name: "NotificationTransportSettings");

            migrationBuilder.DropTable(
                name: "RestoreRequests");

            migrationBuilder.DropTable(
                name: "SlideHints");

            migrationBuilder.DropTable(
                name: "SlideRates");

            migrationBuilder.DropTable(
                name: "StepikAccessTokens");

            migrationBuilder.DropTable(
                name: "StepikExportSlideAndStepMaps");

            migrationBuilder.DropTable(
                name: "StyleErrorSettings");

            migrationBuilder.DropTable(
                name: "SystemAccesses");

            migrationBuilder.DropTable(
                name: "TempCourseErrors");

            migrationBuilder.DropTable(
                name: "TempCourses");

            migrationBuilder.DropTable(
                name: "UnitAppearances");

            migrationBuilder.DropTable(
                name: "UserFlashcardsUnlocking");

            migrationBuilder.DropTable(
                name: "UserFlashcardsVisits");

            migrationBuilder.DropTable(
                name: "UserQuestions");

            migrationBuilder.DropTable(
                name: "UserQuizAnswers");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "WorkQueueItems");

            migrationBuilder.DropTable(
                name: "XQueueExerciseSubmissions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GraderClients");

            migrationBuilder.DropTable(
                name: "GroupLabels");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationTransports");

            migrationBuilder.DropTable(
                name: "XQueueWatchers");

            migrationBuilder.DropTable(
                name: "AdditionalScores");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CourseVersions");

            migrationBuilder.DropTable(
                name: "ExerciseCodeReviewComments");

            migrationBuilder.DropTable(
                name: "GroupAccesses");

            migrationBuilder.DropTable(
                name: "ManualQuizCheckings");

            migrationBuilder.DropTable(
                name: "StepikExportProcesses");

            migrationBuilder.DropTable(
                name: "CertificateTemplates");

            migrationBuilder.DropTable(
                name: "ExerciseCodeReviews");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "UserQuizSubmissions");

            migrationBuilder.DropTable(
                name: "ManualExerciseCheckings");

            migrationBuilder.DropTable(
                name: "UserExerciseSubmissions");

            migrationBuilder.DropTable(
                name: "AutomaticExerciseCheckings");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TextBlobs");
        }
    }
}
