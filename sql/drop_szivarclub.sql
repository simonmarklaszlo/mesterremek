DO $$
BEGIN
   PERFORM pg_terminate_backend(pid)
   FROM pg_stat_activity
   WHERE datname = 'szivarclub'
   AND pid <> pg_backend_pid();
END
$$;

DROP DATABASE IF EXISTS szivarclub;
CREATE DATABASE szivarclub;

\connect szivarclub

DROP SCHEMA IF EXISTS public CASCADE;
CREATE SCHEMA public;
