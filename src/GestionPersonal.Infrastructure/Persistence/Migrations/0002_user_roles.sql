ALTER TABLE centers
    ADD COLUMN active BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE center_users
    ADD COLUMN role TEXT NOT NULL DEFAULT 'CenterAdmin';

ALTER TABLE center_users
    ADD COLUMN active BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE center_users
    ALTER COLUMN center_id DROP NOT NULL;

ALTER TABLE center_users
    ADD CONSTRAINT ck_center_users_role CHECK (role IN ('SuperAdmin', 'CenterAdmin', 'Center'));

ALTER TABLE workers
    DROP COLUMN role;
