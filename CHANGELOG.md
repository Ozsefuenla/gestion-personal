# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
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
- Frontend: edición/borrado del último fichaje activo y botón "+ Añadir marcaje" (visual).
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
- El PIN del trabajador pasa a guardarse con BCrypt (antes en claro); se verifica en login,
  fichaje rápido y cambio de PIN, y se hashea en el alta.

### Removed
- Landing v2 (`/v2`, `LandingPageV2.razor` + `landing-v2.js`): eliminada en favor de la landing
  principal (`/`).
- Menú lateral "Panel" (`MudDrawer`) del layout `/app` (`AppLayout.razor`).

### Changed
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

### Fixed
- Login del centro: la tecla Enter ahora envía el formulario (`OnEnterPressed` en
  `CenterLoginForm`); antes solo respondía al clic en "Entrar".
- Layout `/app`: el nombre del centro ("Peluquería Sol") se muestra en blanco, heredando el
  `AppbarText` del tema en lugar de un `MudChip` con color explícito; antes texto e ícono
  salían en negro.
- Test `DeleteVacation_WhenApprovedAndAdmin_Succeeds`: corregido para reflejar que la
  solicitud de ausencia del admin queda `approved` automáticamente (ya no intenta aprobarla
  de nuevo, lo que lanzaba `Solo se puede decidir sobre una ausencia pendiente`).
