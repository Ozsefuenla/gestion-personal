# AGENTS.md

## Proyecto
Aplicación web de gestión de personal, control horario, turnos y vacaciones
para peluquerías y clínicas estéticas en España. Backend en C# (.NET 10) y
frontend en Next.js.

## Stack
- Backend: ASP.NET Core 10 Web API (C#), EF Core 10, PostgreSQL.
- Frontend: Next.js 14+ (App Router, TypeScript), Tailwind CSS, Shadcn UI,
  TanStack Query, Lucide React.
- Seguridad (diferida): JWT + cookie HTTP-Only, PIN de 4 dígitos hasheado
  con BCrypt para fichaje en tablet.

## Arquitectura (Clean Architecture)
- Cuatro proyectos: `Domain`, `Application`, `Infrastructure`, `Api`.
- Las dependencias apuntan siempre hacia adentro; el núcleo (Domain) no
  depende de EF Core ni de ASP.NET.
- Cada feature vive en una carpeta de `Application` con su request (record),
  handler, response (record) y validador FluentValidation.
- Endpoints delgados: Minimal APIs en `Api/Endpoints` agrupadas por recurso.

## Convenciones de código
- DTOs como `record` en C#. Validación de entrada con FluentValidation.
- Código completo y listo para producción: sin imports omitidos y sin
  comentarios tipo "// Implementar aquí".
- Nombres e identificadores en inglés; documentación y texto de negocio en
  español.

## Manejo de errores
- Result Pattern en la capa de aplicación.
- Middleware global de excepciones que devuelve `ProblemDetails` (RFC 7807).

## Fechas y horas
- Usar siempre `DateTimeOffset` en C# y `timestamptz` (UTC) en PostgreSQL.

## Documentación
- Todo punto importante se documenta en `docs/` como Markdown con nombre
  `NNNN-titulo.md` (numeración secuencial ya usada en el proyecto: `0001-api-design.md`).
- Los documentos de API se acompañan de su especificación OpenAPI (`.yaml`)
  cuando aplique.
- El `README.md` se mantiene actualizado ante cualquier cambio que lo afecte.

## Changelog
- Todo cambio se refleja en `CHANGELOG.md` siguiendo
  https://keepachangelog.com/ (formato 1.1.0) y Semantic Versioning.
- Secciones: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`;
  entrada `Unreleased` siempre al tope.

## Testing
- TDD: escribir el test antes que la implementación.
- Framework: xUnit. Comandos: `dotnet test`, `dotnet build`.

## Comandos útiles
- `dotnet build` / `dotnet test` desde `src/`.
- Migraciones: `dotnet ef migrations add <Nombre>` y `dotnet ef database update`.
- Frontend: `npm run dev` / `npm run build` desde `web/`.
