ALTER TABLE public."AspNetUsers"
    DROP COLUMN "Names";

ALTER TABLE public."AspNetUsers"
    ADD "Names" text GENERATED ALWAYS AS (public.immutable_concat_ws(' '::text, VARIADIC ARRAY[NULLIF(("UserName")::text, ''::text), NULLIF("FirstName", ''::text), NULLIF("LastName", ''::text), NULLIF("FirstName", ''::text)])) STORED;

ALTER TABLE public."Likes"
    RENAME COLUMN "ID" TO "Id";
