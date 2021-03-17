SELECT setval('antiplagiarism."Clients_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Clients"), 1), false);

SELECT setval('antiplagiarism."Codes_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Codes"), 1), false);

SELECT setval('antiplagiarism."OldSubmissionsInfluenceBorder_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."OldSubmissionsInfluenceBorder"), 1), false);

SELECT setval('antiplagiarism."SnippetsOccurences_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."SnippetsOccurences"), 1), false);

SELECT setval('antiplagiarism."SnippetsStatistics_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."SnippetsStatistics"), 1), false);

SELECT setval('antiplagiarism."Snippets_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Snippets"), 1), false);

SELECT setval('antiplagiarism."Submissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."Submissions"), 1), false);

SELECT setval('antiplagiarism."TaskStatisticsSourceData_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."TaskStatisticsSourceData"), 1), false);

SELECT setval('antiplagiarism."WorkQueueItems_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM antiplagiarism."WorkQueueItems"), 1), false);

SELECT setval('public."AdditionalScores_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AdditionalScores"), 1), false);

SELECT setval('public."AspNetRoleClaims_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AspNetRoleClaims"), 1), false);

SELECT setval('public."AspNetUserClaims_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AspNetUserClaims"), 1), false);

SELECT setval('public."AutomaticExerciseCheckings_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."AutomaticExerciseCheckings"), 1), false);

SELECT setval('public."CommentLikes_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CommentLikes"), 1), false);

SELECT setval('public."Comments_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Comments"), 1), false);

SELECT setval('public."CourseAccesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CourseAccesses"), 1), false);

SELECT setval('public."CourseFiles_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CourseFiles"), 1), false);

SELECT setval('public."CourseGitRepos_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."CourseGitRepos"), 1), false);

SELECT setval('public."EnabledAdditionalScoringGroups_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."EnabledAdditionalScoringGroups"), 1), false);

SELECT setval('public."ExerciseCodeReviewComments_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ExerciseCodeReviewComments"), 1), false);

SELECT setval('public."ExerciseCodeReviews_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ExerciseCodeReviews"), 1), false);

SELECT setval('public."ExerciseSolutionByGraders_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ExerciseSolutionByGraders"), 1), false);

SELECT setval('public."FeedViewTimestamps_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."FeedViewTimestamps"), 1), false);

SELECT setval('public."GroupAccesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."GroupAccesses"), 1), false);

SELECT setval('public."GroupLabels_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."GroupLabels"), 1), false);

SELECT setval('public."GroupMembers_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."GroupMembers"), 1), false);

SELECT setval('public."Groups_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Groups"), 1), false);

SELECT setval('public."LabelOnGroups_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."LabelOnGroups"), 1), false);

SELECT setval('public."LastVisits_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."LastVisits"), 1), false);

SELECT setval('public."Likes_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Likes"), 1), false);

SELECT setval('public."LtiConsumers_ConsumerId_seq"', COALESCE((SELECT MAX("ConsumerId")+1 FROM public."LtiConsumers"), 1), false);

SELECT setval('public."LtiSlideRequests_RequestId_seq"', COALESCE((SELECT MAX("RequestId")+1 FROM public."LtiSlideRequests"), 1), false);

SELECT setval('public."ManualExerciseCheckings_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."ManualExerciseCheckings"), 1), false);

SELECT setval('public."NotificationDeliveries_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."NotificationDeliveries"), 1), false);

SELECT setval('public."NotificationTransportSettings_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."NotificationTransportSettings"), 1), false);

SELECT setval('public."NotificationTransports_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."NotificationTransports"), 1), false);

SELECT setval('public."Notifications_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Notifications"), 1), false);

SELECT setval('public."SlideHints_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."SlideHints"), 1), false);

SELECT setval('public."SlideRates_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."SlideRates"), 1), false);

SELECT setval('public."StepikAccessTokens_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."StepikAccessTokens"), 1), false);

SELECT setval('public."StepikExportProcesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."StepikExportProcesses"), 1), false);

SELECT setval('public."StepikExportSlideAndStepMaps_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."StepikExportSlideAndStepMaps"), 1), false);

SELECT setval('public."SystemAccesses_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."SystemAccesses"), 1), false);

SELECT setval('public."UnitAppearances_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UnitAppearances"), 1), false);

SELECT setval('public."UserExerciseSubmissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserExerciseSubmissions"), 1), false);

SELECT setval('public."UserFlashcardsUnlocking_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserFlashcardsUnlocking"), 1), false);

SELECT setval('public."UserFlashcardsVisits_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserFlashcardsVisits"), 1), false);

SELECT setval('public."UserQuestions_QuestionId_seq"', COALESCE((SELECT MAX("QuestionId")+1 FROM public."UserQuestions"), 1), false);

SELECT setval('public."UserQuizAnswers_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserQuizAnswers"), 1), false);

SELECT setval('public."UserQuizSubmissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserQuizSubmissions"), 1), false);

SELECT setval('public."UserRoles_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."UserRoles"), 1), false);

SELECT setval('public."Visits_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."Visits"), 1), false);

SELECT setval('public."WorkQueueItems_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."WorkQueueItems"), 1), false);

SELECT setval('public."XQueueExerciseSubmissions_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."XQueueExerciseSubmissions"), 1), false);

SELECT setval('public."XQueueWatchers_Id_seq"', COALESCE((SELECT MAX("Id")+1 FROM public."XQueueWatchers"), 1), false);

