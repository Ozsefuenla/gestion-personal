# GestionPersonal

Aplicación web de gestión de personal, control horario, turnos y vacaciones para
peluquerías y clínicas estéticas en España. Backend en C# (.NET 10) siguiendo
Clean Architecture; frontend en Blazor Server (en desarrollo).

## Stack tecnológico

| Capa | Tecnologías |
|------|-------------|
| Backend | ASP.NET Core 10 Web API (C#), Minimal APIs |
| Validación de entrada | FluentValidation |
| Documentación de API | Swagger / OpenAPI |
| Manejo de errores | Result Pattern + `ProblemDetails` (RFC 7807) |
| Persistencia | En memoria (actual). SQL directo sin ORM (pendiente) |
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
└─ 0004-frontend-ui-worker.md
AGENTS.md
CHANGELOG.md
```

## Funcionalidades principales

| Funcionalidad | Estado |
|---------------|--------|
| Alta de trabajadores (`POST /api/workers`) | Implementado (sin control de rol todavía) |
| Listado de trabajadores (`GET /api/workers`) | Implementado |
| Estado actual de todos los trabajadores (`GET /api/workers/status`) | Implementado (con resumen trabajado/pausado y timeline) |
| Inicio de fichaje (`POST /api/time-entries/start`) | Implementado |
| Pausa de fichaje (`POST /api/time-entries/pause`) | Implementado |
| Reanudar fichaje (`POST /api/time-entries/resume`) | Implementado |
| Fin de fichaje (`POST /api/time-entries/end`) | Implementado |
| Estado actual y resumen del día (`GET /api/time-entries/today`) | Implementado |
| Fichaje rápido por PIN (`POST /api/time-entries/quick-clock`) | Implementado |
| Inicio de sesión por PIN (`POST /api/workers/login`) | Implementado |
| Estado de un trabajador por id (`GET /api/workers/{id}/status`) | Implementado |
| Cambiar PIN (`POST /api/workers/{id}/change-pin`) | Implementado |
| Editar inicio del fichaje activo (`PATCH /api/time-entries/active/{workerId}`) | Implementado |
| Eliminar el último fichaje activo (`DELETE /api/time-entries/active/{workerId}`) | Implementado |
| Resumen mensual por día (`GET /api/time-entries/monthly`) | Implementado |
| Control Horario (panel mensual con progressbar vs horas diarias) | Implementado (sin exportar ni detalle) |
| Visualización de fichajes (propios / globales) | Pendiente |
| Solicitud de vacaciones | Pendiente |
| Aprobación de vacaciones | Pendiente |
| Autenticación / login (JWT) | Pendiente |

## Usuario y contraseña de prueba

Pendiente — todavía no hay autenticación ni login implementados. Los trabajadores
se identifican por un **PIN de 4 dígitos** (no hay email).

PINs de prueba (en memoria): `1111` (María García), `2222` (Ana López), `3333` (Carlos Ruiz).

## Pendientes

- Hashear el PIN con BCrypt (hoy se guarda en claro en `PinHash`).
- Persistencia SQL real (hoy en memoria, sin ORM).
- Seguridad (JWT + cookie).
- Botón "Ausencias" (solo visual por ahora).
- Botones "Exportar" y detalle (ojo) del panel de Control Horario (pendientes).
- Botón "+ Añadir marcaje" (solo visual por ahora).
