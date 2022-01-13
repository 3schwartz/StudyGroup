CREATE USER outbox WITH PASSWORD 'outbox';
CREATE DATABASE outbox OWNER outbox;
GRANT ALL PRIVILEGES ON DATABASE outbox TO outbox;
ALTER USER postgres WITH PASSWORD 'postgres';
GRANT ALL PRIVILEGES ON DATABASE postgres TO outbox;

\connect outbox;
CREATE SCHEMA outbox AUTHORIZATION outbox;
GRANT ALL ON ALL TABLES IN SCHEMA outbox TO outbox;
GRANT ALL ON ALL TABLES IN SCHEMA outbox TO postgres;
alter default privileges in schema outbox grant all on tables to outbox;
alter default privileges in schema outbox grant all on sequences to outbox;                                