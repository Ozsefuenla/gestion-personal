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
- Landing v2 (`/v2`, `LandingPageV2.razor`) con diseño clínico-estético (Tailwind CDN +
  Material Symbols, paleta teal/rosa), simulador interactivo de PIN (`landing-v2.js`) y
  acceso real vía `CenterLoginDialog`.

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
