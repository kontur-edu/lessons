CREATE EXTENSION IF NOT EXISTS "uuid-ossp"; --генерация гуидов
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; --gin индекс с триграммами для поиска по регвыру

CREATE OR REPLACE FUNCTION immutable_concat_ws(text, VARIADIC text[])
RETURNS text AS 'text_concat_ws' LANGUAGE internal IMMUTABLE PARALLEL SAFE;