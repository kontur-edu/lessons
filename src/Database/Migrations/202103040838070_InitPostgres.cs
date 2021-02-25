namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitPostgres : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.AdditionalScores",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        UnitId = c.Guid(nullable: false),
                        ScoringGroupId = c.String(nullable: false, maxLength: 64),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Score = c.Int(nullable: false),
                        InstructorId = c.String(nullable: false, maxLength: 128),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.InstructorId)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => new { t.CourseId, t.UnitId, t.ScoringGroupId, t.UserId }, unique: true, name: "Course_Unit_ScoringGroup_User")
                .Index(t => new { t.CourseId, t.UserId }, name: "Course_User")
                .Index(t => t.UnitId, name: "Unit")
                .Index(t => new { t.UnitId, t.UserId }, name: "Unit_User")
                .Index(t => t.InstructorId);
            
            CreateTable(
                "public.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Registered = c.DateTime(nullable: false),
                        LastEdit = c.DateTime(),
                        AvatarUrl = c.String(),
                        TelegramChatId = c.Long(),
                        TelegramChatTitle = c.String(maxLength: 200),
                        KonturLogin = c.String(maxLength: 200),
                        LastConfirmationEmailTime = c.DateTime(),
                        Gender = c.Short(),
                        IsDeleted = c.Boolean(nullable: false),
                        Names = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.TelegramChatId, name: "TelegramChat")
                .Index(t => t.IsDeleted, name: "IsDeleted")
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "public.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "public.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "public.UserQuestions",
                c => new
                    {
                        QuestionId = c.Int(nullable: false, identity: true),
                        SlideTitle = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        UserName = c.String(nullable: false, maxLength: 64),
                        Question = c.String(nullable: false),
                        UnitName = c.String(nullable: false),
                        SlideId = c.Guid(nullable: false),
                        CourseId = c.String(maxLength: 100),
                        Time = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.QuestionId)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "public.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("public.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "public.AutomaticExerciseCheckings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.Int(nullable: false),
                        Elapsed = c.Time(precision: 6),
                        DisplayName = c.String(),
                        IsRightAnswer = c.Boolean(nullable: false),
                        IsCompilationError = c.Boolean(nullable: false),
                        CompilationErrorHash = c.String(maxLength: 40),
                        OutputHash = c.String(maxLength: 40),
                        ExecutionServiceName = c.String(maxLength: 40),
                        CheckingAgentName = c.String(maxLength: 256),
                        Score = c.Int(),
                        Points = c.Single(),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.TextBlobs", t => t.CompilationErrorHash)
                .ForeignKey("public.TextBlobs", t => t.OutputHash)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.IsRightAnswer, name: "IsRightanswer")
                .Index(t => t.CompilationErrorHash)
                .Index(t => t.OutputHash)
                .Index(t => new { t.CourseId, t.SlideId }, name: "Course_Slide")
                .Index(t => new { t.CourseId, t.SlideId, t.Timestamp }, name: "Course_Slide_Time")
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "Course_Slide_User")
                .Index(t => new { t.CourseId, t.UserId }, name: "Course_User");
            
            CreateTable(
                "public.TextBlobs",
                c => new
                    {
                        Hash = c.String(nullable: false, maxLength: 40),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.Hash);
            
            CreateTable(
                "public.AutomaticQuizCheckings",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Score = c.Int(nullable: false),
                        IgnoreInAttemptsCount = c.Boolean(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.UserQuizSubmissions", t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => new { t.CourseId, t.SlideId }, name: "Course_Slide")
                .Index(t => new { t.CourseId, t.SlideId, t.Timestamp }, name: "Course_Slide_Time")
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "Course_Slide_User")
                .Index(t => new { t.CourseId, t.UserId }, name: "Course_User");
            
            CreateTable(
                "public.UserQuizSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "Course_Slide_User")
                .Index(t => new { t.CourseId, t.SlideId }, name: "Course_Slide")
                .Index(t => new { t.CourseId, t.SlideId, t.Timestamp }, name: "Course_Slide_Time");
            
            CreateTable(
                "public.ManualQuizCheckings",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Score = c.Int(nullable: false),
                        IgnoreInAttemptsCount = c.Boolean(nullable: false),
                        LockedUntil = c.DateTime(),
                        LockedById = c.String(maxLength: 128),
                        IsChecked = c.Boolean(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.LockedById)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("public.UserQuizSubmissions", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.LockedById)
                .Index(t => new { t.CourseId, t.SlideId }, name: "Course_Slide")
                .Index(t => new { t.CourseId, t.SlideId, t.Timestamp }, name: "Course_Slide_Time")
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "Course_Slide_User")
                .Index(t => new { t.CourseId, t.UserId }, name: "Course_User");
            
            CreateTable(
                "public.Certificates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TemplateId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        InstructorId = c.String(nullable: false, maxLength: 128),
                        Parameters = c.String(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsPreview = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.InstructorId)
                .ForeignKey("public.CertificateTemplates", t => t.TemplateId, cascadeDelete: true)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => t.TemplateId, name: "Template")
                .Index(t => t.UserId, name: "User")
                .Index(t => t.InstructorId);
            
            CreateTable(
                "public.CertificateTemplates",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        ArchiveName = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.CourseId, name: "Course");
            
            CreateTable(
                "public.CertificateTemplateArchives",
                c => new
                    {
                        ArchiveName = c.String(nullable: false, maxLength: 128),
                        CertificateTemplateId = c.Guid(nullable: false),
                        Content = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.ArchiveName)
                .ForeignKey("public.CertificateTemplates", t => t.CertificateTemplateId, cascadeDelete: true)
                .Index(t => t.CertificateTemplateId, name: "CertificateTemplate");
            
            CreateTable(
                "public.CommentLikes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CommentId = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.Comments", t => t.CommentId)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.CommentId }, unique: true, name: "User_Comment")
                .Index(t => t.CommentId, name: "Comment");
            
            CreateTable(
                "public.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                        PublishTime = c.DateTime(nullable: false),
                        Text = c.String(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsCorrectAnswer = c.Boolean(nullable: false),
                        IsPinnedToTop = c.Boolean(nullable: false),
                        IsForInstructorsOnly = c.Boolean(nullable: false),
                        ParentCommentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.SlideId, name: "Slide")
                .Index(t => new { t.AuthorId, t.PublishTime }, name: "Author_PublishTime");
            
            CreateTable(
                "public.CommentsPolicies",
                c => new
                    {
                        CourseId = c.String(nullable: false, maxLength: 100),
                        IsCommentsEnabled = c.Boolean(nullable: false),
                        ModerationPolicy = c.Int(nullable: false),
                        OnlyInstructorsCanReply = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CourseId);
            
            CreateTable(
                "public.LtiConsumers",
                c => new
                    {
                        ConsumerId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 64),
                        Key = c.String(nullable: false, maxLength: 64),
                        Secret = c.String(nullable: false, maxLength: 64),
                    })
                .PrimaryKey(t => t.ConsumerId)
                .Index(t => t.Key, name: "Key");
            
            CreateTable(
                "public.CourseAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        UserId = c.String(maxLength: 128),
                        GrantedById = c.String(maxLength: 128),
                        AccessType = c.Short(nullable: false),
                        GrantTime = c.DateTime(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.GrantedById)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => t.CourseId, name: "Course")
                .Index(t => new { t.CourseId, t.IsEnabled }, name: "Course_IsEnabled")
                .Index(t => new { t.CourseId, t.UserId, t.IsEnabled }, name: "Course_User_IsEnabled")
                .Index(t => t.UserId, name: "User")
                .Index(t => t.GrantedById)
                .Index(t => t.GrantTime, name: "GrantTime");
            
            CreateTable(
                "public.CourseFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        CourseVersionId = c.Guid(nullable: false),
                        File = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.CourseVersions", t => t.CourseVersionId, cascadeDelete: true)
                .Index(t => t.CourseVersionId, name: "CourseVersion");
            
            CreateTable(
                "public.CourseVersions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        LoadingTime = c.DateTime(nullable: false),
                        PublishTime = c.DateTime(),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                        RepoUrl = c.String(),
                        CommitHash = c.String(maxLength: 40),
                        Description = c.String(),
                        PathToCourseXml = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.AuthorId, cascadeDelete: true)
                .Index(t => new { t.CourseId, t.LoadingTime }, name: "Course_LoadingTime")
                .Index(t => new { t.CourseId, t.PublishTime }, name: "Course_PublishTime")
                .Index(t => t.AuthorId);
            
            CreateTable(
                "public.CourseGitRepos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(maxLength: 100),
                        RepoUrl = c.String(),
                        Branch = c.String(),
                        PublicKey = c.String(),
                        PrivateKey = c.String(),
                        IsWebhookEnabled = c.Boolean(nullable: false),
                        PathToCourseXml = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "public.EnabledAdditionalScoringGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        ScoringGroupId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.Groups", t => t.GroupId, cascadeDelete: true)
                .Index(t => t.GroupId, name: "Group");
            
            CreateTable(
                "public.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 300),
                        OwnerId = c.String(nullable: false, maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        IsArchived = c.Boolean(nullable: false),
                        InviteHash = c.Guid(nullable: false),
                        IsInviteLinkEnabled = c.Boolean(nullable: false),
                        IsManualCheckingEnabled = c.Boolean(nullable: false),
                        IsManualCheckingEnabledForOldSolutions = c.Boolean(nullable: false),
                        CanUsersSeeGroupProgress = c.Boolean(nullable: false),
                        DefaultProhibitFutherReview = c.Boolean(nullable: false),
                        CreateTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.OwnerId, cascadeDelete: true)
                .Index(t => t.CourseId, name: "Course")
                .Index(t => t.OwnerId, name: "Owner")
                .Index(t => t.InviteHash, name: "InviteHash");
            
            CreateTable(
                "public.GroupMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        AddingTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.Groups", t => t.GroupId)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.GroupId, name: "Group")
                .Index(t => t.UserId, name: "Member");
            
            CreateTable(
                "public.ExerciseCodeReviewComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReviewId = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        AddingTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.AuthorId)
                .ForeignKey("public.ExerciseCodeReviews", t => t.ReviewId, cascadeDelete: true)
                .Index(t => new { t.ReviewId, t.IsDeleted }, name: "Review_IsDeleted")
                .Index(t => t.AuthorId)
                .Index(t => t.AddingTime, name: "AddingTime");
            
            CreateTable(
                "public.ExerciseCodeReviews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExerciseCheckingId = c.Int(),
                        SubmissionId = c.Int(),
                        StartLine = c.Int(nullable: false),
                        StartPosition = c.Int(nullable: false),
                        FinishLine = c.Int(nullable: false),
                        FinishPosition = c.Int(nullable: false),
                        Comment = c.String(nullable: false),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        HiddenFromTopComments = c.Boolean(nullable: false),
                        AddingTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.AuthorId)
                .ForeignKey("public.ManualExerciseCheckings", t => t.ExerciseCheckingId)
                .ForeignKey("public.UserExerciseSubmissions", t => t.SubmissionId)
                .Index(t => t.ExerciseCheckingId, name: "ManualExerciseChecking")
                .Index(t => t.SubmissionId, name: "Submission")
                .Index(t => t.AuthorId);
            
            CreateTable(
                "public.ManualExerciseCheckings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        ProhibitFurtherManualCheckings = c.Boolean(nullable: false),
                        Score = c.Int(),
                        Percent = c.Int(),
                        LockedUntil = c.DateTime(),
                        LockedById = c.String(maxLength: 128),
                        IsChecked = c.Boolean(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.LockedById)
                .ForeignKey("public.UserExerciseSubmissions", t => t.SubmissionId, cascadeDelete: true)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => t.SubmissionId)
                .Index(t => new { t.CourseId, t.SlideId, t.UserId, t.ProhibitFurtherManualCheckings }, name: "Course_Slide_User")
                .Index(t => t.LockedById)
                .Index(t => new { t.CourseId, t.SlideId }, name: "Course_Slide")
                .Index(t => new { t.CourseId, t.SlideId, t.Timestamp }, name: "Course_Slide_Time")
                .Index(t => new { t.CourseId, t.UserId }, name: "Course_User");
            
            CreateTable(
                "public.UserExerciseSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        SolutionCodeHash = c.String(nullable: false, maxLength: 40),
                        CodeHash = c.Int(nullable: false),
                        AutomaticCheckingId = c.Int(),
                        AutomaticCheckingIsRightAnswer = c.Boolean(nullable: false),
                        Language = c.Short(nullable: false),
                        Sandbox = c.String(maxLength: 40),
                        AntiPlagiarismSubmissionId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AutomaticExerciseCheckings", t => t.AutomaticCheckingId)
                .ForeignKey("public.TextBlobs", t => t.SolutionCodeHash, cascadeDelete: true)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "Course_Slide_User")
                .Index(t => new { t.CourseId, t.AutomaticCheckingIsRightAnswer }, name: "Course_IsRightAnswer")
                .Index(t => new { t.CourseId, t.SlideId }, name: "Course_Slide")
                .Index(t => new { t.CourseId, t.SlideId, t.AutomaticCheckingIsRightAnswer }, name: "Course_Slide_IsRightAnswer")
                .Index(t => new { t.CourseId, t.SlideId, t.Timestamp }, name: "Course_Slide_Time")
                .Index(t => t.Timestamp, name: "Time")
                .Index(t => t.SolutionCodeHash)
                .Index(t => t.AutomaticCheckingId)
                .Index(t => t.AutomaticCheckingIsRightAnswer, name: "IsRightAnswer")
                .Index(t => t.Language, name: "Language")
                .Index(t => t.Sandbox, name: "Sandbox")
                .Index(t => t.AntiPlagiarismSubmissionId, name: "AntiPlagiarismSubmission");
            
            CreateTable(
                "public.Likes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.UserExerciseSubmissions", t => t.SubmissionId)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.SubmissionId, name: "Submission")
                .Index(t => new { t.UserId, t.SubmissionId }, name: "User_Submission");
            
            CreateTable(
                "public.ExerciseSolutionByGraders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Guid(nullable: false),
                        SubmissionId = c.Int(nullable: false),
                        ClientUserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.GraderClients", t => t.ClientId, cascadeDelete: true)
                .ForeignKey("public.UserExerciseSubmissions", t => t.SubmissionId, cascadeDelete: true)
                .Index(t => t.ClientId)
                .Index(t => t.SubmissionId);
            
            CreateTable(
                "public.GraderClients",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "public.FeedViewTimestamps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 64),
                        TransportId = c.Int(),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.NotificationTransports", t => t.TransportId)
                .Index(t => t.UserId, name: "User")
                .Index(t => new { t.UserId, t.TransportId }, name: "User_Transport")
                .Index(t => t.Timestamp, name: "Timestamp");
            
            CreateTable(
                "public.NotificationTransports",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        IsEnabled = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId, name: "User")
                .Index(t => new { t.UserId, t.IsDeleted }, name: "User_IsDeleted");
            
            CreateTable(
                "public.GroupAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        GrantedById = c.String(nullable: false, maxLength: 128),
                        AccessType = c.Short(nullable: false),
                        GrantTime = c.DateTime(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.GrantedById)
                .ForeignKey("public.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => t.GroupId, name: "Group")
                .Index(t => new { t.GroupId, t.IsEnabled }, name: "Group_IsEnabled")
                .Index(t => new { t.GroupId, t.UserId, t.IsEnabled }, name: "Group_User_IsEnabled")
                .Index(t => t.UserId, name: "User")
                .Index(t => t.GrantedById)
                .Index(t => t.GrantTime, name: "GrantTime");
            
            CreateTable(
                "public.GroupLabels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OwnerId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(maxLength: 100),
                        ColorHex = c.String(maxLength: 6),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.OwnerId)
                .Index(t => t.OwnerId, name: "Owner")
                .Index(t => new { t.OwnerId, t.IsDeleted }, name: "Owner_IsDeleted");
            
            CreateTable(
                "public.SlideHints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        HintId = c.Int(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        IsHintHelped = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.CourseId, t.SlideId, t.UserId, t.HintId, t.IsHintHelped }, name: "FullIndex");
            
            CreateTable(
                "public.LabelOnGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        LabelId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.Groups", t => t.GroupId)
                .ForeignKey("public.GroupLabels", t => t.LabelId)
                .Index(t => t.GroupId, name: "Group")
                .Index(t => new { t.GroupId, t.LabelId }, unique: true, name: "Group_Label")
                .Index(t => t.LabelId, name: "Label");
            
            CreateTable(
                "public.LastVisits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.CourseId, t.UserId }, name: "Course_User");
            
            CreateTable(
                "public.LtiSlideRequests",
                c => new
                    {
                        RequestId = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 64),
                        Request = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.RequestId)
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "Slide_User");
            
            CreateTable(
                "public.NotificationDeliveries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NotificationId = c.Int(nullable: false),
                        NotificationTransportId = c.Int(nullable: false),
                        Status = c.Short(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        NextTryTime = c.DateTime(),
                        FailsCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.Notifications", t => t.NotificationId, cascadeDelete: true)
                .ForeignKey("public.NotificationTransports", t => t.NotificationTransportId, cascadeDelete: true)
                .Index(t => new { t.NotificationId, t.NotificationTransportId }, name: "Notification_Transport")
                .Index(t => t.CreateTime, name: "CreateTime")
                .Index(t => t.NextTryTime, name: "NextTryTime");
            
            CreateTable(
                "public.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        InitiatedById = c.String(nullable: false, maxLength: 128),
                        CreateTime = c.DateTime(nullable: false),
                        AreDeliveriesCreated = c.Boolean(nullable: false),
                        CheckingId = c.Int(),
                        IsRecheck = c.Boolean(),
                        CommentId = c.Int(),
                        CommentId1 = c.Int(),
                        LikedUserId = c.String(maxLength: 128),
                        ParentCommentId = c.Int(),
                        UserIds = c.String(),
                        UserDescriptions = c.String(),
                        GroupId = c.Int(),
                        NotUploadedPackageNotification_CommitHash = c.String(),
                        NotUploadedPackageNotification_RepoUrl = c.String(),
                        CourseVersionId = c.Guid(),
                        UploadedPackageNotification_CourseVersionId = c.Guid(),
                        AddedUserId = c.String(maxLength: 128),
                        ProcessId = c.Int(),
                        GroupId1 = c.Int(),
                        AccessId = c.Int(),
                        UserId = c.String(maxLength: 128),
                        GroupMemberHasBeenRemovedNotification_GroupId = c.Int(),
                        Text = c.String(),
                        JoinedToYourGroupNotification_GroupId = c.Int(),
                        JoinedUserId = c.String(maxLength: 128),
                        PassedManualQuizCheckingNotification_CheckingId = c.Int(),
                        ScoreId = c.Int(),
                        CertificateId = c.Guid(),
                        RevokedAccessToGroupNotification_AccessId = c.Int(),
                        Text1 = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.InitiatedById)
                .ForeignKey("public.ManualExerciseCheckings", t => t.CheckingId)
                .ForeignKey("public.ExerciseCodeReviewComments", t => t.CommentId)
                .ForeignKey("public.Comments", t => t.CommentId1, cascadeDelete: true)
                .ForeignKey("public.AspNetUsers", t => t.LikedUserId)
                .ForeignKey("public.Comments", t => t.ParentCommentId)
                .ForeignKey("public.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("public.CourseVersions", t => t.CourseVersionId)
                .ForeignKey("public.CourseVersions", t => t.UploadedPackageNotification_CourseVersionId)
                .ForeignKey("public.AspNetUsers", t => t.AddedUserId)
                .ForeignKey("public.StepikExportProcesses", t => t.ProcessId)
                .ForeignKey("public.Groups", t => t.GroupId1)
                .ForeignKey("public.GroupAccesses", t => t.AccessId)
                .ForeignKey("public.Groups", t => t.GroupMemberHasBeenRemovedNotification_GroupId)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .ForeignKey("public.Groups", t => t.JoinedToYourGroupNotification_GroupId)
                .ForeignKey("public.AspNetUsers", t => t.JoinedUserId)
                .ForeignKey("public.ManualQuizCheckings", t => t.PassedManualQuizCheckingNotification_CheckingId)
                .ForeignKey("public.AdditionalScores", t => t.ScoreId)
                .ForeignKey("public.Certificates", t => t.CertificateId, cascadeDelete: true)
                .ForeignKey("public.GroupAccesses", t => t.RevokedAccessToGroupNotification_AccessId)
                .Index(t => t.CourseId, name: "Course")
                .Index(t => t.InitiatedById)
                .Index(t => t.CreateTime, name: "CreateTime")
                .Index(t => t.AreDeliveriesCreated, name: "AreDeliveriesCreated")
                .Index(t => t.CheckingId)
                .Index(t => t.CommentId)
                .Index(t => t.CommentId1)
                .Index(t => t.LikedUserId)
                .Index(t => t.ParentCommentId)
                .Index(t => t.GroupId)
                .Index(t => t.CourseVersionId)
                .Index(t => t.UploadedPackageNotification_CourseVersionId)
                .Index(t => t.AddedUserId)
                .Index(t => t.ProcessId)
                .Index(t => t.GroupId1)
                .Index(t => t.AccessId)
                .Index(t => t.UserId)
                .Index(t => t.GroupMemberHasBeenRemovedNotification_GroupId)
                .Index(t => t.JoinedToYourGroupNotification_GroupId)
                .Index(t => t.JoinedUserId)
                .Index(t => t.PassedManualQuizCheckingNotification_CheckingId)
                .Index(t => t.ScoreId)
                .Index(t => t.CertificateId)
                .Index(t => t.RevokedAccessToGroupNotification_AccessId);
            
            CreateTable(
                "public.StepikExportProcesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UlearnCourseId = c.String(nullable: false, maxLength: 100),
                        StepikCourseId = c.Int(nullable: false),
                        StepikCourseTitle = c.String(maxLength: 100),
                        IsFinished = c.Boolean(nullable: false),
                        IsInitialExport = c.Boolean(nullable: false),
                        IsSuccess = c.Boolean(nullable: false),
                        Log = c.String(),
                        OwnerId = c.String(nullable: false, maxLength: 128),
                        StartTime = c.DateTime(nullable: false),
                        FinishTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.OwnerId)
                .Index(t => t.OwnerId, name: "Owner");
            
            CreateTable(
                "public.NotificationTransportSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NotificationTransportId = c.Int(nullable: false),
                        CourseId = c.String(maxLength: 100),
                        NotificationType = c.Short(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.NotificationTransports", t => t.NotificationTransportId, cascadeDelete: true)
                .Index(t => t.NotificationTransportId, name: "NotificationTransport")
                .Index(t => t.CourseId, name: "Course")
                .Index(t => new { t.CourseId, t.NotificationType }, name: "Course_NotificationType")
                .Index(t => t.NotificationType, name: "NotificationType");
            
            CreateTable(
                "public.RestoreRequests",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "public.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "public.SlideRates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Rate = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.SlideId, t.UserId }, name: "SlideAndUser");
            
            CreateTable(
                "public.StepikAccessTokens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        AccessToken = c.String(nullable: false, maxLength: 100),
                        AddedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.AddedTime);
            
            CreateTable(
                "public.StepikExportSlideAndStepMaps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UlearnCourseId = c.String(nullable: false, maxLength: 100),
                        StepikCourseId = c.Int(nullable: false),
                        SlideId = c.Guid(nullable: false),
                        StepId = c.Int(nullable: false),
                        SlideXml = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UlearnCourseId, t.SlideId }, name: "UlearnCourse_Slide")
                .Index(t => new { t.UlearnCourseId, t.StepikCourseId }, name: "UlearnCourse_StepikCourse")
                .Index(t => t.UlearnCourseId, name: "UlearnCourseId");
            
            CreateTable(
                "public.StyleErrorSettings",
                c => new
                    {
                        ErrorType = c.Int(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ErrorType);
            
            CreateTable(
                "public.SystemAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        GrantedById = c.String(nullable: false, maxLength: 128),
                        AccessType = c.Short(nullable: false),
                        GrantTime = c.DateTime(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.GrantedById)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId, name: "User")
                .Index(t => new { t.UserId, t.IsEnabled }, name: "User_IsEnabled")
                .Index(t => t.GrantedById)
                .Index(t => t.GrantTime, name: "GrantTime")
                .Index(t => t.IsEnabled, name: "IsEnabled");
            
            CreateTable(
                "public.TempCourseErrors",
                c => new
                    {
                        CourseId = c.String(nullable: false, maxLength: 100),
                        Error = c.String(),
                    })
                .PrimaryKey(t => t.CourseId);
            
            CreateTable(
                "public.TempCourses",
                c => new
                    {
                        CourseId = c.String(nullable: false, maxLength: 100),
                        LoadingTime = c.DateTime(nullable: false),
                        LastUpdateTime = c.DateTime(nullable: false),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.CourseId)
                .ForeignKey("public.AspNetUsers", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "public.UnitAppearances",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        UnitId = c.Guid(nullable: false),
                        UserName = c.String(nullable: false),
                        PublishTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.CourseId, t.PublishTime }, name: "Course_Time");
            
            CreateTable(
                "public.UserFlashcardsVisits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        UnitId = c.Guid(nullable: false),
                        FlashcardId = c.String(nullable: false, maxLength: 64),
                        Rate = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.CourseId, t.UnitId, t.FlashcardId }, name: "User_Course_Unit_Flashcard");
            
            CreateTable(
                "public.UserQuizAnswers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        BlockId = c.String(maxLength: 64),
                        ItemId = c.String(maxLength: 64),
                        Text = c.String(maxLength: 8192),
                        IsRightAnswer = c.Boolean(nullable: false),
                        QuizBlockScore = c.Int(nullable: false),
                        QuizBlockMaxScore = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.UserQuizSubmissions", t => t.SubmissionId, cascadeDelete: true)
                .Index(t => new { t.SubmissionId, t.BlockId }, name: "Submission_Block")
                .Index(t => t.ItemId, name: "Item");
            
            CreateTable(
                "public.UserRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false),
                        Role = c.Int(nullable: false),
                        GrantedById = c.String(),
                        GrantTime = c.DateTime(),
                        IsEnabled = c.Boolean(),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "public.Visits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        Score = c.Int(nullable: false),
                        HasManualChecking = c.Boolean(nullable: false),
                        AttemptsCount = c.Int(nullable: false),
                        IsSkipped = c.Boolean(nullable: false),
                        IsPassed = c.Boolean(nullable: false),
                        IpAddress = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "Course_Slide_User")
                .Index(t => new { t.UserId, t.SlideId }, name: "Slide_User")
                .Index(t => new { t.SlideId, t.Timestamp }, name: "Slide_Time");
            
            CreateTable(
                "public.WorkQueueItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QueueId = c.Int(nullable: false),
                        ItemId = c.String(nullable: false),
                        Priority = c.Int(nullable: false),
                        Type = c.String(),
                        TakeAfterTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "public.XQueueExerciseSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubmissionId = c.Int(nullable: false),
                        WatcherId = c.Int(nullable: false),
                        XQueueHeader = c.String(nullable: false),
                        IsResultSent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.UserExerciseSubmissions", t => t.SubmissionId, cascadeDelete: true)
                .ForeignKey("public.XQueueWatchers", t => t.WatcherId, cascadeDelete: true)
                .Index(t => t.SubmissionId)
                .Index(t => t.WatcherId);
            
            CreateTable(
                "public.XQueueWatchers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        BaseUrl = c.String(nullable: false),
                        QueueName = c.String(nullable: false),
                        UserName = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.XQueueExerciseSubmissions", "WatcherId", "public.XQueueWatchers");
            DropForeignKey("public.XQueueWatchers", "UserId", "public.AspNetUsers");
            DropForeignKey("public.XQueueExerciseSubmissions", "SubmissionId", "public.UserExerciseSubmissions");
            DropForeignKey("public.Visits", "UserId", "public.AspNetUsers");
            DropForeignKey("public.UserRoles", "UserId", "public.AspNetUsers");
            DropForeignKey("public.UserQuizAnswers", "SubmissionId", "public.UserQuizSubmissions");
            DropForeignKey("public.UserFlashcardsVisits", "UserId", "public.AspNetUsers");
            DropForeignKey("public.TempCourses", "AuthorId", "public.AspNetUsers");
            DropForeignKey("public.SystemAccesses", "UserId", "public.AspNetUsers");
            DropForeignKey("public.SystemAccesses", "GrantedById", "public.AspNetUsers");
            DropForeignKey("public.StepikAccessTokens", "UserId", "public.AspNetUsers");
            DropForeignKey("public.SlideRates", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AspNetUserRoles", "RoleId", "public.AspNetRoles");
            DropForeignKey("public.RestoreRequests", "UserId", "public.AspNetUsers");
            DropForeignKey("public.NotificationTransportSettings", "NotificationTransportId", "public.NotificationTransports");
            DropForeignKey("public.NotificationDeliveries", "NotificationTransportId", "public.NotificationTransports");
            DropForeignKey("public.NotificationDeliveries", "NotificationId", "public.Notifications");
            DropForeignKey("public.Notifications", "RevokedAccessToGroupNotification_AccessId", "public.GroupAccesses");
            DropForeignKey("public.Notifications", "CertificateId", "public.Certificates");
            DropForeignKey("public.Notifications", "ScoreId", "public.AdditionalScores");
            DropForeignKey("public.Notifications", "PassedManualQuizCheckingNotification_CheckingId", "public.ManualQuizCheckings");
            DropForeignKey("public.Notifications", "JoinedUserId", "public.AspNetUsers");
            DropForeignKey("public.Notifications", "JoinedToYourGroupNotification_GroupId", "public.Groups");
            DropForeignKey("public.Notifications", "UserId", "public.AspNetUsers");
            DropForeignKey("public.Notifications", "GroupMemberHasBeenRemovedNotification_GroupId", "public.Groups");
            DropForeignKey("public.Notifications", "AccessId", "public.GroupAccesses");
            DropForeignKey("public.Notifications", "GroupId1", "public.Groups");
            DropForeignKey("public.Notifications", "ProcessId", "public.StepikExportProcesses");
            DropForeignKey("public.StepikExportProcesses", "OwnerId", "public.AspNetUsers");
            DropForeignKey("public.Notifications", "AddedUserId", "public.AspNetUsers");
            DropForeignKey("public.Notifications", "UploadedPackageNotification_CourseVersionId", "public.CourseVersions");
            DropForeignKey("public.Notifications", "CourseVersionId", "public.CourseVersions");
            DropForeignKey("public.Notifications", "GroupId", "public.Groups");
            DropForeignKey("public.Notifications", "ParentCommentId", "public.Comments");
            DropForeignKey("public.Notifications", "LikedUserId", "public.AspNetUsers");
            DropForeignKey("public.Notifications", "CommentId1", "public.Comments");
            DropForeignKey("public.Notifications", "CommentId", "public.ExerciseCodeReviewComments");
            DropForeignKey("public.Notifications", "CheckingId", "public.ManualExerciseCheckings");
            DropForeignKey("public.Notifications", "InitiatedById", "public.AspNetUsers");
            DropForeignKey("public.LastVisits", "UserId", "public.AspNetUsers");
            DropForeignKey("public.LabelOnGroups", "LabelId", "public.GroupLabels");
            DropForeignKey("public.LabelOnGroups", "GroupId", "public.Groups");
            DropForeignKey("public.SlideHints", "UserId", "public.AspNetUsers");
            DropForeignKey("public.GroupLabels", "OwnerId", "public.AspNetUsers");
            DropForeignKey("public.GroupAccesses", "UserId", "public.AspNetUsers");
            DropForeignKey("public.GroupAccesses", "GroupId", "public.Groups");
            DropForeignKey("public.GroupAccesses", "GrantedById", "public.AspNetUsers");
            DropForeignKey("public.FeedViewTimestamps", "TransportId", "public.NotificationTransports");
            DropForeignKey("public.NotificationTransports", "UserId", "public.AspNetUsers");
            DropForeignKey("public.ExerciseSolutionByGraders", "SubmissionId", "public.UserExerciseSubmissions");
            DropForeignKey("public.ExerciseSolutionByGraders", "ClientId", "public.GraderClients");
            DropForeignKey("public.GraderClients", "UserId", "public.AspNetUsers");
            DropForeignKey("public.ManualExerciseCheckings", "UserId", "public.AspNetUsers");
            DropForeignKey("public.UserExerciseSubmissions", "UserId", "public.AspNetUsers");
            DropForeignKey("public.UserExerciseSubmissions", "SolutionCodeHash", "public.TextBlobs");
            DropForeignKey("public.ExerciseCodeReviews", "SubmissionId", "public.UserExerciseSubmissions");
            DropForeignKey("public.ManualExerciseCheckings", "SubmissionId", "public.UserExerciseSubmissions");
            DropForeignKey("public.Likes", "UserId", "public.AspNetUsers");
            DropForeignKey("public.Likes", "SubmissionId", "public.UserExerciseSubmissions");
            DropForeignKey("public.UserExerciseSubmissions", "AutomaticCheckingId", "public.AutomaticExerciseCheckings");
            DropForeignKey("public.ExerciseCodeReviews", "ExerciseCheckingId", "public.ManualExerciseCheckings");
            DropForeignKey("public.ManualExerciseCheckings", "LockedById", "public.AspNetUsers");
            DropForeignKey("public.ExerciseCodeReviewComments", "ReviewId", "public.ExerciseCodeReviews");
            DropForeignKey("public.ExerciseCodeReviews", "AuthorId", "public.AspNetUsers");
            DropForeignKey("public.ExerciseCodeReviewComments", "AuthorId", "public.AspNetUsers");
            DropForeignKey("public.EnabledAdditionalScoringGroups", "GroupId", "public.Groups");
            DropForeignKey("public.Groups", "OwnerId", "public.AspNetUsers");
            DropForeignKey("public.GroupMembers", "UserId", "public.AspNetUsers");
            DropForeignKey("public.GroupMembers", "GroupId", "public.Groups");
            DropForeignKey("public.CourseFiles", "CourseVersionId", "public.CourseVersions");
            DropForeignKey("public.CourseVersions", "AuthorId", "public.AspNetUsers");
            DropForeignKey("public.CourseAccesses", "UserId", "public.AspNetUsers");
            DropForeignKey("public.CourseAccesses", "GrantedById", "public.AspNetUsers");
            DropForeignKey("public.CommentLikes", "UserId", "public.AspNetUsers");
            DropForeignKey("public.CommentLikes", "CommentId", "public.Comments");
            DropForeignKey("public.Comments", "AuthorId", "public.AspNetUsers");
            DropForeignKey("public.CertificateTemplateArchives", "CertificateTemplateId", "public.CertificateTemplates");
            DropForeignKey("public.Certificates", "UserId", "public.AspNetUsers");
            DropForeignKey("public.Certificates", "TemplateId", "public.CertificateTemplates");
            DropForeignKey("public.Certificates", "InstructorId", "public.AspNetUsers");
            DropForeignKey("public.AutomaticQuizCheckings", "UserId", "public.AspNetUsers");
            DropForeignKey("public.UserQuizSubmissions", "UserId", "public.AspNetUsers");
            DropForeignKey("public.ManualQuizCheckings", "Id", "public.UserQuizSubmissions");
            DropForeignKey("public.ManualQuizCheckings", "UserId", "public.AspNetUsers");
            DropForeignKey("public.ManualQuizCheckings", "LockedById", "public.AspNetUsers");
            DropForeignKey("public.AutomaticQuizCheckings", "Id", "public.UserQuizSubmissions");
            DropForeignKey("public.AutomaticExerciseCheckings", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AutomaticExerciseCheckings", "OutputHash", "public.TextBlobs");
            DropForeignKey("public.AutomaticExerciseCheckings", "CompilationErrorHash", "public.TextBlobs");
            DropForeignKey("public.AdditionalScores", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AdditionalScores", "InstructorId", "public.AspNetUsers");
            DropForeignKey("public.AspNetUserRoles", "UserId", "public.AspNetUsers");
            DropForeignKey("public.UserQuestions", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AspNetUserLogins", "UserId", "public.AspNetUsers");
            DropForeignKey("public.AspNetUserClaims", "UserId", "public.AspNetUsers");
            DropIndex("public.XQueueWatchers", new[] { "UserId" });
            DropIndex("public.XQueueExerciseSubmissions", new[] { "WatcherId" });
            DropIndex("public.XQueueExerciseSubmissions", new[] { "SubmissionId" });
            DropIndex("public.Visits", "Slide_Time");
            DropIndex("public.Visits", "Slide_User");
            DropIndex("public.Visits", "Course_Slide_User");
            DropIndex("public.UserRoles", new[] { "UserId" });
            DropIndex("public.UserQuizAnswers", "Item");
            DropIndex("public.UserQuizAnswers", "Submission_Block");
            DropIndex("public.UserFlashcardsVisits", "User_Course_Unit_Flashcard");
            DropIndex("public.UnitAppearances", "Course_Time");
            DropIndex("public.TempCourses", new[] { "AuthorId" });
            DropIndex("public.SystemAccesses", "IsEnabled");
            DropIndex("public.SystemAccesses", "GrantTime");
            DropIndex("public.SystemAccesses", new[] { "GrantedById" });
            DropIndex("public.SystemAccesses", "User_IsEnabled");
            DropIndex("public.SystemAccesses", "User");
            DropIndex("public.StepikExportSlideAndStepMaps", "UlearnCourseId");
            DropIndex("public.StepikExportSlideAndStepMaps", "UlearnCourse_StepikCourse");
            DropIndex("public.StepikExportSlideAndStepMaps", "UlearnCourse_Slide");
            DropIndex("public.StepikAccessTokens", new[] { "AddedTime" });
            DropIndex("public.StepikAccessTokens", new[] { "UserId" });
            DropIndex("public.SlideRates", "SlideAndUser");
            DropIndex("public.AspNetRoles", "RoleNameIndex");
            DropIndex("public.RestoreRequests", new[] { "UserId" });
            DropIndex("public.NotificationTransportSettings", "NotificationType");
            DropIndex("public.NotificationTransportSettings", "Course_NotificationType");
            DropIndex("public.NotificationTransportSettings", "Course");
            DropIndex("public.NotificationTransportSettings", "NotificationTransport");
            DropIndex("public.StepikExportProcesses", "Owner");
            DropIndex("public.Notifications", new[] { "RevokedAccessToGroupNotification_AccessId" });
            DropIndex("public.Notifications", new[] { "CertificateId" });
            DropIndex("public.Notifications", new[] { "ScoreId" });
            DropIndex("public.Notifications", new[] { "PassedManualQuizCheckingNotification_CheckingId" });
            DropIndex("public.Notifications", new[] { "JoinedUserId" });
            DropIndex("public.Notifications", new[] { "JoinedToYourGroupNotification_GroupId" });
            DropIndex("public.Notifications", new[] { "GroupMemberHasBeenRemovedNotification_GroupId" });
            DropIndex("public.Notifications", new[] { "UserId" });
            DropIndex("public.Notifications", new[] { "AccessId" });
            DropIndex("public.Notifications", new[] { "GroupId1" });
            DropIndex("public.Notifications", new[] { "ProcessId" });
            DropIndex("public.Notifications", new[] { "AddedUserId" });
            DropIndex("public.Notifications", new[] { "UploadedPackageNotification_CourseVersionId" });
            DropIndex("public.Notifications", new[] { "CourseVersionId" });
            DropIndex("public.Notifications", new[] { "GroupId" });
            DropIndex("public.Notifications", new[] { "ParentCommentId" });
            DropIndex("public.Notifications", new[] { "LikedUserId" });
            DropIndex("public.Notifications", new[] { "CommentId1" });
            DropIndex("public.Notifications", new[] { "CommentId" });
            DropIndex("public.Notifications", new[] { "CheckingId" });
            DropIndex("public.Notifications", "AreDeliveriesCreated");
            DropIndex("public.Notifications", "CreateTime");
            DropIndex("public.Notifications", new[] { "InitiatedById" });
            DropIndex("public.Notifications", "Course");
            DropIndex("public.NotificationDeliveries", "NextTryTime");
            DropIndex("public.NotificationDeliveries", "CreateTime");
            DropIndex("public.NotificationDeliveries", "Notification_Transport");
            DropIndex("public.LtiSlideRequests", "Slide_User");
            DropIndex("public.LastVisits", "Course_User");
            DropIndex("public.LabelOnGroups", "Label");
            DropIndex("public.LabelOnGroups", "Group_Label");
            DropIndex("public.LabelOnGroups", "Group");
            DropIndex("public.SlideHints", "FullIndex");
            DropIndex("public.GroupLabels", "Owner_IsDeleted");
            DropIndex("public.GroupLabels", "Owner");
            DropIndex("public.GroupAccesses", "GrantTime");
            DropIndex("public.GroupAccesses", new[] { "GrantedById" });
            DropIndex("public.GroupAccesses", "User");
            DropIndex("public.GroupAccesses", "Group_User_IsEnabled");
            DropIndex("public.GroupAccesses", "Group_IsEnabled");
            DropIndex("public.GroupAccesses", "Group");
            DropIndex("public.NotificationTransports", "User_IsDeleted");
            DropIndex("public.NotificationTransports", "User");
            DropIndex("public.FeedViewTimestamps", "Timestamp");
            DropIndex("public.FeedViewTimestamps", "User_Transport");
            DropIndex("public.FeedViewTimestamps", "User");
            DropIndex("public.GraderClients", new[] { "UserId" });
            DropIndex("public.ExerciseSolutionByGraders", new[] { "SubmissionId" });
            DropIndex("public.ExerciseSolutionByGraders", new[] { "ClientId" });
            DropIndex("public.Likes", "User_Submission");
            DropIndex("public.Likes", "Submission");
            DropIndex("public.UserExerciseSubmissions", "AntiPlagiarismSubmission");
            DropIndex("public.UserExerciseSubmissions", "Sandbox");
            DropIndex("public.UserExerciseSubmissions", "Language");
            DropIndex("public.UserExerciseSubmissions", "IsRightAnswer");
            DropIndex("public.UserExerciseSubmissions", new[] { "AutomaticCheckingId" });
            DropIndex("public.UserExerciseSubmissions", new[] { "SolutionCodeHash" });
            DropIndex("public.UserExerciseSubmissions", "Time");
            DropIndex("public.UserExerciseSubmissions", "Course_Slide_Time");
            DropIndex("public.UserExerciseSubmissions", "Course_Slide_IsRightAnswer");
            DropIndex("public.UserExerciseSubmissions", "Course_Slide");
            DropIndex("public.UserExerciseSubmissions", "Course_IsRightAnswer");
            DropIndex("public.UserExerciseSubmissions", "Course_Slide_User");
            DropIndex("public.ManualExerciseCheckings", "Course_User");
            DropIndex("public.ManualExerciseCheckings", "Course_Slide_Time");
            DropIndex("public.ManualExerciseCheckings", "Course_Slide");
            DropIndex("public.ManualExerciseCheckings", new[] { "LockedById" });
            DropIndex("public.ManualExerciseCheckings", "Course_Slide_User");
            DropIndex("public.ManualExerciseCheckings", new[] { "SubmissionId" });
            DropIndex("public.ExerciseCodeReviews", new[] { "AuthorId" });
            DropIndex("public.ExerciseCodeReviews", "Submission");
            DropIndex("public.ExerciseCodeReviews", "ManualExerciseChecking");
            DropIndex("public.ExerciseCodeReviewComments", "AddingTime");
            DropIndex("public.ExerciseCodeReviewComments", new[] { "AuthorId" });
            DropIndex("public.ExerciseCodeReviewComments", "Review_IsDeleted");
            DropIndex("public.GroupMembers", "Member");
            DropIndex("public.GroupMembers", "Group");
            DropIndex("public.Groups", "InviteHash");
            DropIndex("public.Groups", "Owner");
            DropIndex("public.Groups", "Course");
            DropIndex("public.EnabledAdditionalScoringGroups", "Group");
            DropIndex("public.CourseVersions", new[] { "AuthorId" });
            DropIndex("public.CourseVersions", "Course_PublishTime");
            DropIndex("public.CourseVersions", "Course_LoadingTime");
            DropIndex("public.CourseFiles", "CourseVersion");
            DropIndex("public.CourseAccesses", "GrantTime");
            DropIndex("public.CourseAccesses", new[] { "GrantedById" });
            DropIndex("public.CourseAccesses", "User");
            DropIndex("public.CourseAccesses", "Course_User_IsEnabled");
            DropIndex("public.CourseAccesses", "Course_IsEnabled");
            DropIndex("public.CourseAccesses", "Course");
            DropIndex("public.LtiConsumers", "Key");
            DropIndex("public.Comments", "Author_PublishTime");
            DropIndex("public.Comments", "Slide");
            DropIndex("public.CommentLikes", "Comment");
            DropIndex("public.CommentLikes", "User_Comment");
            DropIndex("public.CertificateTemplateArchives", "CertificateTemplate");
            DropIndex("public.CertificateTemplates", "Course");
            DropIndex("public.Certificates", new[] { "InstructorId" });
            DropIndex("public.Certificates", "User");
            DropIndex("public.Certificates", "Template");
            DropIndex("public.ManualQuizCheckings", "Course_User");
            DropIndex("public.ManualQuizCheckings", "Course_Slide_User");
            DropIndex("public.ManualQuizCheckings", "Course_Slide_Time");
            DropIndex("public.ManualQuizCheckings", "Course_Slide");
            DropIndex("public.ManualQuizCheckings", new[] { "LockedById" });
            DropIndex("public.ManualQuizCheckings", new[] { "Id" });
            DropIndex("public.UserQuizSubmissions", "Course_Slide_Time");
            DropIndex("public.UserQuizSubmissions", "Course_Slide");
            DropIndex("public.UserQuizSubmissions", "Course_Slide_User");
            DropIndex("public.AutomaticQuizCheckings", "Course_User");
            DropIndex("public.AutomaticQuizCheckings", "Course_Slide_User");
            DropIndex("public.AutomaticQuizCheckings", "Course_Slide_Time");
            DropIndex("public.AutomaticQuizCheckings", "Course_Slide");
            DropIndex("public.AutomaticQuizCheckings", new[] { "Id" });
            DropIndex("public.AutomaticExerciseCheckings", "Course_User");
            DropIndex("public.AutomaticExerciseCheckings", "Course_Slide_User");
            DropIndex("public.AutomaticExerciseCheckings", "Course_Slide_Time");
            DropIndex("public.AutomaticExerciseCheckings", "Course_Slide");
            DropIndex("public.AutomaticExerciseCheckings", new[] { "OutputHash" });
            DropIndex("public.AutomaticExerciseCheckings", new[] { "CompilationErrorHash" });
            DropIndex("public.AutomaticExerciseCheckings", "IsRightanswer");
            DropIndex("public.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("public.AspNetUserRoles", new[] { "UserId" });
            DropIndex("public.UserQuestions", new[] { "UserId" });
            DropIndex("public.AspNetUserLogins", new[] { "UserId" });
            DropIndex("public.AspNetUserClaims", new[] { "UserId" });
            DropIndex("public.AspNetUsers", "UserNameIndex");
            DropIndex("public.AspNetUsers", "IsDeleted");
            DropIndex("public.AspNetUsers", "TelegramChat");
            DropIndex("public.AdditionalScores", new[] { "InstructorId" });
            DropIndex("public.AdditionalScores", "Unit_User");
            DropIndex("public.AdditionalScores", "Unit");
            DropIndex("public.AdditionalScores", "Course_User");
            DropIndex("public.AdditionalScores", "Course_Unit_ScoringGroup_User");
            DropTable("public.XQueueWatchers");
            DropTable("public.XQueueExerciseSubmissions");
            DropTable("public.WorkQueueItems");
            DropTable("public.Visits");
            DropTable("public.UserRoles");
            DropTable("public.UserQuizAnswers");
            DropTable("public.UserFlashcardsVisits");
            DropTable("public.UnitAppearances");
            DropTable("public.TempCourses");
            DropTable("public.TempCourseErrors");
            DropTable("public.SystemAccesses");
            DropTable("public.StyleErrorSettings");
            DropTable("public.StepikExportSlideAndStepMaps");
            DropTable("public.StepikAccessTokens");
            DropTable("public.SlideRates");
            DropTable("public.AspNetRoles");
            DropTable("public.RestoreRequests");
            DropTable("public.NotificationTransportSettings");
            DropTable("public.StepikExportProcesses");
            DropTable("public.Notifications");
            DropTable("public.NotificationDeliveries");
            DropTable("public.LtiSlideRequests");
            DropTable("public.LastVisits");
            DropTable("public.LabelOnGroups");
            DropTable("public.SlideHints");
            DropTable("public.GroupLabels");
            DropTable("public.GroupAccesses");
            DropTable("public.NotificationTransports");
            DropTable("public.FeedViewTimestamps");
            DropTable("public.GraderClients");
            DropTable("public.ExerciseSolutionByGraders");
            DropTable("public.Likes");
            DropTable("public.UserExerciseSubmissions");
            DropTable("public.ManualExerciseCheckings");
            DropTable("public.ExerciseCodeReviews");
            DropTable("public.ExerciseCodeReviewComments");
            DropTable("public.GroupMembers");
            DropTable("public.Groups");
            DropTable("public.EnabledAdditionalScoringGroups");
            DropTable("public.CourseGitRepos");
            DropTable("public.CourseVersions");
            DropTable("public.CourseFiles");
            DropTable("public.CourseAccesses");
            DropTable("public.LtiConsumers");
            DropTable("public.CommentsPolicies");
            DropTable("public.Comments");
            DropTable("public.CommentLikes");
            DropTable("public.CertificateTemplateArchives");
            DropTable("public.CertificateTemplates");
            DropTable("public.Certificates");
            DropTable("public.ManualQuizCheckings");
            DropTable("public.UserQuizSubmissions");
            DropTable("public.AutomaticQuizCheckings");
            DropTable("public.TextBlobs");
            DropTable("public.AutomaticExerciseCheckings");
            DropTable("public.AspNetUserRoles");
            DropTable("public.UserQuestions");
            DropTable("public.AspNetUserLogins");
            DropTable("public.AspNetUserClaims");
            DropTable("public.AspNetUsers");
            DropTable("public.AdditionalScores");
        }
    }
}
