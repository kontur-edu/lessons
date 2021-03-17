ALTER TABLE public."AspNetUsers"
    DROP COLUMN "Names";

ALTER TABLE public."AspNetUsers"
    ADD "Names" text COLLATE public.case_insensitive;

ALTER TABLE public."Likes"
    RENAME COLUMN "Id" TO "ID";