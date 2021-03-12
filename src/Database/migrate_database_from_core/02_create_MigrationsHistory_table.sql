-- Table: public.__MigrationHistory

-- DROP TABLE public."__MigrationHistory";

CREATE TABLE public."__MigrationHistory"
(
    "MigrationId" character varying(150) COLLATE pg_catalog."default" NOT NULL,
    "ContextKey" character varying(300) COLLATE pg_catalog."default" NOT NULL,
    "Model" bytea NOT NULL,
    "ProductVersion" character varying(32) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT "PK_public.__MigrationHistory" PRIMARY KEY ("MigrationId", "ContextKey")
)

    TABLESPACE pg_default;

ALTER TABLE public."__MigrationHistory"
    OWNER to ulearn;