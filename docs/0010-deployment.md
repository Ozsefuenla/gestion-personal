# Despliegue en producción

Este documento describe el flujo de release y rollback de GestionPersonal en
MonsterASP.NET. El despliegue se hace desde GitHub Actions con aprobación manual;
`main` no despliega por sí sola.

## Hosting

- Proveedor: **MonsterASP.NET Free** (`https://gestion-personal.runasp.net`).
- Runtime: **.NET 10**, hosting model **InProcess**.
- Deploy: **Web Deploy** desde un runner de GitHub Actions.
- Persistencia: **PostgreSQL externo** (MonsterASP Free no incluye base de datos).
  La cadena de conexión llega por la variable de entorno `ConnectionStrings__Default`.
  Las claves de Data Protection se guardan en `App_Data/keys` en producción.
- HTTPS activo. La cookie de autenticación es de sesión y `Secure`.

## Base de datos en producción

1. Provisionar un PostgreSQL accesible desde MonsterASP (Neon, Supabase, RDS,
   DigitalOcean, VPS propio, etc.). El plan Free no incluye BBDD.
2. Crear la base de datos y un usuario con permisos de DDL (DbUp crea el esquema
   y la tabla `schemaversions`).
3. Configurar `ConnectionStrings__Default` como secreto del environment
   `production` (o variable del hosting). No commitear la cadena.
4. Dejar `Persistence:SeedDemoData` en `false` y definir
   `Persistence__AdminPassword` con una contraseña fuerte; la app siembra el
   centro demo y el usuario `admin` de forma idempotente.
5. Al arrancar, DbUp aplica las migraciones pendientes. La base de datos debe
   existir antes del primer arranque.
6. Antes de una release con cambios de esquema: backup y expand/contract. Un
   rollback de aplicación no revierte migraciones ni datos.

Los detalles de esquema, migraciones y seed están en
[`docs/0011-persistencia-postgres.md`](0011-persistencia-postgres.md).

## Artefacto

El workflow `release.yml` publica el proyecto Web **sin RID** (Any CPU):

```bash
dotnet publish src/GestionPersonal.Web/GestionPersonal.Web.csproj \
  -c Release \
  -p:Version=<version> \
  -p:ContinuousIntegrationBuild=true \
  -o publish
```

El zip resultante se adjunta al GitHub Release como
`GestionPersonal-vX.Y.Z.zip`. El deploy descarga ese asset y lo publica con Web
Deploy. El rollback usa el mismo asset, sin recompilar.

## Ramas y tags

| Rama / tag | Uso | Deploy |
|---|---|---|
| `main` | Integración, siempre con CI verde | No |
| `feat/*`, `fix/*`, `chore/*`, `ci/*` | Trabajo diario, PR a `main` | No |
| `release/vX.Y.Z` | Candidato a producción; actualiza CHANGELOG | Dispara `release.yml` |
| Tag `vX.Y.Z` | Artefacto inmutable de producción | Lo usa `deploy.yml` / `rollback.yml` |

- SemVer: `MAJOR.MINOR.PATCH`.
- El tag se crea una sola vez y no se mueve.
- El tag es la fuente de verdad de producción, no `main`.
- Si hay que corregir una release, se saca un patch nuevo (`v1.0.1`), no se
  reutiliza el tag anterior.

## Environment `production`

Configurado en **Settings → Environments → production**:

- **Required reviewers**: el propietario del repositorio.
- **Prevent self-review**: desactivado (desarrollo individual).
- **Wait timer**: 5 minutos.
- **Deployment branches and tags**: `main`, `release/*`, `v*`.
- **Environment secrets** (solo nombres):
  - `WEBSITE_NAME`
  - `SERVER_COMPUTER_NAME`
  - `SERVER_USERNAME`
  - `SERVER_PASSWORD`
- **Environment variable**:
  - `PRODUCTION_URL=https://gestion-personal.runasp.net`

Los valores se copian del panel de MonsterASP (**Deploy → Visual Studio
access**) y nunca se commitean.

## Release

1. Verificar que `main` está verde y con el CHANGELOG actualizado.
2. Crear `release/vX.Y.Z` desde `main`.
3. En la rama de release:
   - mover las entradas de `Unreleased` a `## [X.Y.Z] - AAAA-MM-DD`;
   - dejar una sección `Unreleased` vacía al tope.
4. Push de `release/vX.Y.Z`. Se ejecuta `release.yml`:
   - valida el nombre de rama y el CHANGELOG;
   - corre tests unitarios y de integración;
   - publica y comprime el artefacto;
   - crea el tag `vX.Y.Z` y un GitHub Release **pre-release** con el zip.
5. El job `deploy` espera la aprobación del environment `production` y el wait
   timer de 5 minutos.
6. Al aprobar:
   - descarga el asset del release;
   - publica por Web Deploy;
   - prueba `/health` y `/login`;
   - promueve el release a **latest**.
7. Abrir PR `release/vX.Y.Z` → `main` y mergear con **squash** o **rebase**
   (el ruleset de `main` exige linear history). El tag queda en el commit de la
   rama de release.

## Rollback

1. Actions → **Rollback production** → Run workflow.
2. Indicar el tag destino (`vX.Y.Z`) y el motivo.
3. Aprobar el environment `production`.
4. El workflow llama a `deploy.yml` con `mode: rollback`: despliega el asset
   del release indicado, pero **no** lo promueve a latest.
5. Verificar `/health`, `/login` y `/app`.

Un rollback de aplicación **no revierte** cambios de esquema ni datos. Antes de
cambiar el esquema, aplicar expand/contract y backup previo.

## Reglas de GitHub

- `main`: PR obligatorio, linear history, no force push, no borrado y check
  `build-test` obligatorio.
- `release/*`: no force push, no borrado; sin PR obligatorio porque el push
  dispara la release.
- Tags `v*`: no force push, no borrado. No restringir la creación: el tag lo
  crea GitHub Actions.
- El environment `production` limita los deploys a `main`, `release/*` y `v*`.

## Verificación después de un deploy

- `GET /health` → 200.
- `GET /login` → 200.
- Login con el usuario demo → `/app` → 200 (implica que la conexión a PostgreSQL
  y el esquema están operativos).
- Logout.
- Export PDF.
- En DevTools, el WebSocket de Blazor conecta.
- En MonsterASP, la memoria pico no supera 256 MB.

## Limitaciones conocidas del plan Free

- 256 MB de RAM: el app pool puede reciclar si se supera el límite.
- Idle shutdown a las 3 horas.
- Sin AlwaysRunning, sin backups automáticos y sin dominio propio.
- HTTPS manual (ya activo).
- MonsterASP puede enviar un header `Date` desfasado. Por eso la cookie de
  autenticación es de sesión y no persistente.

## Primer release

Para el primer release se usa `v1.0.0`. Para probar el rollback hace falta un
segundo release: `v1.0.1` con un cambio trivial, deploy y luego rollback a
`v1.0.0`.
