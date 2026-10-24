# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.0] - 2026-10-24

### Added
- Documento del TFM publicado como contenido estático en `/docs/GestionPersonal.pdf`.

### Fixed
- Control horario: una jornada abierta y no cerrada (p. ej. del día 18) ya no se
  propaga a los días siguientes como jornadas "abiertas" de 24 h. El resumen
  mensual cierra la jornada abierta al final del día en que se inició, de modo
  que los días sin marcajes propios no heredan tiempo anterior.

## [1.4.0] - 2026-10-22

### Added
- Módulo de monitorización para el `SuperAdmin`: captura de eventos, métricas y
  excepciones persistida en PostgreSQL (migración `0004_telemetry.sql` con
  `app_events`, `app_exceptions` y `request_metrics`) y dashboard en una pestaña
  "Monitorización" dentro de `/admin`.
- `ILoggerProvider` propio (`TelemetryLoggerProvider`) y `IEventCollector`
  (`TelemetryCollector`) que encolan en un `Channel<T>`; un
  `TelemetryWriter` (`BackgroundService`) persiste por lotes sin bloquear el
  request (drop-safe si la cola se llena o la BD no responde).
- Captura de excepciones no controladas (middleware `TelemetryExceptionMiddleware`)
  y métricas de request (`RequestMetricsMiddleware`: path, método, status y duración).
- `IMonitoringService` + `Monitoring/*` (overview, listado de excepciones y
  serie de errores) con autorización `RequireSuperAdmin` en el servidor.
- Tab "Monitorización" en `/admin` con tarjetas KPI, gráficas `MudChart` (donut
  de severidad y línea de peticiones/errores) y refresco cada 30 s.
- Health check de PostgreSQL (custom, `SELECT 1`) sobre el `/health` existente.
- `RetentionJob`: purga diaria de telemetría con más de 90 días.
- Documento `docs/0014-monitorizacion.md`.

## [1.3.0] - 2026-10-18

### Added
- Actualización en tiempo real de los fichajes entre vistas del mismo centro:
  un bus de eventos en proceso (`ITimeEntryChangeNotifier`) notifica los cambios
  tras cada mutación de fichaje y los paneles suscritos se refrescan sin polling.
  El panel "Marcajes de hoy" del `CenterAdmin` y la vista personal del
  `CenterWorker` se actualizan al instante; se elimina el refresco cada 30 s.
- Tiempo trabajado en vivo: el donut del trabajador y el listado "Marcajes de
  hoy" suman minutos y segundos cada segundo, sin consultas adicionales al
  servidor. `WorkerStatusResponse` incluye `ComputedAt` y el cliente proyecta el
  tiempo con `LiveTime.Compute`.
- Panel "Marcajes de hoy" como grid de columnas fijas (Trabajador, Estado,
  Trabajado, Restante, Pausado) con `tabular-nums`, sin botón de refresco.
  "Restante" muestra `Xh Ym` y, si se supera la jornada, `+Xh Ym`.
- Donut del trabajador con el **% completado** sobre la jornada (tope 100%) y
  bloque fijo "Trabajando / Pausado / Restante" debajo.

### Changed
- Los botones del panel "Administración del centro" usan colores distintos
  (Primary, Warning, Info y Tertiary) para diferenciarse visualmente.
- Las barras de progreso de "Horarios de trabajadores" (admin y vista del
  trabajador) muestran solo el porcentaje trabajado en verde y el resto en gris;
  se elimina el tramo de pausa.

## [1.2.0] - 2026-10-18

### Added
- Modelo de roles de cuatro niveles: `SuperAdmin` (global), `CenterAdmin`
  (administra el centro), `Center` (usuario kiosco de la tablet) y
  `CenterWorker` (trabajador con PIN).
- Área `/admin` para el `SuperAdmin`: alta y baja de centros, alta y baja de sus
  administradores y consulta de usuarios por centro.
- Centros en `/admin` en dos tabs (**Activos** / **Desactivados**) y cada centro
  **colapsado por defecto**; los usuarios se cargan al expandir el centro.
- Reset de contraseña de usuarios del centro (`CenterAdmin` y `Center`) desde
  `/admin`, y de usuarios de tablet desde la gestión del `CenterAdmin`, con
  validación de permisos y confirmación de la nueva contraseña.
- El listado de usuarios de `/admin` se limita a `CenterAdmin` y `Center`
  (nunca `SuperAdmin`).
- Cambio forzado de contraseña: cualquier **reset** y el **alta** de
  `CenterAdmin`/`Center` marcan `MustChangePassword`; al entrar se muestra un
  **popup bloqueante** para cambiarla (sin página intermedia).
- `ChangePasswordDialog` (popup) y endpoint `POST /auth/change-password`, que
  valida la contraseña actual, guarda la nueva y re-emite la cookie sin la claim
  `mustChangePassword`. Si se accede directo a `/app` o `/admin` con el flag, el
  `AppLayout` abre el mismo popup.
- Migración `0003_center_user_must_change_password.sql`
  (`center_users.must_change_password`).
- Gestión de usuarios de tablet (`Center`) desde el panel del `CenterAdmin`.
- Autorización en el servidor (`ICurrentUser`, `AccessControl`, `WorkerScope`,
  `VacationScope`): el `CenterWorker` solo accede a sus propios datos y las
  acciones de administración exigen `CenterAdmin`.
- Doble cookie de sesión (`gp.center` y `gp.worker`) con un esquema de política
  que resuelve la identidad activa; el login por PIN emite una sesión de worker.
- Documento `docs/0012-modelo-roles.md`.

### Changed
- `CenterUser` incorpora `Role` y `Active`, y su `CenterId` pasa a ser nullable
  para el `SuperAdmin`; `Center` incorpora `Active` para la baja lógica.
- El login del centro devuelve el rol y la cookie incluye `ClaimTypes.Role`;
  el `NameIdentifier` pasa a ser el id del usuario (antes guardaba el `centerId`).
- El dashboard `/app` se adapta al rol: kiosco (`Center`), administración
  (`CenterAdmin`) o espacio personal del trabajador (`CenterWorker`). El
  `CenterAdmin` ya no ve el fichaje rápido ni el acceso de trabajador: su vista
  es exclusivamente de administración.
- El seed crea `superadmin`, `carlos.ruiz` (CenterAdmin) y `centro` (kiosco);
  los trabajadores demo pasan a ser María García y Ana López.

### Removed
- El rol `Admin` de `Worker` y la columna `workers.role` (migración
  `0002_user_roles.sql`). Carlos Ruiz deja de ser trabajador y pasa a ser el
  `CenterAdmin` del centro.
- La restricción de administración basada solo en el frontend (`Worker.Role`).
- El usuario `admin` del seed anterior.

### Fixed
- El **cambio forzado de PIN** de un trabajador nuevo (`MustChangePin`) se abría
  en `OnInitializedAsync`, también durante el prerender SSR, dejando colgada la
  página `/app` y sin completar el acceso. Ahora se abre en
  `OnAfterRenderAsync(firstRender)`. El diálogo forzado ya no permite cancelar.
- El popup de cambio de contraseña ahora guarda al pulsar **Enter** en cualquier
  campo (se envolvió en un `MudForm` con `OnEnterPressed`), sin tener que pulsar
  el botón "Guardar contraseña".
- El botón **"Gestionar ausencias"** del panel del `CenterAdmin` quedaba oculto
  cuando no había pendientes: se usaba `MudBadge.Hidden` (que no existe como
  parámetro y se renderizaba como atributo HTML `hidden`). Ahora se usa
  `MudBadge.Visible` y el botón está siempre disponible.
- Se cierra el acceso indebido a datos de otros trabajadores (BOLA/BFLA): las
  operaciones de autoservicio validan el `workerId` contra la sesión.

## [1.1.0] - 2026-10-18

### Added
- Persistencia PostgreSQL con Dapper (SQL directo, sin ORM) y migraciones
  versionadas con DbUp sobre scripts SQL embebidos.
- `docker-compose.yml` con PostgreSQL 17 y volumen persistente para desarrollo local.
- `DatabaseInitializer`: aplica migraciones y siembra de forma idempotente el centro
  demo y el usuario `admin`; los trabajadores y fichajes de ejemplo solo se siembran
  en desarrollo (`Persistence:SeedDemoData`).
- Documento `docs/0011-persistencia-postgres.md` (esquema, migraciones, seed y
  configuración local/producción).

### Fixed
- Normalización a UTC de los `DateTimeOffset` antes de escribir en PostgreSQL;
  Npgsql rechaza offsets distintos de 0 en columnas `timestamptz` (afectaba al
  seed de demo, marcajes manuales, ediciones y rangos de consulta).

### Changed
- Las escrituras de los repositorios son asíncronas (`AddAsync`, `UpdateAsync`,
  `RemoveAsync`) y los handlers persisten explícitamente las entidades que mutan.
- Los identificadores de trabajadores y centros los asigna PostgreSQL
  (`INSERT ... RETURNING id`); se elimina el cálculo manual `max(Id)+1`.
- `AddInfrastructure(IConfiguration)` registra la infraestructura PostgreSQL con
  repositorios scoped; los dobles in-memory viven en `GestionPersonal.TestSupport`
  (`AddInMemoryTestInfrastructure`).
- La persistencia es siempre PostgreSQL; la cadena se configura con
  `ConnectionStrings:Default` (variable de entorno en producción).
- README y AGENTS.md actualizados con los pasos de Docker y PostgreSQL.

### Removed
- Persistencia in-memory de producción (`InMemory*Repository`,
  `AddInMemoryInfrastructure`) y la clave `Persistence:Provider`; los dobles de
  test se movieron a `GestionPersonal.TestSupport`.

## [1.0.0] - 2026-10-18

### Added
- Workflows `release.yml`, `deploy.yml` y `rollback.yml`: release por rama
  `release/vX.Y.Z`, despliegue con aprobación manual del environment `production`
  y rollback manual por tag.
- Runbook de despliegue (`docs/0010-deployment.md`).
- Autenticación del centro con token antiforgery: endpoint `GET /auth/csrf` y envío de
  `RequestVerificationToken` en login/logout.
- Proyecto `GestionPersonal.IntegrationTests` con pruebas del flujo de autenticación
  (CSRF, login, logout y acceso protegido a `/app`).
- Health check `/health`.
- Integración continua con GitHub Actions (`.github/workflows/ci.yml`): compila y corre los
  tests en cada push/PR a `main`. Plantilla de PR en `.github/pull_request_template.md`.
- Documento de diseño de API (`docs/0001-api-design.md`).
- Especificación OpenAPI (`docs/0001-api-openapi.yaml`).
- Modelo multi-centro (`docs/0005-multi-tenant-model.md`).
- Entidades de dominio `Center` (centro/tenant) y `CenterUser` (usuario de acceso al centro).
- Campo `CenterId` en `Worker`; los trabajadores pertenecen a un centro.
- Repositorios `ICenterRepository` e `ICenterUserRepository` (in-memory) con seed de
  centro demo (`1`, "Peluquería Sol") y `CenterUser` (`admin`/`admin`).
- Endpoint `GET /api/centers` (listar centros).
- Endpoint `GET /api/centers/{id}/workers` (trabajadores del centro para el selector).
- Frontend: selector de trabajadores (`WorkerSelectorDialog`) previo al PIN en fichaje rápido y login.
- Archivo de reglas del asistente (`AGENTS.md`).
- Solución .NET `GestionPersonal` (Domain, Application, Infrastructure, Api)
  con entidades, Result Pattern, EF Core, repositorios y middleware de excepciones.
- Proyecto de tests `GestionPersonal.Tests` (xUnit + `Microsoft.AspNetCore.Mvc.Testing`).
- Endpoint `GET /api/workers` con persistencia en memoria (pendiente de mover a las capas definitivas).
- Endpoint `POST /api/workers` (alta de trabajador) con validación FluentValidation y tests.
- Endpoint `POST /api/time-entries/start` (inicio de jornada) con validación FluentValidation y tests.
- Endpoint `POST /api/time-entries/pause` (pausa de jornada) con validación FluentValidation y tests.
- Endpoint `POST /api/time-entries/end` (fin de jornada) con validación FluentValidation y tests.
- Endpoint `POST /api/time-entries/resume` (reanudar jornada) con validación FluentValidation y tests.
- Endpoint `GET /api/time-entries/today` (estado actual + resumen del día con línea de tiempo) en `Europe/Madrid`.
- Endpoint `GET /api/workers/status` (estado actual de todos los trabajadores con resumen trabajado/pausado y timeline).
- Frontend: panel de fichaje con cards de trabajadores, estado por color, resumen del día (trabajado/pausado), timeline y acciones habilitadas según estado.
- Documento de reglas de UI del frontend (`docs/0003-frontend-ui.md`).
- Endpoint `POST /api/time-entries/quick-clock` (fichaje rápido por PIN de 4 dígitos).
- Frontend: panel de fichaje rápido (Iniciar/Pausar/Finalizar) con modal de PIN (MudDialog).
- Frontend: botón "Login" (modal de PIN en modo login) y cards de trabajadores ocultas temporalmente.
- Endpoint `POST /api/workers/login` (inicio de sesión por PIN) y `GET /api/workers/{id}/status`.
- Frontend: card personal post-login (datos del día + acciones) con botones de perfil 2x2 (Cambiar PIN, Control Horario, Ausencias, Cerrar sesión).
- Endpoint `POST /api/workers/{id}/change-pin` (cambio de PIN verificando el actual).
- Frontend: donut circular de progreso (reemplaza a la mini progressbar) y diálogo de cambio de PIN.
- Endpoints `PATCH`/`DELETE /api/time-entries/active/{workerId}` (editar inicio / eliminar último fichaje activo).
- Frontend: edición/borrado del último fichaje activo y botón "+ Añadir marcaje".
- Endpoint `POST /api/time-entries/manual` (marcaje manual con tipo `start`/`pause`/`end`, fecha pasada o de hoy
  y hora de inicio/final según tipo) con validación FluentValidation y tests.
- Frontend: botón "Añadir marcaje" funcional en la card del trabajador (`AddMarkingDialog` con tipo,
  fecha con tope hoy, hora de inicio/final según tipo y validación de hora final > hora inicial).
- Campo `DailyHours` en `Worker` (horas diarias objetivo, seed inicial 8h) expuesto en `WorkerStatus`.
- Endpoint `GET /api/time-entries/monthly` (resumen mensual por día: trabajado/pausado por día + tramos + objetivo diario).
- Frontend: panel "Control Horario" (`TimeControlPanel`) con select de mes/año, listado diario con progressbar
  vs horas diarias, delta en minutos, días sin datos y botón "Exportar" (visual).
- Frontend: resumen mensual (Previstas/Trabajadas/Diferencia), barra segmentada (`SegmentedBar` verde/ámbar/gris
  sobre las 8h) y detalle del día por icono de ojo (`DayDetailDialog`) con tramos del día.
- Documento de reglas de UI del trabajador logueado (`docs/0004-frontend-ui-worker.md`).
- Datos de demo en memoria (`SeedData`): fichajes para los 3 workers seed en septiembre/octubre 2026
  (días laborables hasta hoy, con jornadas de más/menos/justas y pausas).
- Documento de landing page (`docs/0006-landing-page.md`): copy completo en español, diseño
  (paleta, tipografía, rejilla), propuestas de venta y detalle del componente de acceso.
- Login del centro end-to-end: `CenterLoginCommand`, `CenterLoginCommandValidator`,
  `CenterSessionResponse`, `CenterLoginCommandHandler` (verifica contraseña con BCrypt) y
  endpoint `POST /api/centers/login`.
- Abstracción `IPasswordHasher` + implementación `BcryptPasswordHasher` (`BCrypt.Net-Next`);
  el seed de `CenterUser` ahora guarda la contraseña hasheada con BCrypt.
- Autenticación del centro en el Web con cookie HttpOnly (`AddAuthentication(Cookie)`),
  endpoints `POST /auth/login` y `POST /auth/logout`, `AuthorizeRouteView` + `RedirectToLogin`.
- Landing page en `/` (`LandingPage.razor` + `LandingLayout.razor`) con el copy de `docs/0006`
  y estilos (paleta violeta/rosa, tipografía Poppins/Inter).
- Dashboard protegido en `/app` (`Dashboard.razor` + `AppLayout.razor`) con app bar y menú lateral.
- Componentes `CenterLoginForm` (formulario reutilizable), `CenterLoginDialog` (modal) y
  página `/login`.
- Tests `CenterLoginTests` (login de centro: credenciales válidas, contraseña incorrecta,
  usuario inexistente y campos vacíos).
- Documento de seguridad (`docs/0007-seguridad-owasp.md`) con mapeo OWASP Top 10 (Web 2021 +
  API 2023) y roadmap.
- Rate limiting en endpoints de login (Web `/auth/login`; API `/api/centers/login` y
  `/api/workers/login`), con rechazo `429`.
- Headers de seguridad en el Web (`X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`).
- Logging de intentos de login fallidos (centro y trabajador).
- Autenticación de la API con JWT (HS256, `Microsoft.AspNetCore.Authentication.JwtBearer`):
  `POST /api/centers/login` devuelve un token con claims `centerId`/`centerName`/`username`.
- Scoping por centro (BOLA/BFLA): `RequireAuthorization` en todos los endpoints y verificación
  de pertenencia al centro (`ICurrentCenter` + `IWorkerRepository.GetByIdAndCenterAsync` +
  `WorkerScope`).
- Frontend: `CenterTokenStore` y envío del JWT como `Authorization: Bearer` en `ApiClient`.
- Lockout del PIN del trabajador (`ILoginAttemptTracker`): 5 intentos fallidos bloquean al
  trabajador 15 minutos.
- Inventario de endpoints y propuesta de versionado (`/api/v1`) documentados en
  `docs/0007-seguridad-owasp.md`.
- Auditoría BOPLA y escaneo de dependencias (`dotnet list package --vulnerable --include-transitive`):
  sin hallazgos.
- Ausencias del trabajador (vacaciones / enfermedad / otro): entidad `Vacation` con `Type`,
  estados `pending`/`approved`/`deleted` y borrado lógico; `IVacationRepository` (in-memory).
- Endpoints `GET /api/vacations?workerId`, `POST /api/vacations` (queda `pending`) y
  `DELETE /api/vacations/{id}` (borrado lógico; aprobadas solo si el dueño es admin).
- Validación de solape: no se permite una ausencia que coincida con otra pendiente o aprobada
  del mismo trabajador (`409`).
- Frontend trabajador: botón "Ausencias" operativo, panel `AbsencesPanel` (listado con estado,
  tipo y borrado) y `RequestAbsenceDialog` (tipo + fecha inicio/fin con validación).
- Control Horario: los días con ausencia aprobada se muestran como "Ausencia - {tipo}" y no
  computan en el cálculo de horas.
- Gestión de ausencias del admin: `PATCH /api/vacations/{id}/approve` y
  `PATCH /api/vacations/{id}/reject` (deciden sobre una solicitud pendiente) y
  `GET /api/vacations/center` (ausencias no eliminadas del centro).
- Frontend admin: badge de pendientes en el botón "Ausencias", botón "Gestionar Ausencias"
  remarcado y diálogo `ManageAbsencesDialog` con tabs por trabajador (no-admin) y acciones
  Aprobar/Denegar por ausencia pendiente.
- Estado `rejected` (denegada): aparece en la lista propia del trabajador como "Denegado"
  (rojo, no eliminable).
- Panel "Marcajes de hoy" para el admin (`TodayWorkersPanel`): debajo del donut, lista los
  trabajadores no-admin del centro con horas/minutos trabajados hoy y diferencia vs objetivo,
  refrescado en directo cada 30 s (reutiliza `GET /api/workers/status`).
- En "Marcajes de hoy": cada fila muestra el **estado actual** (chip En curso / En pausa /
  Finalizado / Sin iniciar); si el trabajador tiene una **ausencia aprobada hoy**, la fila
  muestra solo "Ausencia - {Motivo}" en lugar de estado/trabajadas/diferencia.
- Botón de **refresco manual** en la cabecera de "Marcajes de hoy": recarga los datos al
  instante sin recargar la página (se deshabilita mientras carga).
- Botón "Ver horarios trabajadores" (solo admin) en el panel Control Horario y diálogo
  `WorkersTimeControlDialog` con tabs por trabajador y filtro Mes/Año a nivel de diálogo.
- Componente compartido `MonthlyTimeControlView` (resumen + listado diario) extraído de
  `TimeControlPanel` para reutilizarlo en el diálogo del admin.
- Exportar a PDF del control horario con **QuestPDF** (`TimeControlPdfExporter` + descarga
  vía JS interop `downloadFileFromStream`): botón "Exportar" del Control Horario propio y
  botón "Exportar PDF" del diálogo del admin (todos los trabajadores). El PDF incluye nombre,
  mes, año y el resumen completo del mes.
- Documento de reglas de UI del admin (`docs/0008-frontend-ui-admin.md`).
- Gestión de trabajadores del admin (`docs/0009-gestion-trabajadores.md`): renombrar,
  resetear PIN (con cambio forzado al entrar) y dar de baja / reactivar trabajadores.
- `Worker.MustChangePin` (bool) y métodos de dominio `Rename`, `ResetPin`, `Activate`
  y `Deactivate`; `ChangePin` apaga `MustChangePin` al cambiar el PIN.
- Endpoints `GET /api/workers/manage` (todos los trabajadores del centro, activos e
  inactivos), `PATCH /api/workers/{id}/name`, `POST /api/workers/{id}/reset-pin`,
  `PATCH /api/workers/{id}/activate` y `PATCH /api/workers/{id}/deactivate`.
- `WorkerResponse`/`WorkerStatusResponse` exponen `Active` y/o `MustChangePin`.
- Frontend admin: botón "Gestión de trabajadores" en el perfil y diálogo
  `ManageWorkersDialog` con tabs Activos/Inactivos, acciones por trabajador y
  confirmaciones de baja/reactivación.
- Cambio forzado de PIN tras reset: `Dashboard` abre `ChangePinDialog` bloqueante cuando
  `MustChangePin == true`.
- Alta de trabajador desde el panel de gestión: botón "Añadir nuevo trabajador"
  (`CreateWorkerDialog`) con nombre y PIN inicial (`role = "worker"`, `mustChangePin = true`),
  de modo que el trabajador debe cambiar el PIN en su primer acceso.
- `CreateWorkerCommand` y `Worker.Create` aceptan `MustChangePin` opcional (default `false`).

### Security
- Cookies de autenticación y antiforgery con `Secure` en producción.
- Claves de Data Protection persistidas en `App_Data/keys` en producción para no invalidar
  sesiones en cada recycle del app pool.
- El PIN del trabajador pasa a guardarse con BCrypt (antes en claro); se verifica en login,
  fichaje rápido y cambio de PIN, y se hashea en el alta.

### Removed
- Proyecto `GestionPersonal.Api` (Minimal APIs, Swagger, middleware de excepciones y
  `ResultExtensions`): la API se fusionó en `GestionPersonal.Web`.
- Autenticación JWT de la API y piezas asociadas (`IJwtTokenGenerator`, `JwtTokenGenerator`,
  `CenterLoginResponse`, `JwtBearer`, `Jwt:Key`). La sesión del centro se apoya solo en la cookie.
- `ApiClient`, `CenterTokenStore` y la claim `jwt` del Web; la UI llama a los servicios de
  aplicación en proceso.
- Capa `GestionPersonal.Web/Models` (DTOs duplicados); se reutilizan los `record` de `Application`.
- Landing v2 (`/v2`, `LandingPageV2.razor` + `landing-v2.js`): eliminada en favor de la landing
  principal (`/`).
- Menú lateral "Panel" (`MudDrawer`) del layout `/app` (`AppLayout.razor`).

### Changed
- Login del centro: la cookie de autenticación pasa a ser de sesión (sin `Expires`) para evitar
  que navegadores con reloj o `Date` de servidor desfasado la descarten.
- Fusión de `GestionPersonal.Api` en `GestionPersonal.Web` (monolito modular). Nuevos servicios
  de aplicación `ICentersService`, `IWorkersService`, `ITimeEntriesService` e `IVacationsService`
  en `Application/Services`; la validación FluentValidation se movió del endpoint a `IRequestValidator`.
- `ICurrentCenter` pasa a implementarse en el Web con `CookieCurrentCenter` (claim `centerId`
  de la cookie).
- Tests migrados de integración HTTP (`WebApplicationFactory`) a tests de servicios de aplicación
  (`GestionPersonal.Tests/Services`), con repositorios en memoria y `ICurrentCenter` falso.
- Documentación actualizada (`0002`, `0003`, `0005`, `0006`, `0007`, `0008`, `0009`, `README`).
  `docs/0001` se conserva como contrato histórico para una futura re-extracción.
- Framework backend actualizado a .NET 10 / ASP.NET Core 10.
- Eliminado EF Core; la persistencia es en memoria hasta implementar acceso SQL directo.
- Fichajes: historial de pausas (cada pausa guarda `pausedAt`/`resumedAt`); `end` ahora es válido estando en pausa.
- Trabajadores: eliminado el campo `email`; añadido `pin` de 4 dígitos (guardado en `PinHash`, en claro temporalmente).
- `/api/time-entries/today`: `currentStatus` distingue `finished` (fichajes cerrados hoy) de `idle` (sin iniciar).
- Acción "Iniciar" unificada con "Reanudar" (iniciar jornada o reanudar según estado) en card y fichaje rápido.
- `docs/0003-frontend-ui.md` se reestructura como base común; el contenido del trabajador logueado pasa a `docs/0004-frontend-ui-worker.md`.
- `Worker.Id` y `WorkerId` migrados de `Guid` a `int` (autoincremental; seed `1`, `2`, `3`); `TimeEntry.Id` y `Vacation.Id` siguen en `Guid`.
- `POST /api/workers/login`: ahora recibe `{ workerId, pin }` (antes `{ pin }`); el PIN se valida contra el trabajador seleccionado.
- `POST /api/time-entries/quick-clock`: ahora recibe `{ workerId, pin, action }` (antes `{ pin, action }`).
- `POST /api/workers`: añade `centerId` obligatorio para asociar el trabajador a un centro.
- `IWorkerRepository`: eliminado `GetByPinAsync` (PIN global); añadido `GetByCenterAsync`.
- Frontend: el flujo de PIN queda precedido por un selector de trabajadores del centro.
- Frontend: `/` pasa a ser la landing page; el dashboard (fichaje/estado del equipo) se mueve
  a `/app`, accesible tras el login del centro.
- `MudTheme` con paleta violeta/rosa (primario `#5B3E8E`) y tipografía Poppins/Inter.
- `/app`: el dashboard del centro ahora es el kiosco de fichaje (Iniciar/Pausar/Finalizar +
  botón "Login" para el acceso personal del trabajador por PIN), en lugar de la vista
  "Estado del equipo".
- `VacationStatus`: `rejected` se sustituye por `deleted` (borrado lógico); `Vacation` gana
  `type` (`vacations`/`sick-leave`/`other`) y `Delete`. `GET /api/vacations` pasa a listar por
  `workerId` (pendientes y aprobadas, descendente) en lugar de filtrar por estado.
- `DailySummaryResponse`/`DailySummary` añaden `isAbsence` y `absenceType` (días cubiertos por
  ausencia aprobada).
- `VacationStatus` añade `rejected` (denegada por el admin); `VacationResponse` incluye
  `workerId`; `GET /api/vacations?workerId` lista también las denegadas.
- `POST /api/vacations`: si la solicita un trabajador con rol `admin`, la ausencia queda
  `approved` automáticamente (no `pending`).
- Paneles de "Ausencias" y "Control Horario" ahora son mutuamente excluyentes: al abrir
  uno se cierra el otro.
- Listados de trabajadores (`GET /api/workers`, `GET /api/centers/{id}/workers` y
  `GET /api/workers/status`) filtran solo trabajadores **activos**; los inactivos se
  consultan con `GET /api/workers/manage`.
- `POST /api/workers/login` y `POST /api/time-entries/quick-clock` rechazan a los
  trabajadores inactivos (`403`).
- `DailySummaryResponse`/`DailySummary` añaden `hasOpenEntry` (indica si el día tiene una
  jornada abierta).
- Control Horario: las barras segmentadas del listado mensual ahora tienen ancho uniforme
  (columnas de texto con ancho fijo + barra con `flex: 1 1 0%`).

### Fixed
- Login del centro: el formulario obtiene el token antiforgery y muestra el estado HTTP real
  cuando la petición falla, en lugar de mostrar siempre "Usuario o contraseña incorrectos".
- Detalle del día (`DayDetailDialog`): ahora muestra la hora de salida — "Finalizado HH:mm"
  para jornadas terminadas y "En curso" para la jornada abierta (antes solo se veía el inicio).
- Login del centro: la tecla Enter ahora envía el formulario (`OnEnterPressed` en
  `CenterLoginForm`); antes solo respondía al clic en "Entrar".
- Layout `/app`: el nombre del centro ("Peluquería Sol") se muestra en blanco, heredando el
  `AppbarText` del tema en lugar de un `MudChip` con color explícito; antes texto e ícono
  salían en negro.
- Test `DeleteVacation_WhenApprovedAndAdmin_Succeeds`: corregido para reflejar que la
  solicitud de ausencia del admin queda `approved` automáticamente (ya no intenta aprobarla
  de nuevo, lo que lanzaba `Solo se puede decidir sobre una ausencia pendiente`).
