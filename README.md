# GestionPersonal

Aplicación web de gestión de personal, control horario, turnos y vacaciones para
peluquerías y clínicas estéticas en España. Backend en C# (.NET 10) siguiendo
Clean Architecture; frontend en Next.js (pendiente).

## Stack tecnológico

| Capa | Tecnologías |
|------|-------------|
| Backend | ASP.NET Core 10 Web API (C#), Minimal APIs |
| Validación de entrada | FluentValidation |
| Documentación de API | Swagger / OpenAPI |
| Manejo de errores | Result Pattern + `ProblemDetails` (RFC 7807) |
| Persistencia | En memoria (actual). SQL directo sin ORM (pendiente) |
| Testing | xUnit, `Microsoft.AspNetCore.Mvc.Testing` |
| Frontend | Next.js 14+, Tailwind CSS, Shadcn UI, TanStack Query, Lucide React (pendiente) |

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
└─ GestionPersonal.Tests/           Tests de integración
docs/
├─ 0001-api-design.md
├─ 0001-api-openapi.yaml
└─ 0002-solution-structure.md
AGENTS.md
CHANGELOG.md
```

## Funcionalidades principales

| Funcionalidad | Estado |
|---------------|--------|
| Alta de trabajadores (`POST /api/workers`) | Implementado (sin control de rol todavía) |
| Listado de trabajadores (`GET /api/workers`) | Implementado |
| Registro de fichajes (inicio, pausa, fin) | Pendiente |
| Visualización de fichajes (propios / globales) | Pendiente |
| Solicitud de vacaciones | Pendiente |
| Aprobación de vacaciones | Pendiente |
| Autenticación / login | Pendiente |

## Usuario y contraseña de prueba

Pendiente — todavía no hay autenticación ni login implementados.
