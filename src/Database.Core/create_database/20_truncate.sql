CREATE OR REPLACE FUNCTION truncate_tables(username IN VARCHAR) RETURNS void AS $$
DECLARE
    statements CURSOR FOR
        SELECT tablename, schemaname FROM pg_tables
        WHERE tableowner = username AND (schemaname = 'antiplagiarism' or schemaname = 'public');
BEGIN
    FOR stmt IN statements LOOP
            EXECUTE 'TRUNCATE TABLE ' || quote_ident(stmt.schemaname) || '.' || quote_ident(stmt.tablename) || ' CASCADE;';
        END LOOP;
END;
$$ LANGUAGE plpgsql;
SELECT truncate_tables('ulearn');