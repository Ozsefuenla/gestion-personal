ALTER TABLE center_users
    ADD COLUMN must_change_password BOOLEAN NOT NULL DEFAULT FALSE;
