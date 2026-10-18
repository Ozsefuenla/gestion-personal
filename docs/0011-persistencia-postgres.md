# 0011 — Persistencia con PostgreSQL

Este documento describe la persistencia definitiva: PostgreSQL con acceso SQL
directo mediante **Dapper** y migraciones versionadas con **DbUp**. No se usa un
ORM completo.

## Stack

| Pieza | Paquete | Papel |
|-------|---------|-------|
| Driver | `Npgsql` | Conexión y tipos de PostgreSQL |
| Mapeo | `Dapper` | Mapeo fila ↔ entidad sin change-tracking |
| Migraciones | `dbup-postgresql` | Scripts SQL versionados y registro de aplicados |

## Esquema

| Tabla | Campos principales |
|-------|--------------------|
| `centers` | `id` (identity), `name` |
| `center_users` | `id`, `center_id`, `username` (único), `password_hash` |
| `workers` | `id`, `center_id`, `full_name`, `pin_hash`, `role`, `active`, `must_change_pin`, `daily_hours` (`interval`), `created_at` |
| `time_entries` | `id` (`uuid`), `worker_id`, `status`, `started_at`, `ended_at` |
| `pause_periods` | `id`, `time_entry_id`, `paused_at`, `resumed_at` |
| `vacations` | `id` (`uuid`), `worker_id`, `start_date`, `end_date`, `type`, `status`, `requested_at`, `decided_at` |

- Enums como `TEXT` con `CHECK` (`Admin`/`Worker`, `InProgress`/`Paused`/`Finished`,
  `Pending`/`Approved`/`Rejected`/`Deleted`, `Vacations`/`SickLeave`/`Other`).
- Fechas y horas en `TIMESTAMPTZ` (UTC). `DateOnly` se guarda como `DATE`.
- `TimeSpan` (`DailyHours`) se guarda como `INTERVAL`.
- Índices por `center_id`, `worker_id` y rangos de fechas.

## Migraciones (DbUp)

- Los scripts viven en `src/GestionPersonal.Infrastructure/Persistence/Migrations/`
  y se incluyen como recursos embebidos (`*.sql`).
- DbUp los aplica al arrancar la aplicación, en orden alfabético, y registra los
  ejecutados en la tabla `schemaversions`.
- Cada script se ejecuta una sola vez. Los cambios de esquema nuevos se añaden
  como un script numerado adicional (`0002-...sql`, `0003-...sql`, etc.).

## Seed / bootstrap

`DatabaseInitializer` se ejecuta después de las migraciones:

- **Siempre** asegura el centro demo (`id = 1`, "Peluquería Sol") y el usuario
  `admin` (contraseña BCrypt tomada de `Persistence:AdminPassword`, por defecto
  `admin`). Es idempotente.
- **Solo en desarrollo** (`Persistence:SeedDemoData=true`) crea los trabajadores
  de demostración (María García `1111`, Ana López `2222`, Carlos Ruiz `3333`) y
  fichajes de ejemplo para septiembre/octubre de 2026.
- En producción no se siembran datos de demostración.

## Configuración

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=gestionpersonal;Username=gestionpersonal;Password=gestionpersonal"
  },
  "Persistence": {
    "SeedDemoData": true,
    "AdminPassword": "admin"
  }
}
```

- La persistencia es siempre PostgreSQL; no hay proveedor in-memory en producción.
- `Persistence:SeedDemoData`: datos demo. Solo en `appsettings.Development.json`.
- `Persistence:AdminPassword`: contraseña inicial del usuario `admin` (solo se usa
  si el usuario aún no existe). En producción se define con
  `Persistence__AdminPassword`.
- La cadena de conexión de desarrollo está en `appsettings.Development.json`.
- En producción se pasa por variable de entorno:
  `ConnectionStrings__Default=...`.

## Entorno local

```bash
docker compose up -d
dotnet run --project src/GestionPersonal.Web
```

- `docker-compose.yml` levanta PostgreSQL 17 con un volumen persistente.
- Al arrancar, la app aplica migraciones y seed automáticamente.
- Para reiniciar la base de datos desde cero: `docker compose down -v`.

## Producción

- MonsterASP.NET Free no incluye base de datos: hace falta un PostgreSQL
  externo (Neon, Supabase, RDS, DigitalOcean, VPS propio, etc.).
- Configurar `ConnectionStrings__Default` como secreto/variable de entorno del hosting.
- Dejar `Persistence:SeedDemoData` en `false`.
- La base de datos debe existir antes del primer arranque; DbUp aplica el esquema.
- Antes de cada release con cambios de esquema: backup y estrategia expand/contract.
- Un rollback de aplicación no revierte migraciones ni datos.

## Repositorios y persistencia explícita

- Las lecturas y escrituras son asíncronas (`AddAsync`, `UpdateAsync`,
  `RemoveAsync`).
- Al no haber change-tracking, cada handler que muta una entidad llama
  explícitamente a `UpdateAsync`.
- `AddAsync` de trabajadores inserta con `RETURNING id`; PostgreSQL asigna los
  identificadores.
- `TimeEntry` y sus `pause_periods` se guardan en una transacción: el update
  reemplaza las pausas del fichaje de forma atómica.
- Las entidades se reconstruyen con factories `Rehydrate(...)` para respetar
  setters privados y constructores privados del dominio.

## Zona horaria

- PostgreSQL `timestamptz` solo acepta `DateTimeOffset` con offset 0 (UTC).
- Todos los `DateTimeOffset` se normalizan con `PgConvert.ToUtc(...)` antes de
  escribir: seed, marcajes manuales, ediciones y rangos de consulta.
- Al leer, `timestamptz` se convierte a `DateTimeOffset` UTC y la UI lo formatea
  a `Europe/Madrid` con `MadridTime`.

## Tests

- Los dobles de test viven en `GestionPersonal.TestSupport` y se registran con
  `AddInMemoryTestInfrastructure()`; no forman parte de la infraestructura de
  producción.
- Los tests de integración reemplazan la infraestructura Postgres por esos fakes
  (`ConfigureTestServices`), por lo que el CI no necesita un servidor PostgreSQL.
- Una futura suite de integración contra PostgreSQL real puede usar Testcontainers.
