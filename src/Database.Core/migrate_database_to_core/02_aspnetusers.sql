BEGIN TRANSACTION;


-- AspNetRoles

ALTER TABLE public."AspNetRoles"
ADD "ConcurrencyStamp" text COLLATE pg_catalog."default";

ALTER TABLE public."AspNetRoles" 
ADD "NormalizedName" character varying(256) COLLATE pg_catalog."default";

-- AspNetUsers

ALTER TABLE public."AspNetUsers"
ADD "ConcurrencyStamp" text COLLATE pg_catalog."default";

ALTER TABLE public."AspNetUsers"
ADD "LockoutEnd" timestamp with time zone;

ALTER TABLE public."AspNetUsers"
ADD "NormalizedEmail" character varying(256) COLLATE pg_catalog."default";

ALTER TABLE public."AspNetUsers"
ADD "NormalizedUserName" character varying(256) COLLATE pg_catalog."default";


COMMIT TRANSACTION;