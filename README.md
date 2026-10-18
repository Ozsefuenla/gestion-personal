# GestionPersonal

![CI](https://github.com/Ozsefuenla/gestion-personal/actions/workflows/ci.yml/badge.svg)

Aplicación web de gestión de personal, control horario, turnos y vacaciones para
peluquerías y clínicas estéticas en España. El software se vende por centro: cada
centro tiene sus trabajadores, un administrador y un usuario de tablet (kiosco).
Es un monolito modular en C# (.NET 10) con Clean Architecture: una única app
Blazor Server que consume la capa `Application` en proceso.

Roles: `SuperAdmin` (plataforma), `CenterAdmin` (centro), `Center` (kiosco de la
tablet) y `CenterWorker` (trabajador con PIN). Ver
[`docs/0012-modelo-roles.md`](docs/0012-modelo-roles.md).

## Stack tecnológico

| Capa | Tecnologías |
|------|-------------|
| Backend | ASP.NET Core 10 + Blazor Server (C#), monolito modular |
| Validación de entrada | FluentValidation |
| Manejo de errores | Result Pattern |
| Persistencia | PostgreSQL + Dapper + DbUp (SQL directo, sin ORM) |
| Autenticación | Cookie HTTP-Only para usuarios del centro y del SuperAdmin; cookie de worker para autoservicio; PIN de trabajador con BCrypt |
| Testing | xUnit (tests de servicios de aplicación y casos de uso) |
| Frontend | Blazor Web App (.NET 10, interactividad Server) + MudBlazor |

## Instalación y ejecución

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (o Docker Engine + Compose)

### Pasos

1. Clonar el repositorio.
2. Levantar PostgreSQL en local:
   ```
   docker compose up -d
   ```
   El contenedor `gestionpersonal-postgres` expone PostgreSQL en `localhost:5432`.
3. Compilar la solución:
   ```
   dotnet build src/GestionPersonal.slnx
   ```
4. Ejecutar la aplicación:
   ```
   dotnet run --project src/GestionPersonal.Web
   ```
5. Abrir la URL que muestra la consola: la landing está en `/` y el dashboard en `/app`.

Al arrancar, la aplicación aplica las migraciones (DbUp) y siembra el centro demo,
su `CenterAdmin`, el usuario de tablet (`centro`) y el `SuperAdmin`. En desarrollo
también crea los trabajadores y fichajes de ejemplo. Para reiniciar la base de
datos desde cero:

```
docker compose down -v
docker compose up -d
```

La cadena de conexión de desarrollo está en
`src/GestionPersonal.Web/appsettings.Development.json`. En producción se
configura con la variable de entorno `ConnectionStrings__Default`. Los detalles
están en [`docs/0011-persistencia-postgres.md`](docs/0011-persistencia-postgres.md).

### Tests

```
dotnet test src/GestionPersonal.Tests/GestionPersonal.Tests.csproj
dotnet test src/GestionPersonal.IntegrationTests/GestionPersonal.IntegrationTests.csproj
```

## Despliegue

El despliegue de producción se hace desde GitHub Actions con aprobación manual.
Las ramas `release/vX.Y.Z` generan un tag y un artefacto; `deploy.yml` publica
en MonsterASP y `rollback.yml` permite volver a un tag anterior. El runbook
completo está en [`docs/0010-deployment.md`](docs/0010-deployment.md).

## Estructura del proyecto

```
src/
├─ GestionPersonal.slnx
├─ GestionPersonal.Domain/          Entidades y enums (sin dependencias)
├─ GestionPersonal.Application/     Casos de uso, DTOs, validadores, Result
├─ GestionPersonal.Infrastructure/  Persistencia PostgreSQL (Dapper + DbUp) e implementaciones
├─ GestionPersonal.TestSupport/     Dobles in-memory para tests
├─ GestionPersonal.Web/             App Blazor Server + MudBlazor (monolito desplegable)
├─ GestionPersonal.Tests/           Tests de servicios de aplicación
└─ GestionPersonal.IntegrationTests/ Tests de integración del flujo web
docs/
├─ 0001-api-design.md
├─ 0001-api-openapi.yaml
├─ 0002-solution-structure.md
├─ 0003-frontend-ui.md
├─ 0004-frontend-ui-worker.md
├─ 0005-multi-tenant-model.md
├─ 0006-landing-page.md
├─ 0007-seguridad-owasp.md
├─ 0008-frontend-ui-admin.md
├─ 0009-gestion-trabajadores.md
├─ 0010-deployment.md
├─ 0011-persistencia-postgres.md
└─ 0012-modelo-roles.md
docker-compose.yml
AGENTS.md
CHANGELOG.md
```

## Funcionalidades principales

> Las rutas `/api/*` de la tabla son el contrato HTTP histórico
> (`docs/0001-api-design.md`); hoy las operaciones se ejecutan in-process vía
> `Application/Services`.

| Funcionalidad | Estado |
|---------------|--------|
| Listado de centros (`GET /api/centers`) | Implementado |
| Trabajadores de un centro (`GET /api/centers/{id}/workers`) | Implementado |
| Alta de trabajadores (`POST /api/workers`) | Implementado (con `centerId`; UI con PIN inicial que fuerza cambio) |
| Listado de trabajadores (`GET /api/workers`) | Implementado |
| Estado actual de todos los trabajadores (`GET /api/workers/status`) | Implementado (con resumen trabajado/pausado y timeline) |
| Inicio de fichaje (`POST /api/time-entries/start`) | Implementado |
| Pausa de fichaje (`POST /api/time-entries/pause`) | Implementado |
| Reanudar fichaje (`POST /api/time-entries/resume`) | Implementado |
| Fin de fichaje (`POST /api/time-entries/end`) | Implementado |
| Estado actual y resumen del día (`GET /api/time-entries/today`) | Implementado |
| Fichaje rápido por trabajador y PIN (`POST /api/time-entries/quick-clock`) | Implementado |
| Inicio de sesión por trabajador y PIN (`POST /api/workers/login`) | Implementado |
| Estado de un trabajador por id (`GET /api/workers/{id}/status`) | Implementado |
| Cambiar PIN (`POST /api/workers/{id}/change-pin`) | Implementado |
| Editar inicio del fichaje activo (`PATCH /api/time-entries/active/{workerId}`) | Implementado |
| Eliminar el último fichaje activo (`DELETE /api/time-entries/active/{workerId}`) | Implementado |
| Resumen mensual por día (`GET /api/time-entries/monthly`) | Implementado |
| Control Horario (panel mensual con barra segmentada vs horas diarias) | Implementado (con exportar PDF) |
| Marcajes de hoy de los trabajadores (admin) | Implementado (en directo) |
| Ver horarios trabajadores (admin, filtro mes/año) | Implementado |
| Exportar PDF del control horario (QuestPDF) | Implementado |
| Visualización de fichajes (propios / globales) | Pendiente |
| Solicitud de ausencias (`POST /api/vacations`) | Implementado (vacaciones/enfermedad/otro) |
| Listado de ausencias (`GET /api/vacations?workerId`) | Implementado (pendientes + aprobadas) |
| Eliminar ausencia (`DELETE /api/vacations/{id}`) | Implementado (borrado lógico) |
| Panel de Ausencias + solicitud (trabajador) | Implementado |
| Días de ausencia en Control Horario | Implementado (ausencias aprobadas) |
| Gestión de ausencias del admin (aprobar/denegar) | Implementado |
| Gestión de trabajadores del admin (renombrar, resetear PIN, dar de baja/reactivar) | Implementado |
| Aprobación de ausencias | Implementado |
| Login del centro (`POST /api/centers/login`) | Implementado (cookie HttpOnly) |
| Modelo de roles (`SuperAdmin`/`CenterAdmin`/`Center`/`CenterWorker`) | Implementado (autorización en servidor) |
| Cambio de contraseña forzado (popup al entrar) al resetear o dar de alta un usuario | Implementado |
| Área `/admin` (centros en tabs Activos/Desactivados, usuarios y reset de contraseña) | Implementado |
| Usuarios de tablet (`Center`) gestionados por el `CenterAdmin` | Implementado |
| Landing page (`/`) | Implementado |
| Dashboard del centro (`/app`) | Implementado (con layout profesional) |

## Usuario y contraseña de prueba

El acceso se hace desde la landing (`/`, botón "Acceso") o en `/login`. Usuarios
sembrados por defecto (configurables con `Persistence:*`):

- **SuperAdmin**: `superadmin` / `superadmin` → área `/admin`.
- **CenterAdmin** del centro demo `1` ("Peluquería Sol"): `carlos.ruiz` / `sol` →
  dashboard `/app`.
- **Center** (kiosco/tablet) del centro demo `1`: `centro` / `centro` → quick-clock
  en `/app`.

Las contraseñas se guardan hasheadas con BCrypt.

Los trabajadores se identifican por un **PIN de 4 dígitos** asociado a cada
trabajador (no hay email y el PIN no es único global).

PINs de prueba (centro `1`, sembrados en desarrollo): María García → `1111`,
Ana López → `2222`. Carlos Ruiz ya no es trabajador: su identidad es el
`CenterAdmin` (`carlos.ruiz`). Los identificadores los asigna PostgreSQL al
sembrar los datos.

## Pendientes

- CSP en el Web (diferido por la complejidad de Blazor + Tailwind CDN).
