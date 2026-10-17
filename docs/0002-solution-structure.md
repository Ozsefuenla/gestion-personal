# Estructura de la solución

Solución `GestionPersonal` (`.slnx`) bajo `src/`, siguiendo Clean Architecture.
Las dependencias apuntan siempre hacia adentro; el núcleo (`Domain`) no depende
de ninguna librería de infraestructura.

La aplicación es un **monolito modular**: un único proyecto desplegable
(`GestionPersonal.Web`, Blazor Server) que consume la capa `Application` en
proceso. No existe un servicio HTTP de API separado.

## Proyectos

| Proyecto | Referencia a | Responsabilidad |
|----------|--------------|-----------------|
| `GestionPersonal.Domain` | — | Entidades (`Center`, `CenterUser`, `Worker`, `TimeEntry`, `Vacation`) y enums (`Role`, `TimeEntryStatus`, `VacationStatus`, `VacationType`) |
| `GestionPersonal.Application` | `Domain` | Casos de uso, DTOs (`record`), validadores, interfaces de repositorio, `Result`, servicios de aplicación (facades) |
| `GestionPersonal.Infrastructure` | `Application` | Implementaciones de persistencia (por ahora en memoria), DI |
| `GestionPersonal.Web` | `Application`, `Infrastructure` | Frontend Blazor Server + MudBlazor, autenticación por cookie, implementación de `ICurrentCenter` |
| `GestionPersonal.Tests` | `Application`, `Infrastructure` | Tests de los servicios de aplicación y casos de uso (xUnit) |

## Capas

### Domain
- `Entities/Center.cs`: centro (agregado raíz multi-tenant) con `Id` y `Name`.
- `Entities/CenterUser.cs`: usuario de acceso al centro (`Id`, `CenterId`, `Username`, `PasswordHash`).
- `Entities/Worker.cs`: alta con `Create` (nombre, `PinHash`, rol, `CenterId`).
- `Entities/TimeEntry.cs`: máquina de estados `Start` → `Pause`/`Resume` → `End`,
  con historial de pausas (`PausePeriod`).
- `Entities/Vacation.cs`: `Request` (con `Type`) → `Approve`/`Reject`/`Delete` (borrado lógico) y validación de fechas.
- `Enums/`: `Role`, `TimeEntryStatus`, `VacationStatus`, `VacationType`.

### Application
- `Common/`: `Error`, `Result`, `Result<T>` (Result Pattern) e `IRequestValidator`/`RequestValidator`
  (ejecuta FluentValidation y devuelve el primer error como `Error.Validation`).
- `Abstractions/`: `ICenterRepository`, `ICenterUserRepository`, `IWorkerRepository`, `ITimeEntryRepository`,
  `IVacationRepository`, `ICurrentCenter`, `IPasswordHasher`, `ILoginAttemptTracker`.
- `Services/`: facades de aplicación `ICentersService`, `IWorkersService`, `ITimeEntriesService` y
  `IVacationsService`. Validan la entrada y delegan en los handlers. **Es el punto de desacoplamiento**:
  una futura API HTTP solo tendría que exponer estas mismas operaciones.
- `Centers/`, `Workers/`, `TimeEntries/`, `Vacations/`: commands, queries, handlers, validadores y
  responses (`record`) por caso de uso.
- `DependencyInjection.cs`: `AddApplication()` registra handlers, validadores, `IRequestValidator` y los servicios.

### Infrastructure
- `Persistence/Repositories/`: repositorios en memoria con datos de ejemplo.
- `Security/`: `BcryptPasswordHasher`, `InMemoryLoginAttemptTracker`.
- `DependencyInjection.cs`: `AddInfrastructure()` registra las implementaciones.

### Web
- `Program.cs`: `AddRazorComponents` + interactividad Server, `AddMudServices`, `AddApplication()`,
  `AddInfrastructure()`, autenticación por cookie, rate limiting y `ICurrentCenter` → `CookieCurrentCenter`.
- `Security/CookieCurrentCenter.cs`: implementa `ICurrentCenter` leyendo la claim `centerId` de la cookie.
- `Endpoints/AuthEndpoints.cs`: login/logout del centro (cookie HttpOnly).
- `Services/`: `TimeControlPdfExporter`, formateo de horas.
- `Components/`: UI (páginas, layouts y diálogos). Consume los servicios de `Application` directamente,
  sin saltos HTTP ni `ApiClient`.
- Los `record` de respuesta de `Application` se reutilizan en la UI; no hay una capa `Models/` duplicada.

## Persistencia
- Por ahora en memoria (sin ORM). La persistencia definitiva irá a una base de
  datos SQL mediante acceso directo (sin EF Core).

## Fechas y horas
- Usar siempre `DateTimeOffset` en C# y UTC en base de datos.

## Paquetes NuGet principales
- `FluentValidation.DependencyInjectionExtensions` (Application)
- `BCrypt.Net-Next` (Infrastructure)
- `MudBlazor` y `QuestPDF` (Web)
- `xUnit`, `Microsoft.NET.Test.Sdk` y `Microsoft.Extensions.DependencyInjection` (Tests)
