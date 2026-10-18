CREATE TABLE centers (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

CREATE TABLE center_users (
    id SERIAL PRIMARY KEY,
    center_id INTEGER NOT NULL REFERENCES centers (id) ON DELETE CASCADE,
    username TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL
);

CREATE TABLE workers (
    id SERIAL PRIMARY KEY,
    center_id INTEGER NOT NULL REFERENCES centers (id) ON DELETE CASCADE,
    full_name TEXT NOT NULL,
    pin_hash TEXT NOT NULL,
    role TEXT NOT NULL CHECK (role IN ('Admin', 'Worker')),
    active BOOLEAN NOT NULL DEFAULT TRUE,
    must_change_pin BOOLEAN NOT NULL DEFAULT FALSE,
    daily_hours INTERVAL NOT NULL DEFAULT '08:00:00',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_workers_center_id ON workers (center_id);

CREATE TABLE time_entries (
    id UUID PRIMARY KEY,
    worker_id INTEGER NOT NULL REFERENCES workers (id) ON DELETE CASCADE,
    status TEXT NOT NULL CHECK (status IN ('InProgress', 'Paused', 'Finished')),
    started_at TIMESTAMPTZ NOT NULL,
    ended_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_time_entries_worker_started_at ON time_entries (worker_id, started_at);

CREATE TABLE pause_periods (
    id SERIAL PRIMARY KEY,
    time_entry_id UUID NOT NULL REFERENCES time_entries (id) ON DELETE CASCADE,
    paused_at TIMESTAMPTZ NOT NULL,
    resumed_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_pause_periods_time_entry_id ON pause_periods (time_entry_id);

CREATE TABLE vacations (
    id UUID PRIMARY KEY,
    worker_id INTEGER NOT NULL REFERENCES workers (id) ON DELETE CASCADE,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    type TEXT NOT NULL CHECK (type IN ('Vacations', 'SickLeave', 'Other')),
    status TEXT NOT NULL CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Deleted')),
    requested_at TIMESTAMPTZ NOT NULL,
    decided_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_vacations_worker_id ON vacations (worker_id);
