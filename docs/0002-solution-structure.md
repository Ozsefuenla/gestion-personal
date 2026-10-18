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
| `GestionPersonal.Infrastructure` | `Application` | Persistencia PostgreSQL (Dapper + DbUp) y DI |
| `GestionPersonal.TestSupport` | `Application`, `Domain`, `Infrastructure` | Dobles in-memory y registro de infraestructura para tests |
| `GestionPersonal.Web` | `Application`, `Infrastructure` | Frontend Blazor Server + MudBlazor, autenticación por cookie, implementación de `ICurrentCenter` |
| `GestionPersonal.Tests` | `Application`, `Infrastructure`, `TestSupport` | Tests de los servicios de aplicación y casos de uso (xUnit) |

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
- `Persistence/Postgres/`: `NpgsqlConnectionFactory`, `DatabaseInitializer` (DbUp + seed)
  y repositorios Dapper (`PostgresCenterRepository`, `PostgresCenterUserRepository`,
  `PostgresWorkerRepository`, `PostgresTimeEntryRepository`, `PostgresVacationRepository`).
- `Persistence/Migrations/`: scripts SQL versionados embebidos que aplica DbUp.
- `Security/`: `BcryptPasswordHasher`, `InMemoryLoginAttemptTracker`.
- `DependencyInjection.cs`: `AddInfrastructure(IConfiguration)` registra PostgreSQL
  y exige `ConnectionStrings:Default`.

### TestSupport
- `Fakes/`: repositorios in-memory usados como dobles de test (seed de centro,
  trabajadores, fichajes y ausencias).
- `TestInfrastructureServiceCollectionExtensions.cs`: `AddInMemoryTestInfrastructure()`
  registra los dobles para los tests unitarios y de integración.

### Web
- `Program.cs`: `AddRazorComponents` + interactividad Server, `AddMudServices`, `AddApplication()`,
  `AddInfrastructure(builder.Configuration)`, autenticación por cookie, rate limiting y
  `ICurrentCenter` → `CookieCurrentCenter`.
- `Security/CookieCurrentCenter.cs`: implementa `ICurrentCenter` leyendo la claim `centerId` de la cookie.
- `Endpoints/AuthEndpoints.cs`: login/logout del centro (cookie HttpOnly).
- `Services/`: `TimeControlPdfExporter`, formateo de horas.
- `Components/`: UI (páginas, layouts y diálogos). Consume los servicios de `Application` directamente,
  sin saltos HTTP ni `ApiClient`.
- Los `record` de respuesta de `Application` se reutilizan en la UI; no hay una capa `Models/` duplicada.

## Persistencia
- PostgreSQL con acceso SQL directo mediante Dapper, sin ORM.
- Migraciones versionadas con DbUp (scripts embebidos) aplicadas al arranque.
- Seed idempotente: centro demo y `admin` siempre; datos de demostración solo en
  desarrollo.
- Detalle de esquema, configuración y despliegue en `docs/0011-persistencia-postgres.md`.

## Fechas y horas
- Usar siempre `DateTimeOffset` en C# y UTC en base de datos.

## Paquetes NuGet principales
- `FluentValidation.DependencyInjectionExtensions` (Application)
- `BCrypt.Net-Next`, `Npgsql`, `Dapper` y `dbup-postgresql` (Infrastructure)
- `MudBlazor` y `QuestPDF` (Web)
- `xUnit`, `Microsoft.NET.Test.Sdk` y `Microsoft.Extensions.DependencyInjection` (Tests)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (TestSupport)
