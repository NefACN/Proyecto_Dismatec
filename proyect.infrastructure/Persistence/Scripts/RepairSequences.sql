-- Execute inside a transaction. Table locks prevent inserts during repair.
DO $repair$
DECLARE
    entry record;
    maximum_id bigint;
    sequence_last bigint;
    sequence_called boolean;
    next_id bigint;
BEGIN
    FOR entry IN
        SELECT DISTINCT ns.nspname AS schema_name, tbl.relname AS table_name
        FROM pg_class tbl
        JOIN pg_namespace ns ON ns.oid = tbl.relnamespace
        JOIN pg_attribute col ON col.attrelid = tbl.oid AND col.attnum > 0 AND NOT col.attisdropped
        WHERE ns.nspname = 'public' AND tbl.relkind = 'r'
          AND pg_get_serial_sequence(format('%I.%I', ns.nspname, tbl.relname), col.attname) IS NOT NULL
        ORDER BY ns.nspname, tbl.relname
    LOOP
        EXECUTE format('LOCK TABLE %I.%I IN ACCESS EXCLUSIVE MODE', entry.schema_name, entry.table_name);
    END LOOP;

    FOR entry IN
        SELECT ns.nspname AS schema_name, tbl.relname AS table_name, col.attname AS column_name,
               pg_get_serial_sequence(format('%I.%I', ns.nspname, tbl.relname), col.attname) AS sequence_name,
               seq.seqstart, seq.seqincrement
        FROM pg_class tbl
        JOIN pg_namespace ns ON ns.oid = tbl.relnamespace
        JOIN pg_attribute col ON col.attrelid = tbl.oid AND col.attnum > 0 AND NOT col.attisdropped
        JOIN pg_sequence seq ON seq.seqrelid = pg_get_serial_sequence(format('%I.%I', ns.nspname, tbl.relname), col.attname)::regclass
        WHERE ns.nspname = 'public' AND tbl.relkind = 'r'
        ORDER BY tbl.relname, col.attname
    LOOP
        IF entry.seqincrement <> 1 THEN
            RAISE EXCEPTION 'Unexpected sequence increment: %', entry.sequence_name;
        END IF;
        EXECUTE format('SELECT max(%I) FROM %I.%I', entry.column_name, entry.schema_name, entry.table_name) INTO maximum_id;
        EXECUTE format('SELECT last_value, is_called FROM %s', entry.sequence_name) INTO sequence_last, sequence_called;
        next_id := greatest(coalesce(maximum_id + 1, entry.seqstart),
                            sequence_last + CASE WHEN sequence_called THEN 1 ELSE 0 END);
        -- RESTART is transactional, unlike setval; it also blocks concurrent nextval.
        EXECUTE format('ALTER SEQUENCE %s RESTART WITH %s', entry.sequence_name, next_id);
        RAISE NOTICE '%: next ID = %', entry.table_name, next_id;
    END LOOP;
END
$repair$;
