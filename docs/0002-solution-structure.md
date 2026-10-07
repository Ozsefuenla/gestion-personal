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
| `GestionPersonal.Web` | — | Frontend Blazor Server + MudBlazor (consume la API vía HTTP) |
| `GestionPersonal.Tests` | `Api` | Tests de integración (xUnit + `WebApplicationFactory`) |

## Capas

### Domain
- `Entities/Worker.cs`: alta con `Create` (nombre, `PinHash`, rol).
- `Entities/TimeEntry.cs`: máquina de estados `Start` → `Pause`/`Resume` → `End`,
  con historial de pausas (`PausePeriod`).
- `Entities/Vacation.cs`: `Request` → `Approve`/`Reject` con validación de fechas.
- `Enums/`: `Role`, `TimeEntryStatus`, `VacationStatus`.

### Application
- `Common/`: `Error`, `Result`, `Result<T>` (Result Pattern).
- `Abstractions/`: `IWorkerRepository`, `ITimeEntryRepository`.
- `Workers/`: alta y listado (`CreateWorkerCommand`, `ListWorkersQuery`, handlers, `WorkerResponse`).
- `TimeEntries/`: `start`, `pause`, `resume`, `end` y `today` (handlers, validadores, responses).
- `DependencyInjection.cs`: `AddApplication()` registra handlers y validadores.

### Infrastructure
- `Persistence/Repositories/InMemoryWorkerRepository.cs` e
  `InMemoryTimeEntryRepository.cs`: persistencia en memoria con datos de ejemplo.
- `DependencyInjection.cs`: `AddInfrastructure()` registra las implementaciones.

### Api
- `Program.cs`: DI, Swagger (solo en desarrollo), endpoint raíz.
- `Endpoints/WorkersEndpoints.cs` y `Endpoints/TimeEntriesEndpoints.cs`: endpoints delgados.
- `Extensions/ResultExtensions.cs`: mapea `Result` → `ProblemDetails`.
- `Middleware/ExceptionHandlingMiddleware.cs`: excepciones no controladas →
  `ProblemDetails` (RFC 7807).

### Web
- `Program.cs`: `AddRazorComponents` + interactividad Server + `AddMudServices`.
- `Components/`: `App.razor`, `Routes.razor`, layout y páginas.
- Consume la API vía `HttpClient` (pendiente de cablear).

## Persistencia
- Por ahora en memoria (sin ORM). La persistencia definitiva irá a una base de
  datos SQL mediante acceso directo (sin EF Core).

## Fechas y horas
- Usar siempre `DateTimeOffset` en C# y UTC en base de datos.

## Paquetes NuGet principales
- `FluentValidation.DependencyInjectionExtensions`
- `Swashbuckle.AspNetCore`
- `MudBlazor` (solo en `GestionPersonal.Web`)
- `Microsoft.AspNetCore.Mvc.Testing` (solo en `GestionPersonal.Tests`)
