
--
-- TOC entry 3731 (class 2606 OID 57165)
-- Name: MostSimilarSubmissions FK_MostSimilarSubmissions_Submissions_SimilarSubmissionId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."MostSimilarSubmissions"
    ADD CONSTRAINT "FK_MostSimilarSubmissions_Submissions_SimilarSubmissionId" FOREIGN KEY ("SimilarSubmissionId") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3732 (class 2606 OID 57170)
-- Name: MostSimilarSubmissions FK_MostSimilarSubmissions_Submissions_SubmissionId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."MostSimilarSubmissions"
    ADD CONSTRAINT "FK_MostSimilarSubmissions_Submissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3733 (class 2606 OID 57182)
-- Name: SnippetsOccurences FK_SnippetsOccurences_Snippets_SnippetId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsOccurences"
    ADD CONSTRAINT "FK_SnippetsOccurences_Snippets_SnippetId" FOREIGN KEY ("SnippetId") REFERENCES antiplagiarism."Snippets"("Id") ON DELETE CASCADE;


--
-- TOC entry 3734 (class 2606 OID 57187)
-- Name: SnippetsOccurences FK_SnippetsOccurences_Submissions_SubmissionId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsOccurences"
    ADD CONSTRAINT "FK_SnippetsOccurences_Submissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3729 (class 2606 OID 57150)
-- Name: SnippetsStatistics FK_SnippetsStatistics_Clients_ClientId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsStatistics"
    ADD CONSTRAINT "FK_SnippetsStatistics_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES antiplagiarism."Clients"("Id") ON DELETE CASCADE;


--
-- TOC entry 3730 (class 2606 OID 57155)
-- Name: SnippetsStatistics FK_SnippetsStatistics_Snippets_SnippetId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."SnippetsStatistics"
    ADD CONSTRAINT "FK_SnippetsStatistics_Snippets_SnippetId" FOREIGN KEY ("SnippetId") REFERENCES antiplagiarism."Snippets"("Id") ON DELETE CASCADE;


--
-- TOC entry 3727 (class 2606 OID 57133)
-- Name: Submissions FK_Submissions_Clients_ClientId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."Submissions"
    ADD CONSTRAINT "FK_Submissions_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES antiplagiarism."Clients"("Id") ON DELETE CASCADE;


--
-- TOC entry 3728 (class 2606 OID 57138)
-- Name: Submissions FK_Submissions_Codes_ProgramId; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."Submissions"
    ADD CONSTRAINT "FK_Submissions_Codes_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES antiplagiarism."Codes"("Id") ON DELETE CASCADE;


--
-- TOC entry 3735 (class 2606 OID 57199)
-- Name: TaskStatisticsSourceData FK_TaskStatisticsSourceData_Submissions_Submission1Id; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."TaskStatisticsSourceData"
    ADD CONSTRAINT "FK_TaskStatisticsSourceData_Submissions_Submission1Id" FOREIGN KEY ("Submission1Id") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3736 (class 2606 OID 57204)
-- Name: TaskStatisticsSourceData FK_TaskStatisticsSourceData_Submissions_Submission2Id; Type: FK CONSTRAINT; Schema: antiplagiarism; Owner: ulearn
--

ALTER TABLE ONLY antiplagiarism."TaskStatisticsSourceData"
    ADD CONSTRAINT "FK_TaskStatisticsSourceData_Submissions_Submission2Id" FOREIGN KEY ("Submission2Id") REFERENCES antiplagiarism."Submissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3738 (class 2606 OID 57361)
-- Name: AdditionalScores FK_AdditionalScores_AspNetUsers_InstructorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AdditionalScores"
    ADD CONSTRAINT "FK_AdditionalScores_AspNetUsers_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3739 (class 2606 OID 57366)
-- Name: AdditionalScores FK_AdditionalScores_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AdditionalScores"
    ADD CONSTRAINT "FK_AdditionalScores_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3737 (class 2606 OID 57349)
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 3740 (class 2606 OID 57381)
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3741 (class 2606 OID 57394)
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3742 (class 2606 OID 57407)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 3743 (class 2606 OID 57412)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3744 (class 2606 OID 57425)
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3773 (class 2606 OID 57772)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3774 (class 2606 OID 57777)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_TextBlobs_CompilationErrorHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_TextBlobs_CompilationErrorHash" FOREIGN KEY ("CompilationErrorHash") REFERENCES public."TextBlobs"("Hash") ON DELETE RESTRICT;


--
-- TOC entry 3775 (class 2606 OID 57782)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_TextBlobs_DebugLogsHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_TextBlobs_DebugLogsHash" FOREIGN KEY ("DebugLogsHash") REFERENCES public."TextBlobs"("Hash") ON DELETE RESTRICT;


--
-- TOC entry 3776 (class 2606 OID 57787)
-- Name: AutomaticExerciseCheckings FK_AutomaticExerciseCheckings_TextBlobs_OutputHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticExerciseCheckings"
    ADD CONSTRAINT "FK_AutomaticExerciseCheckings_TextBlobs_OutputHash" FOREIGN KEY ("OutputHash") REFERENCES public."TextBlobs"("Hash") ON DELETE RESTRICT;


--
-- TOC entry 3790 (class 2606 OID 57924)
-- Name: AutomaticQuizCheckings FK_AutomaticQuizCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticQuizCheckings"
    ADD CONSTRAINT "FK_AutomaticQuizCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3791 (class 2606 OID 57929)
-- Name: AutomaticQuizCheckings FK_AutomaticQuizCheckings_UserQuizSubmissions_Id; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."AutomaticQuizCheckings"
    ADD CONSTRAINT "FK_AutomaticQuizCheckings_UserQuizSubmissions_Id" FOREIGN KEY ("Id") REFERENCES public."UserQuizSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3772 (class 2606 OID 57757)
-- Name: CertificateTemplateArchives FK_CertificateTemplateArchives_CertificateTemplates_Certificat~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CertificateTemplateArchives"
    ADD CONSTRAINT "FK_CertificateTemplateArchives_CertificateTemplates_Certificat~" FOREIGN KEY ("CertificateTemplateId") REFERENCES public."CertificateTemplates"("Id") ON DELETE CASCADE;


--
-- TOC entry 3769 (class 2606 OID 57734)
-- Name: Certificates FK_Certificates_AspNetUsers_InstructorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "FK_Certificates_AspNetUsers_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3770 (class 2606 OID 57739)
-- Name: Certificates FK_Certificates_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "FK_Certificates_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3771 (class 2606 OID 57744)
-- Name: Certificates FK_Certificates_CertificateTemplates_TemplateId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Certificates"
    ADD CONSTRAINT "FK_Certificates_CertificateTemplates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES public."CertificateTemplates"("Id") ON DELETE CASCADE;


--
-- TOC entry 3777 (class 2606 OID 57799)
-- Name: CommentLikes FK_CommentLikes_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CommentLikes"
    ADD CONSTRAINT "FK_CommentLikes_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3778 (class 2606 OID 57804)
-- Name: CommentLikes FK_CommentLikes_Comments_CommentId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CommentLikes"
    ADD CONSTRAINT "FK_CommentLikes_Comments_CommentId" FOREIGN KEY ("CommentId") REFERENCES public."Comments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3745 (class 2606 OID 57440)
-- Name: Comments FK_Comments_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Comments"
    ADD CONSTRAINT "FK_Comments_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3746 (class 2606 OID 57455)
-- Name: CourseAccesses FK_CourseAccesses_AspNetUsers_GrantedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseAccesses"
    ADD CONSTRAINT "FK_CourseAccesses_AspNetUsers_GrantedById" FOREIGN KEY ("GrantedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3747 (class 2606 OID 57460)
-- Name: CourseAccesses FK_CourseAccesses_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseAccesses"
    ADD CONSTRAINT "FK_CourseAccesses_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3779 (class 2606 OID 57819)
-- Name: CourseFiles FK_CourseFiles_CourseVersions_CourseVersionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseFiles"
    ADD CONSTRAINT "FK_CourseFiles_CourseVersions_CourseVersionId" FOREIGN KEY ("CourseVersionId") REFERENCES public."CourseVersions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3748 (class 2606 OID 57473)
-- Name: CourseVersions FK_CourseVersions_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."CourseVersions"
    ADD CONSTRAINT "FK_CourseVersions_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3780 (class 2606 OID 57834)
-- Name: EnabledAdditionalScoringGroups FK_EnabledAdditionalScoringGroups_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."EnabledAdditionalScoringGroups"
    ADD CONSTRAINT "FK_EnabledAdditionalScoringGroups_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3811 (class 2606 OID 58105)
-- Name: ExerciseCodeReviewComments FK_ExerciseCodeReviewComments_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviewComments"
    ADD CONSTRAINT "FK_ExerciseCodeReviewComments_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3812 (class 2606 OID 58110)
-- Name: ExerciseCodeReviewComments FK_ExerciseCodeReviewComments_ExerciseCodeReviews_ReviewId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviewComments"
    ADD CONSTRAINT "FK_ExerciseCodeReviewComments_ExerciseCodeReviews_ReviewId" FOREIGN KEY ("ReviewId") REFERENCES public."ExerciseCodeReviews"("Id") ON DELETE CASCADE;


--
-- TOC entry 3808 (class 2606 OID 58080)
-- Name: ExerciseCodeReviews FK_ExerciseCodeReviews_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviews"
    ADD CONSTRAINT "FK_ExerciseCodeReviews_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3809 (class 2606 OID 58085)
-- Name: ExerciseCodeReviews FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviews"
    ADD CONSTRAINT "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~" FOREIGN KEY ("ExerciseCheckingId") REFERENCES public."ManualExerciseCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3810 (class 2606 OID 58090)
-- Name: ExerciseCodeReviews FK_ExerciseCodeReviews_UserExerciseSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseCodeReviews"
    ADD CONSTRAINT "FK_ExerciseCodeReviews_UserExerciseSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3799 (class 2606 OID 58001)
-- Name: ExerciseSolutionByGraders FK_ExerciseSolutionByGraders_GraderClients_ClientId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseSolutionByGraders"
    ADD CONSTRAINT "FK_ExerciseSolutionByGraders_GraderClients_ClientId" FOREIGN KEY ("ClientId") REFERENCES public."GraderClients"("Id") ON DELETE CASCADE;


--
-- TOC entry 3800 (class 2606 OID 58006)
-- Name: ExerciseSolutionByGraders FK_ExerciseSolutionByGraders_UserExerciseSubmissions_Submissio~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ExerciseSolutionByGraders"
    ADD CONSTRAINT "FK_ExerciseSolutionByGraders_UserExerciseSubmissions_Submissio~" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3788 (class 2606 OID 57902)
-- Name: FeedViewTimestamps FK_FeedViewTimestamps_NotificationTransports_TransportId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."FeedViewTimestamps"
    ADD CONSTRAINT "FK_FeedViewTimestamps_NotificationTransports_TransportId" FOREIGN KEY ("TransportId") REFERENCES public."NotificationTransports"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3749 (class 2606 OID 57483)
-- Name: GraderClients FK_GraderClients_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GraderClients"
    ADD CONSTRAINT "FK_GraderClients_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3781 (class 2606 OID 57846)
-- Name: GroupAccesses FK_GroupAccesses_AspNetUsers_GrantedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupAccesses"
    ADD CONSTRAINT "FK_GroupAccesses_AspNetUsers_GrantedById" FOREIGN KEY ("GrantedById") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3782 (class 2606 OID 57851)
-- Name: GroupAccesses FK_GroupAccesses_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupAccesses"
    ADD CONSTRAINT "FK_GroupAccesses_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3783 (class 2606 OID 57856)
-- Name: GroupAccesses FK_GroupAccesses_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupAccesses"
    ADD CONSTRAINT "FK_GroupAccesses_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3750 (class 2606 OID 57495)
-- Name: GroupLabels FK_GroupLabels_AspNetUsers_OwnerId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupLabels"
    ADD CONSTRAINT "FK_GroupLabels_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3784 (class 2606 OID 57868)
-- Name: GroupMembers FK_GroupMembers_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupMembers"
    ADD CONSTRAINT "FK_GroupMembers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3785 (class 2606 OID 57873)
-- Name: GroupMembers FK_GroupMembers_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."GroupMembers"
    ADD CONSTRAINT "FK_GroupMembers_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3751 (class 2606 OID 57507)
-- Name: Groups FK_Groups_AspNetUsers_OwnerId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Groups"
    ADD CONSTRAINT "FK_Groups_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3786 (class 2606 OID 57885)
-- Name: LabelOnGroups FK_LabelOnGroups_GroupLabels_LabelId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."LabelOnGroups"
    ADD CONSTRAINT "FK_LabelOnGroups_GroupLabels_LabelId" FOREIGN KEY ("LabelId") REFERENCES public."GroupLabels"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3787 (class 2606 OID 57890)
-- Name: LabelOnGroups FK_LabelOnGroups_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."LabelOnGroups"
    ADD CONSTRAINT "FK_LabelOnGroups_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3752 (class 2606 OID 57519)
-- Name: LastVisits FK_LastVisits_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."LastVisits"
    ADD CONSTRAINT "FK_LastVisits_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3801 (class 2606 OID 58018)
-- Name: Likes FK_Likes_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Likes"
    ADD CONSTRAINT "FK_Likes_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3802 (class 2606 OID 58023)
-- Name: Likes FK_Likes_UserExerciseSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Likes"
    ADD CONSTRAINT "FK_Likes_UserExerciseSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3803 (class 2606 OID 58035)
-- Name: ManualExerciseCheckings FK_ManualExerciseCheckings_AspNetUsers_LockedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualExerciseCheckings"
    ADD CONSTRAINT "FK_ManualExerciseCheckings_AspNetUsers_LockedById" FOREIGN KEY ("LockedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3804 (class 2606 OID 58040)
-- Name: ManualExerciseCheckings FK_ManualExerciseCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualExerciseCheckings"
    ADD CONSTRAINT "FK_ManualExerciseCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3805 (class 2606 OID 58045)
-- Name: ManualExerciseCheckings FK_ManualExerciseCheckings_UserExerciseSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualExerciseCheckings"
    ADD CONSTRAINT "FK_ManualExerciseCheckings_UserExerciseSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3792 (class 2606 OID 57939)
-- Name: ManualQuizCheckings FK_ManualQuizCheckings_AspNetUsers_LockedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualQuizCheckings"
    ADD CONSTRAINT "FK_ManualQuizCheckings_AspNetUsers_LockedById" FOREIGN KEY ("LockedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3793 (class 2606 OID 57944)
-- Name: ManualQuizCheckings FK_ManualQuizCheckings_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualQuizCheckings"
    ADD CONSTRAINT "FK_ManualQuizCheckings_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3794 (class 2606 OID 57949)
-- Name: ManualQuizCheckings FK_ManualQuizCheckings_UserQuizSubmissions_Id; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."ManualQuizCheckings"
    ADD CONSTRAINT "FK_ManualQuizCheckings_UserQuizSubmissions_Id" FOREIGN KEY ("Id") REFERENCES public."UserQuizSubmissions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3837 (class 2606 OID 58247)
-- Name: NotificationDeliveries FK_NotificationDeliveries_NotificationTransports_NotificationT~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationDeliveries"
    ADD CONSTRAINT "FK_NotificationDeliveries_NotificationTransports_NotificationT~" FOREIGN KEY ("NotificationTransportId") REFERENCES public."NotificationTransports"("Id") ON DELETE CASCADE;


--
-- TOC entry 3836 (class 2606 OID 58242)
-- Name: NotificationDeliveries FK_NotificationDeliveries_Notifications_NotificationId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationDeliveries"
    ADD CONSTRAINT "FK_NotificationDeliveries_Notifications_NotificationId" FOREIGN KEY ("NotificationId") REFERENCES public."Notifications"("Id") ON DELETE CASCADE;


--
-- TOC entry 3789 (class 2606 OID 57914)
-- Name: NotificationTransportSettings FK_NotificationTransportSettings_NotificationTransports_Notifi~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationTransportSettings"
    ADD CONSTRAINT "FK_NotificationTransportSettings_NotificationTransports_Notifi~" FOREIGN KEY ("NotificationTransportId") REFERENCES public."NotificationTransports"("Id") ON DELETE CASCADE;


--
-- TOC entry 3753 (class 2606 OID 57534)
-- Name: NotificationTransports FK_NotificationTransports_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."NotificationTransports"
    ADD CONSTRAINT "FK_NotificationTransports_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3813 (class 2606 OID 58125)
-- Name: Notifications FK_Notifications_AdditionalScores_ScoreId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AdditionalScores_ScoreId" FOREIGN KEY ("ScoreId") REFERENCES public."AdditionalScores"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3814 (class 2606 OID 58130)
-- Name: Notifications FK_Notifications_AspNetUsers_AddedUserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_AddedUserId" FOREIGN KEY ("AddedUserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3815 (class 2606 OID 58135)
-- Name: Notifications FK_Notifications_AspNetUsers_InitiatedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_InitiatedById" FOREIGN KEY ("InitiatedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3816 (class 2606 OID 58140)
-- Name: Notifications FK_Notifications_AspNetUsers_JoinedUserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_JoinedUserId" FOREIGN KEY ("JoinedUserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3817 (class 2606 OID 58145)
-- Name: Notifications FK_Notifications_AspNetUsers_LikedUserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_LikedUserId" FOREIGN KEY ("LikedUserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3818 (class 2606 OID 58150)
-- Name: Notifications FK_Notifications_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3819 (class 2606 OID 58155)
-- Name: Notifications FK_Notifications_Certificates_CertificateId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Certificates_CertificateId" FOREIGN KEY ("CertificateId") REFERENCES public."Certificates"("Id") ON DELETE CASCADE;


--
-- TOC entry 3820 (class 2606 OID 58160)
-- Name: Notifications FK_Notifications_Comments_CommentId1; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Comments_CommentId1" FOREIGN KEY ("CommentId1") REFERENCES public."Comments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3821 (class 2606 OID 58165)
-- Name: Notifications FK_Notifications_Comments_ParentCommentId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Comments_ParentCommentId" FOREIGN KEY ("ParentCommentId") REFERENCES public."Comments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3822 (class 2606 OID 58170)
-- Name: Notifications FK_Notifications_CourseVersions_CourseVersionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_CourseVersions_CourseVersionId" FOREIGN KEY ("CourseVersionId") REFERENCES public."CourseVersions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3823 (class 2606 OID 58175)
-- Name: Notifications FK_Notifications_CourseVersions_UploadedPackageNotification_Co~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_CourseVersions_UploadedPackageNotification_Co~" FOREIGN KEY ("UploadedPackageNotification_CourseVersionId") REFERENCES public."CourseVersions"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3824 (class 2606 OID 58180)
-- Name: Notifications FK_Notifications_ExerciseCodeReviewComments_CommentId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_ExerciseCodeReviewComments_CommentId" FOREIGN KEY ("CommentId") REFERENCES public."ExerciseCodeReviewComments"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3825 (class 2606 OID 58185)
-- Name: Notifications FK_Notifications_GroupAccesses_AccessId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_GroupAccesses_AccessId" FOREIGN KEY ("AccessId") REFERENCES public."GroupAccesses"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3826 (class 2606 OID 58190)
-- Name: Notifications FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~" FOREIGN KEY ("RevokedAccessToGroupNotification_AccessId") REFERENCES public."GroupAccesses"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3827 (class 2606 OID 58195)
-- Name: Notifications FK_Notifications_Groups_GroupId1; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId1" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3828 (class 2606 OID 58200)
-- Name: Notifications FK_Notifications_Groups_GroupId2; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId2" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3829 (class 2606 OID 58205)
-- Name: Notifications FK_Notifications_Groups_GroupId3; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId3" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3835 (class 2606 OID 58432)
-- Name: Notifications FK_Notifications_Groups_GroupId4; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId4" FOREIGN KEY ("GroupId1") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- TOC entry 3830 (class 2606 OID 58210)
-- Name: Notifications FK_Notifications_Groups_GroupMemberHasBeenRemovedNotification_~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupMemberHasBeenRemovedNotification_~" FOREIGN KEY ("GroupMemberHasBeenRemovedNotification_GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3831 (class 2606 OID 58215)
-- Name: Notifications FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId" FOREIGN KEY ("JoinedToYourGroupNotification_GroupId") REFERENCES public."Groups"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3832 (class 2606 OID 58220)
-- Name: Notifications FK_Notifications_ManualExerciseCheckings_CheckingId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_ManualExerciseCheckings_CheckingId" FOREIGN KEY ("CheckingId") REFERENCES public."ManualExerciseCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3833 (class 2606 OID 58225)
-- Name: Notifications FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~" FOREIGN KEY ("PassedManualQuizCheckingNotification_CheckingId") REFERENCES public."ManualQuizCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3834 (class 2606 OID 58230)
-- Name: Notifications FK_Notifications_StepikExportProcesses_ProcessId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Notifications"
    ADD CONSTRAINT "FK_Notifications_StepikExportProcesses_ProcessId" FOREIGN KEY ("ProcessId") REFERENCES public."StepikExportProcesses"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3754 (class 2606 OID 57547)
-- Name: RestoreRequests FK_RestoreRequests_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."RestoreRequests"
    ADD CONSTRAINT "FK_RestoreRequests_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3755 (class 2606 OID 57559)
-- Name: SlideHints FK_SlideHints_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SlideHints"
    ADD CONSTRAINT "FK_SlideHints_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3756 (class 2606 OID 57571)
-- Name: SlideRates FK_SlideRates_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SlideRates"
    ADD CONSTRAINT "FK_SlideRates_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3757 (class 2606 OID 57583)
-- Name: StepikAccessTokens FK_StepikAccessTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."StepikAccessTokens"
    ADD CONSTRAINT "FK_StepikAccessTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3758 (class 2606 OID 57598)
-- Name: StepikExportProcesses FK_StepikExportProcesses_AspNetUsers_OwnerId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."StepikExportProcesses"
    ADD CONSTRAINT "FK_StepikExportProcesses_AspNetUsers_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3759 (class 2606 OID 57610)
-- Name: SystemAccesses FK_SystemAccesses_AspNetUsers_GrantedById; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SystemAccesses"
    ADD CONSTRAINT "FK_SystemAccesses_AspNetUsers_GrantedById" FOREIGN KEY ("GrantedById") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3760 (class 2606 OID 57615)
-- Name: SystemAccesses FK_SystemAccesses_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."SystemAccesses"
    ADD CONSTRAINT "FK_SystemAccesses_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3761 (class 2606 OID 57625)
-- Name: TempCourses FK_TempCourses_AspNetUsers_AuthorId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."TempCourses"
    ADD CONSTRAINT "FK_TempCourses_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3796 (class 2606 OID 57976)
-- Name: UserExerciseSubmissions FK_UserExerciseSubmissions_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserExerciseSubmissions"
    ADD CONSTRAINT "FK_UserExerciseSubmissions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3797 (class 2606 OID 57981)
-- Name: UserExerciseSubmissions FK_UserExerciseSubmissions_AutomaticExerciseCheckings_Automati~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserExerciseSubmissions"
    ADD CONSTRAINT "FK_UserExerciseSubmissions_AutomaticExerciseCheckings_Automati~" FOREIGN KEY ("AutomaticCheckingId") REFERENCES public."AutomaticExerciseCheckings"("Id") ON DELETE RESTRICT;


--
-- TOC entry 3798 (class 2606 OID 57986)
-- Name: UserExerciseSubmissions FK_UserExerciseSubmissions_TextBlobs_SolutionCodeHash; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserExerciseSubmissions"
    ADD CONSTRAINT "FK_UserExerciseSubmissions_TextBlobs_SolutionCodeHash" FOREIGN KEY ("SolutionCodeHash") REFERENCES public."TextBlobs"("Hash") ON DELETE CASCADE;


--
-- TOC entry 3762 (class 2606 OID 57637)
-- Name: UserFlashcardsUnlocking FK_UserFlashcardsUnlocking_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserFlashcardsUnlocking"
    ADD CONSTRAINT "FK_UserFlashcardsUnlocking_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3763 (class 2606 OID 57649)
-- Name: UserFlashcardsVisits FK_UserFlashcardsVisits_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserFlashcardsVisits"
    ADD CONSTRAINT "FK_UserFlashcardsVisits_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3764 (class 2606 OID 57664)
-- Name: UserQuestions FK_UserQuestions_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserQuestions"
    ADD CONSTRAINT "FK_UserQuestions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3795 (class 2606 OID 57964)
-- Name: UserQuizAnswers FK_UserQuizAnswers_UserQuizSubmissions_SubmissionId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserQuizAnswers"
    ADD CONSTRAINT "FK_UserQuizAnswers_UserQuizSubmissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES public."UserQuizSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3765 (class 2606 OID 57676)
-- Name: UserQuizSubmissions FK_UserQuizSubmissions_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserQuizSubmissions"
    ADD CONSTRAINT "FK_UserQuizSubmissions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3766 (class 2606 OID 57691)
-- Name: UserRoles FK_UserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "FK_UserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3767 (class 2606 OID 57706)
-- Name: Visits FK_Visits_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."Visits"
    ADD CONSTRAINT "FK_Visits_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3806 (class 2606 OID 58060)
-- Name: XQueueExerciseSubmissions FK_XQueueExerciseSubmissions_UserExerciseSubmissions_Submissio~; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."XQueueExerciseSubmissions"
    ADD CONSTRAINT "FK_XQueueExerciseSubmissions_UserExerciseSubmissions_Submissio~" FOREIGN KEY ("SubmissionId") REFERENCES public."UserExerciseSubmissions"("Id") ON DELETE CASCADE;


--
-- TOC entry 3807 (class 2606 OID 58065)
-- Name: XQueueExerciseSubmissions FK_XQueueExerciseSubmissions_XQueueWatchers_WatcherId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."XQueueExerciseSubmissions"
    ADD CONSTRAINT "FK_XQueueExerciseSubmissions_XQueueWatchers_WatcherId" FOREIGN KEY ("WatcherId") REFERENCES public."XQueueWatchers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3768 (class 2606 OID 57721)
-- Name: XQueueWatchers FK_XQueueWatchers_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: ulearn
--

ALTER TABLE ONLY public."XQueueWatchers"
    ADD CONSTRAINT "FK_XQueueWatchers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE RESTRICT;


-- Completed on 2021-03-17 17:27:31

--
-- PostgreSQL database dump complete
--

