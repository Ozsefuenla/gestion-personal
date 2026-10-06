# Estructura de la solución

Solución `GestionPersonal` (`.slnx`) bajo `src/`, siguiendo Clean Architecture.
Las dependencias apuntan siempre hacia adentro; el núcleo (`Domain`) no depende
de ninguna librería de infraestructura.

## Proyectos

| Proyecto | Referencia a | Responsabilidad |
|----------|--------------|-----------------|
| `GestionPersonal.Domain` | — | Entidades (`Worker`, `TimeEntry`, `Vacation`) y enums (`Role`, `TimeEntryStatus`, `VacationStatus`) |
| `GestionPersonal.Application` | `Domain` | Casos de uso, DTOs (`record`), validadores, interfaces de repositorio, `Result` |
| `GestionPersonal.Infrastructure` | `Application` | Implementaciones de persistencia (por ahora en memoria), DI |
| `GestionPersonal.Api` | `Application`, `Infrastructure` | Minimal APIs, middleware de excepciones, `Program.cs` |

## Capas

### Domain
- `Entities/Worker.cs`: alta con `Create`, normaliza `Email` a minúsculas.
- `Entities/TimeEntry.cs`: máquina de estados `Start` → `Pause`/`Resume` → `End`.
- `Entities/Vacation.cs`: `Request` → `Approve`/`Reject` con validación de fechas.
- `Enums/`: `Role`, `TimeEntryStatus`, `VacationStatus`.

### Application
- `Common/`: `Error`, `Result`, `Result<T>` (Result Pattern).
- `Abstractions/`: `IWorkerRepository`.
- `Workers/`: feature de listado (`ListWorkersQuery`, handler, `WorkerResponse`).
- `DependencyInjection.cs`: `AddApplication()` registra los handlers.

### Infrastructure
- `Persistence/Repositories/InMemoryWorkerRepository.cs`: implementación en
  memoria de `IWorkerRepository` con datos de ejemplo.
- `DependencyInjection.cs`: `AddInfrastructure()` registra las implementaciones.

### Api
- `Program.cs`: DI, Swagger (solo en desarrollo), endpoint raíz.
- `Endpoints/WorkersEndpoints.cs`: endpoint delgado de `workers`.
- `Extensions/ResultExtensions.cs`: mapea `Result` → `ProblemDetails`.
- `Middleware/ExceptionHandlingMiddleware.cs`: excepciones no controladas →
  `ProblemDetails` (RFC 7807).

## Persistencia
- Por ahora en memoria (sin ORM). La persistencia definitiva irá a una base de
  datos SQL mediante acceso directo (sin EF Core).

## Fechas y horas
- Usar siempre `DateTimeOffset` en C# y UTC en base de datos.

## Paquetes NuGet principales
- `FluentValidation.DependencyInjectionExtensions`
- `Swashbuckle.AspNetCore`
- `Microsoft.AspNetCore.Mvc.Testing` (solo en `GestionPersonal.Tests`)
