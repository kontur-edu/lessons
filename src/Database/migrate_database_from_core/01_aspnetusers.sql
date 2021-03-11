BEGIN TRANSACTION;

ALTER TABLE "AspNetUsers"
    ADD COLUMN "LockoutEndDateUtc" timestamp with time zone;

ALTER TABLE "Notifications"
    ADD COLUMN "GroupId1" integer;

ALTER TABLE public."Notifications"
    ADD CONSTRAINT "FK_Notifications_Groups_GroupId4" FOREIGN KEY ("GroupId1")
        REFERENCES public."Groups" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE;

ALTER TABLE "Notifications"
    ADD COLUMN "Text1" text COLLATE public.case_insensitive;

COMMIT TRANSACTION;