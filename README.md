# GestionPersonal

![CI](https://github.com/Ozsefuenla/gestion-personal/actions/workflows/ci.yml/badge.svg)

Aplicación web de gestión de personal, control horario, turnos y vacaciones para
peluquerías y clínicas estéticas en España. El software se vende por centro: cada
centro tiene sus trabajadores y un usuario de acceso. Backend en C# (.NET 10)
siguiendo Clean Architecture; frontend en Blazor Server (en desarrollo).

## Stack tecnológico

| Capa | Tecnologías |
|------|-------------|
| Backend | ASP.NET Core 10 Web API (C#), Minimal APIs |
| Validación de entrada | FluentValidation |
| Documentación de API | Swagger / OpenAPI |
| Manejo de errores | Result Pattern + `ProblemDetails` (RFC 7807) |
| Persistencia | En memoria (actual). SQL directo sin ORM (pendiente) |
| Autenticación | Cookie HTTP-Only para el centro (BCrypt en contraseña). JWT + BCrypt del PIN pendientes |
| Testing | xUnit, `Microsoft.AspNetCore.Mvc.Testing` |
| Frontend | Blazor Web App (.NET 10, interactividad Server) + MudBlazor |

## Instalación y ejecución

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/)

### Pasos

1. Clonar el repositorio.
2. Compilar la solución:
   ```
   dotnet build src/GestionPersonal.slnx
   ```
3. Ejecutar la API:
   ```
   dotnet run --project src/GestionPersonal.Api
   ```
4. Abrir Swagger: el navegador abre por defecto `/swagger`.

### Tests

```
dotnet test src/GestionPersonal.Tests/GestionPersonal.Tests.csproj
```

## Estructura del proyecto

```
src/
├─ GestionPersonal.slnx
├─ GestionPersonal.Domain/          Entidades y enums (sin dependencias)
├─ GestionPersonal.Application/     Casos de uso, DTOs, validadores, Result
├─ GestionPersonal.Infrastructure/  Persistencia (en memoria) e implementaciones
├─ GestionPersonal.Api/             Minimal APIs, middleware, Swagger
├─ GestionPersonal.Web/             Frontend Blazor Server + MudBlazor
└─ GestionPersonal.Tests/           Tests de integración
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
└─ 0009-gestion-trabajadores.md
AGENTS.md
CHANGELOG.md
```

## Funcionalidades principales

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
| Landing page (`/`) | Implementado |
| Dashboard del centro (`/app`) | Implementado (con layout profesional) |

## Usuario y contraseña de prueba

El login del centro se hace desde la landing (`/`, botón "Acceso") o en `/login`,
con un `CenterUser` de prueba:

- Centro demo `1` ("Peluquería Sol"): usuario `admin`, contraseña `admin`.

La contraseña se guarda hasheada con BCrypt.

Los trabajadores se identifican por un **PIN de 4 dígitos** asociado a cada
trabajador (no hay email y el PIN no es único global).

PINs de prueba (en memoria, centro `1`): `1` María García → `1111`, `2` Ana López →
`2222`, `3` Carlos Ruiz → `3333`.

## Pendientes

- JWT para la API y hash BCrypt del PIN de trabajador (hoy el PIN se guarda en claro en `PinHash`).
- Persistencia SQL real (hoy en memoria, sin ORM).
- Scoping de endpoints admin (`GET /api/workers`, `/workers/status`) por centro.
