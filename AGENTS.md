# AGENTS.md

## Proyecto
Aplicación web de gestión de personal, control horario, turnos y vacaciones
para peluquerías y clínicas estéticas en España. Backend en C# (.NET 10) y
frontend en Blazor Server.

## Stack
- Backend: ASP.NET Core 10 + Blazor Server (C#).
- Persistencia: PostgreSQL con Dapper (SQL directo) y migraciones DbUp
  (sin ORM). En tests se usan repositorios in-memory como dobles.
- Frontend: Blazor Web App (.NET 10, interactividad Server) + MudBlazor.
- Seguridad: login del centro con cookie HTTP-Only (BCrypt en contraseña y PIN).
  Pendiente: JWT del trabajador para fichaje en tablet.

## Arquitectura (Clean Architecture)
- Proyectos: `Domain`, `Application`, `Infrastructure`, `Web` (Blazor) y `Tests`.
- Las dependencias apuntan siempre hacia adentro; el núcleo (Domain) no
  depende de infraestructura ni de ASP.NET.
- Cada feature vive en una carpeta de `Application` con su request (record),
  handler, response (record) y validador FluentValidation.
- Monolito modular: la UI consume servicios de aplicación (`Application/Services`,
  facades por agregado) en proceso. No hay API HTTP separada; el contrato histórico
  vive en `docs/0001-api-design.md` para una futura re-extracción.

## Convenciones de código
- DTOs como `record` en C#. Validación de entrada con FluentValidation.
- Código completo y listo para producción: sin imports omitidos y sin
  comentarios tipo "// Implementar aquí".
- Nombres e identificadores en inglés; documentación y texto de negocio en
  español.

## Manejo de errores
- Result Pattern en la capa de aplicación.
- La UI consume `Result`/`Result<T>` y muestra `Error.Message`; no usa excepciones
  para flujos de negocio. Las excepciones no controladas las maneja
  `UseExceptionHandler("/Error")` en el Web.

## Fechas y horas
- Usar siempre `DateTimeOffset` en C# y UTC en base de datos.

## Documentación
- Todo punto importante se documenta en `docs/` como Markdown con nombre
  `NNNN-titulo.md` (numeración secuencial ya usada en el proyecto: `0001-api-design.md`).
- Los documentos de API se acompañan de su especificación OpenAPI (`.yaml`)
  cuando aplique. `docs/0001-api-design.md` y `docs/0001-api-openapi.yaml` se
  conservan como contrato histórico para una futura re-extracción de la API.
- El `README.md` se mantiene actualizado ante cualquier cambio que lo afecte.
- Las reglas de UI del frontend están en `docs/0003-frontend-ui.md` (base común:
  estados, colores, acciones, timeline) y en `docs/0004-frontend-ui-worker.md`
  (trabajador logueado: card personal, donut, cambio de PIN, edición de marcajes,
  control horario).
- La landing page (copy, diseño, propuestas de venta) está en `docs/0006-landing-page.md`.
  Se implementa como página raíz en `Components/Pages/LandingPage.razor` (ruta `/`) con su
  layout `LandingLayout`. El dashboard tras login de centro está en `/app` (`Dashboard.razor`
  + `AppLayout.razor`).
- La persistencia (esquema, migraciones DbUp, seed y configuración local/producción)
  está en `docs/0011-persistencia-postgres.md`.

## Changelog
- Todo cambio se refleja en `CHANGELOG.md` siguiendo
  https://keepachangelog.com/ (formato 1.1.0) y Semantic Versioning.
- Secciones: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`;
  entrada `Unreleased` siempre al tope.

## Git y control de versiones
- `main` está protegida: **nunca** se commitea ni se hace push directo. Todo cambio
  va por **rama + Pull Request** y se mergea tras el CI en verde.
- Nombres de rama: `tipo/descripcion` en minúsculas (p. ej. `feat/gestion-trabajadores`,
  `fix/login-enter`, `chore/ci-setup`).
- Commits en formato Conventional Commits (`feat(scope): descripción`), sin trailers
  de co-autoría.
- El CI (`.github/workflows/ci.yml`) compila y corre los tests en cada PR; debe pasar
  antes de mergear. Plantilla de PR en `.github/pull_request_template.md`.

## Testing
- TDD: escribir el test antes que la implementación.
- Framework: xUnit. Los tests ejercitan los servicios de aplicación
  (`Application/Services`) con repositorios en memoria (`AddInMemoryInfrastructure`)
  y un `ICurrentCenter` falso. Los tests de integración también usan in-memory.
- Comandos: `dotnet test`, `dotnet build`.

## Comandos útiles
- `docker compose up -d` (PostgreSQL local; necesario para `dotnet run`).
- `docker compose down -v` (reinicia la base de datos desde cero).
- `dotnet build` / `dotnet test` desde `src/`.
- Frontend: `dotnet run --project src/GestionPersonal.Web`.
