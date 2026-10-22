CREATE TABLE app_events (
    id BIGSERIAL PRIMARY KEY,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    category TEXT NOT NULL,
    event_type TEXT NOT NULL,
    severity TEXT NOT NULL DEFAULT 'Info',
    actor_role TEXT NULL,
    actor_id INT NULL,
    center_id INT NULL,
    message TEXT NOT NULL,
    data JSONB NULL,
    correlation_id UUID NULL
);

CREATE INDEX ix_app_events_occurred_at ON app_events (occurred_at DESC);
CREATE INDEX ix_app_events_type_occurred ON app_events (event_type, occurred_at DESC);
CREATE INDEX ix_app_events_severity ON app_events (severity) WHERE severity IN ('Error', 'Critical');

CREATE TABLE app_exceptions (
    id BIGSERIAL PRIMARY KEY,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    exception_type TEXT NOT NULL,
    message TEXT NOT NULL,
    stack_trace TEXT NULL,
    request_path TEXT NULL,
    http_method TEXT NULL,
    status_code INT NULL,
    actor_role TEXT NULL,
    actor_id INT NULL,
    center_id INT NULL,
    correlation_id UUID NULL
);

CREATE INDEX ix_app_exceptions_occurred_at ON app_exceptions (occurred_at DESC);

CREATE TABLE request_metrics (
    id BIGSERIAL PRIMARY KEY,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    path TEXT NOT NULL,
    http_method TEXT NOT NULL,
    status_code INT NOT NULL,
    duration_ms DOUBLE PRECISION NOT NULL,
    center_id INT NULL
);

CREATE INDEX ix_request_metrics_occurred_at ON request_metrics (occurred_at DESC);
