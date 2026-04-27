-- Bootstrap schema for the IncTrak control-plane PostgreSQL database.
-- Apply this to an empty PostgreSQL database dedicated to tenant, identity,
-- domain, and provisioning metadata.

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE OR REPLACE FUNCTION cp_updated_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated = now();
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TABLE cp_schema_version(
    schema_version_pk int NOT NULL PRIMARY KEY,
    version_name varchar(100) NOT NULL UNIQUE,
    applied timestamp with time zone NOT NULL DEFAULT now()
);

INSERT INTO cp_schema_version (schema_version_pk, version_name)
VALUES (1, 'control-plane-bootstrap-v1');

CREATE TABLE cp_users(
    user_pk uuid NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    supabase_user_id uuid NOT NULL UNIQUE,
    email_address varchar(256) NOT NULL,
    display_name varchar(256) NULL,
    is_platform_operator boolean NOT NULL DEFAULT false,
    last_login timestamp with time zone NULL,
    created timestamp with time zone NOT NULL DEFAULT now(),
    updated timestamp with time zone NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX uc_cp_users_email ON cp_users (lower(email_address));
CREATE TRIGGER updated_cp_users BEFORE UPDATE ON cp_users FOR EACH ROW EXECUTE PROCEDURE cp_updated_column();

CREATE TABLE cp_tenants(
    tenant_pk uuid NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    company_name varchar(256) NOT NULL,
    tenant_slug varchar(63) NOT NULL UNIQUE,
    status varchar(30) NOT NULL,
    primary_domain varchar(255) NOT NULL,
    tenant_db_name varchar(128) NULL,
    template_version varchar(100) NULL,
    activated timestamp with time zone NULL,
    created timestamp with time zone NOT NULL DEFAULT now(),
    updated timestamp with time zone NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX uc_cp_tenants_primary_domain ON cp_tenants (lower(primary_domain));
CREATE TRIGGER updated_cp_tenants BEFORE UPDATE ON cp_tenants FOR EACH ROW EXECUTE PROCEDURE cp_updated_column();

CREATE TABLE cp_tenant_domains(
    tenant_domain_pk uuid NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    tenant_fk uuid NOT NULL REFERENCES cp_tenants ON DELETE CASCADE,
    host_name varchar(255) NOT NULL UNIQUE,
    is_primary boolean NOT NULL DEFAULT false,
    verification_status varchar(30) NOT NULL,
    created timestamp with time zone NOT NULL DEFAULT now(),
    updated timestamp with time zone NOT NULL DEFAULT now()
);
CREATE TRIGGER updated_cp_tenant_domains BEFORE UPDATE ON cp_tenant_domains FOR EACH ROW EXECUTE PROCEDURE cp_updated_column();

CREATE TABLE cp_memberships(
    membership_pk uuid NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    tenant_fk uuid NOT NULL REFERENCES cp_tenants ON DELETE CASCADE,
    user_fk uuid NOT NULL REFERENCES cp_users ON DELETE CASCADE,
    role_code varchar(50) NOT NULL,
    status varchar(30) NOT NULL,
    participant_external_key varchar(128) NULL,
    invited_by_user_fk uuid NULL REFERENCES cp_users,
    accepted timestamp with time zone NULL,
    created timestamp with time zone NOT NULL DEFAULT now(),
    updated timestamp with time zone NOT NULL DEFAULT now(),
    UNIQUE (tenant_fk, user_fk)
);
CREATE TRIGGER updated_cp_memberships BEFORE UPDATE ON cp_memberships FOR EACH ROW EXECUTE PROCEDURE cp_updated_column();

CREATE TABLE cp_provisioning_jobs(
    provisioning_job_pk uuid NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    tenant_fk uuid NOT NULL REFERENCES cp_tenants ON DELETE CASCADE,
    job_type varchar(50) NOT NULL,
    status varchar(30) NOT NULL,
    attempt_count int NOT NULL DEFAULT 0,
    requested_by_user_fk uuid NULL REFERENCES cp_users,
    payload_json text NULL,
    result_json text NULL,
    started timestamp with time zone NULL,
    finished timestamp with time zone NULL,
    created timestamp with time zone NOT NULL DEFAULT now(),
    updated timestamp with time zone NOT NULL DEFAULT now()
);
CREATE TRIGGER updated_cp_provisioning_jobs BEFORE UPDATE ON cp_provisioning_jobs FOR EACH ROW EXECUTE PROCEDURE cp_updated_column();

CREATE TABLE cp_reserved_slugs(
    reserved_slug_pk uuid NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    slug_value varchar(63) NOT NULL UNIQUE,
    reason varchar(256) NOT NULL,
    created timestamp with time zone NOT NULL DEFAULT now()
);

INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('www', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('api', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('shared', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('signup', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('vesting', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('docs', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('blog', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('admin', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('app', 'reserved platform hostname');
INSERT INTO cp_reserved_slugs (slug_value, reason) VALUES ('support', 'reserved platform hostname');
