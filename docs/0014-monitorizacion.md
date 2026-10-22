# 0014 — Monitorización (dashboard de SuperAdmin)

Documenta el módulo de monitorización de la plataforma: captura de eventos,
métricas y excepciones, su persistencia y el dashboard que el `SuperAdmin` ve
dentro del área `/admin`. La aplicación es un monolito modular en .NET 10
(Blazor Server + MudBlazor) con PostgreSQL (Dapper + DbUp); la telemetría se
almacena en la misma base de datos, sin infraestructura externa.

## Alcance

- El dashboard vive como una pestaña dentro de `/admin` (solo `SuperAdmin`).
- Logging vía `ILoggerProvider` propio (sin Serilog ni Application Insights).
- Alertas por SMTP (fase 2).
- Retención de 90 días.
- Volumen asumido bajo (decenas de centros); si crece, revisar particionado.

## Decisiones de diseño

| Tema | Decisión |
|------|----------|
| Ubicación | Tab "Monitorización" en `AdminDashboard.razor` (`/admin`) |
| Logging | `ILoggerProvider` propio → cola → escritor por lotes |
| Eventos de negocio | `IEventCollector` tipado, compartiendo la misma cola |
| Excepciones | Middleware `TelemetryExceptionMiddleware` → `app_exceptions` |
| Métricas de request | `RequestMetricsMiddleware` → `request_metrics` |
| Gráficas | `MudChart` (ya instalado; sin dependencias nuevas) |
| Health checks | Custom `PostgresHealthCheck` (`SELECT 1`) sobre `/health` |
| Retención | `RetentionJob` (purga diaria > 90 días) |
| Alertas | SMTP, umbrales configurables (fase 2) |
| SuperAdmin | Uno solo |

## Puntos críticos y eventos a registrar

| Punto crítico | Categoría | Eventos / métricas |
|---------------|-----------|--------------------|
| Login centro / worker | security | `login.succeeded`, `login.failed`, rate-limit `429` |
| Fichajes (start/pause/resume/end, quick-clock) | business | evento por transición con `workerId`/`centerId` |
| Aprobación de ausencias | business | `vacation.approved` / `vacation.rejected` (actor + sujeto) |
| Gestión de trabajadores (alta, rename, reset PIN, baja) | business | auditoría con actor, sujeto y campo |
| Alta/baja de centros y usuarios (`/admin`) | business | actor `SuperAdmin` + entidad afectada |
| Reset de contraseña | security | `password.reset` (actor + target) |
| Export PDF (QuestPDF) | system | inicio/fin, duración, errores |
| Migraciones DbUp + seed | system | resultado, versión aplicada |
| Errores no controlados | system | `app_exceptions` (tipo, stack, path) |
| Latencia / estado HTTP | system | `request_metrics` (path, status, `duration_ms`) |

## Modelo de datos

Migración `0004_telemetry.sql` (DbUp, embebida):

```sql
CREATE TABLE app_events (
    id BIGSERIAL PRIMARY KEY,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    category TEXT NOT NULL,             -- business | security | system | log
    event_type TEXT NOT NULL,           -- 'login.failed', 'worker.created', ...
    severity TEXT NOT NULL DEFAULT 'Info',
    actor_role TEXT NULL,
    actor_id INT NULL,
    center_id INT NULL,
    message TEXT NOT NULL,
    data JSONB NULL,
    correlation_id UUID NULL
);
CREATE INDEX ix_app_events_occurred_at   ON app_events (occurred_at DESC);
CREATE INDEX ix_app_events_type_occurred ON app_events (event_type, occurred_at DESC);
CREATE INDEX ix_app_events_severity      ON app_events (severity) WHERE severity IN ('Error','Critical');

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
CREATE INDEX ix_app_exceptions_occurred ON app_exceptions (occurred_at DESC);

CREATE TABLE request_metrics (
    id BIGSERIAL PRIMARY KEY,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    path TEXT NOT NULL,
    http_method TEXT NOT NULL,
    status_code INT NOT NULL,
    duration_ms DOUBLE PRECISION NOT NULL,
    center_id INT NULL
);
CREATE INDEX ix_request_metrics_occurred ON request_metrics (occurred_at DESC);
```

`metric_points` y `metric_daily` se añaden en la fase 1 (agregaciones
precalculadas y percentiles).

## Pipeline de escritura

```
ILogger<T> (middleware, handlers)  ─┐
                                    ├─► TelemetryCollector ─► Channel<T> ─► TelemetryWriter
IEventCollector.Emit(...) (dominio) ─┘  (Infrastructure, singleton)  (bounded)    (BackgroundService,
                                                                                    INSERT por lotes)
```

- `TelemetryLoggerProvider`/`TelemetryLogger` implementan `ILogger` y encolan
  cada entrada como `AppEvent(category="log")`; si hay `Exception`, va a
  `app_exceptions`.
- La escritura es **asíncrona y desacoplada**: no bloquea el request; si la cola
  está llena o la BD caída, se descarta y se incrementa un contador (nunca tira
  abajo la petición).
- `TelemetryWriter` hace `INSERT` por lotes y es idempotente ante reinicios del
  app pool (MonsterASP recicla procesos).

## KPIs (MVP, vista general)

| KPI | Fuente |
|-----|--------|
| Centros activos / total | `centers` |
| Trabajadores activos (plataforma) | `workers` |
| Fichajes hoy | `time_entries` (`started_at` hoy) |
| Ausencias pendientes | `vacations` (`status='Pending'`) |
| Errores 24 h | `app_exceptions` (ventana 24 h) |
| Tasa de error 24 h | `request_metrics` (`status >= 500`) |

Gráficas del MVP (MudChart): donut de severidad (eventos 24 h) y línea de
peticiones/errores por hora.

## Gestión de excepciones y logs

- **Severidades**: `Debug` (off en prod), `Info` (evento de negocio normal),
  `Warning` (login fallido, rate-limit, validación rechazada), `Error`
  (excepción manejada con impacto), `Critical` (excepción no controlada,
  migración fallida, BD caída).
- **Estructura del evento** — obligatorios: `occurred_at`, `category`,
  `event_type`, `severity`, `message`, `correlation_id`. Opcionales: `actor_role`,
  `actor_id`, `center_id`, `subject_type`, `subject_id`, `data` (JSONB).
- **Protección de datos**: nunca loguear `password_hash`, `pin_hash`, contraseñas
  en claro, tokens ni cookies. El escritor aplica una lista de exclusión.

## UI (tab "Monitorización" en `/admin`)

- `AdminDashboard.razor` gana una pestaña "Monitorización" que monta
  `AdminMonitoringPanel.razor`.
- Tarjetas KPI (`MudCard`) + `MudChart` (donut de severidad, línea de
  peticiones/errores) + listado de excepciones recientes.
- Refresco con `PeriodicTimer` cada 30 s + refresco manual (patrón existente de
  `TodayWorkersPanel`).
- Drill-down (fase 1): clic en tarjeta → listado filtrado; clic en excepción →
  detalle con stack.

## Health checks

- `PostgresHealthCheck` (custom, `SELECT 1`) sobre el `/health` existente.

## Retención

- `RetentionJob` (`BackgroundService`) purga diariamente filas con
  `occurred_at < now() - interval '90 days'` en lotes.

## Seguridad

- Página y consultas bajo `[Authorize(Roles = "SuperAdmin")]` y
  `AccessControl.RequireSuperAdmin` (defensa en profundidad).
- Redacción de datos sensibles antes de persistir y antes de mostrar.
- Ventanas de consulta acotadas (máx. 90 días) y paginación.

## Roadmap

- **Fase 0 — MVP**: migración, pipeline de escritura, `ILoggerProvider`,
  `TelemetryExceptionMiddleware`, `RequestMetricsMiddleware`, eventos en login/fichaje/
  alta-baja, `IMonitoringService`, tab "Monitorización", health check BD,
  `RetentionJob`.
- **Fase 1 — Histórico/drill-down**: `metric_points` + `metric_daily`,
  percentiles p50/p95/p99, listado de excepciones paginado, filtros por
  centro/fecha, barras de fichajes/día.
- **Fase 2 — Alertas SMTP**: `SmtpOptions` + `IEmailSender`, umbrales (tasa de
  error, backlog de ausencias, health check caído).

## Archivos afectados

- `Application/Abstractions/IEventCollector.cs`, `IMonitoringRepository.cs`
- `Application/Monitoring/*` (queries, handlers, responses)
- `Application/Services/IMonitoringService.cs`, `MonitoringService.cs`
- `Infrastructure/Persistence/Migrations/0004_telemetry.sql`
- `Infrastructure/Telemetry/*` (collector, writer, logger provider, retention)
- `Infrastructure/Persistence/Postgres/Repositories/PostgresMonitoringRepository.cs`
- `Web/Program.cs`, `Web/TelemetryExceptionMiddleware.cs`,
  `Web/RequestMetricsMiddleware.cs`, `Web/PostgresHealthCheck.cs`
- `Web/Components/AdminMonitoringPanel.razor`, `Components/Pages/AdminDashboard.razor`
- `TestSupport/Fakes/*`, `Tests/Services/MonitoringServiceTests.cs`
