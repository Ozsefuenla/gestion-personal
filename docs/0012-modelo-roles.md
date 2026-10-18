# 0012 — Modelo de roles y autorización

Documenta el modelo de identidades de la aplicación, la separación entre la
administración de la plataforma, la administración del centro, el usuario de
tablet (kiosco) y el trabajador, y cómo se aplica la autorización en el servidor.

## Niveles

| Nivel | Identidad | Credencial | Permisos |
|-------|-----------|------------|----------|
| `SuperAdmin` | global (`center_id NULL`) | usuario + contraseña | Alta/baja de centros y de sus `CenterAdmin` |
| `CenterAdmin` | centro | usuario + contraseña | Gestión de workers, vacaciones, horarios, y alta/baja de usuarios `Center` |
| `Center` | centro | usuario + contraseña | Kiosco de fichaje: quick-clock y paso al login por PIN del worker |
| `CenterWorker` | centro | PIN de 4 dígitos | Fichar, ver su control horario y solicitar vacaciones (solo sus propios datos) |

- `CenterUser.Role` almacena `SuperAdmin` / `CenterAdmin` / `Center` (tabla
  `center_users`). `CenterWorker` nunca se almacena en `center_users`: los
  trabajadores viven en `workers` y carecen de columna `role`.
- Un `CenterAdmin` **no** es un trabajador: no ficha ni tiene PIN. La figura
  antigua de "worker con rol admin" desaparece.

## Identidad en la sesión

La aplicación usa **dos cookies** y un esquema de política que elige cuál se
evalúa en cada petición:

| Cookie | Esquema | Contenido |
|--------|---------|-----------|
| `gp.center` | `CenterCookie` | `NameIdentifier` = id de `center_users`, `centerId` (salvo SuperAdmin), `centerName`, `ClaimTypes.Role` = `SuperAdmin`/`CenterAdmin`/`Center` |
| `gp.worker` | `WorkerCookie` | `NameIdentifier` = id del worker, `centerId`, `ClaimTypes.Role` = `CenterWorker` |

- El esquema `AppScheme` (por defecto) delega en `WorkerCookie` si existe la cookie
  `gp.worker`; en caso contrario, en `CenterCookie`. Así el kiosco y el admin
  conservan su sesión y el worker tiene una sesión propia y efímera (8 h).
- `ICurrentUser` (implementado por `CookieCurrentUser`) expone `SubjectId`,
  `CenterId`, `Role` y `WorkerId`. En Blazor Server lee el estado de autenticación
  del circuito y cae a `IHttpContextAccessor` durante el render SSR.
- `CookieCurrentCenter` ya no existe: `ICurrentCenter` lo implementa el mismo
  `CookieCurrentUser`.

## Autorización

La autorización se aplica en los **handlers** de `Application`, nunca solo en la
UI. `AccessControl` centraliza las comprobaciones:

- `RequireSuperAdmin`: alta/baja de centros, alta/baja de `CenterAdmin`.
- `RequireCenterAdmin`: gestión de workers (alta, rename, reset PIN, baja,
  reactivación), listado completo, estado de hoy de todos, vacaciones del centro
  (listar/aprobar/denegar) y usuarios `Center`.
- `RequireCenterMember`: listados necesarios para el kiosco.
- `RequireKiosk`: quick-clock.
- `RequireSelfOrAdmin`: operaciones de autoservicio del worker. Un
  `CenterWorker` solo puede operar sobre su propio `workerId`
  (`ICurrentUser.WorkerId`); un `CenterAdmin` puede operar sobre cualquier worker
  de su centro.

`WorkerScope` ofrece `GetAdminScopedWorkerAsync` (solo admin) y
`GetSelfScopedWorkerAsync` (worker propio o admin). Los casos de uso de fichaje
(`Start/Pause/Resume/End`) comparten la lógica con el quick-clock mediante
métodos internos que omiten el chequeo de scope, porque el PIN ya autenticó al
worker.

## Rutas

- `/` landing, `/login` acceso (centro o SuperAdmin).
- `/app` se adapta al rol: el `Center` ve el kiosco (fichaje rápido y acceso por
  PIN); el `CenterAdmin` ve solo la administración del centro (gestión de
  workers, ausencias, horarios y usuarios de tablet), sin panel de fichaje ni
  login de trabajador; el `CenterWorker` ve su espacio personal.
- `/admin` solo `SuperAdmin` (atributo `[Authorize(Roles = "SuperAdmin")]`).

## Área `/admin` (SuperAdmin)

- Los centros se muestran en dos tabs: **Activos** y **Desactivados**, con el
  número de centros en cada pestaña.
- Cada centro es un `MudExpansionPanel` **colapsado por defecto**; al expandirlo
  se cargan sus usuarios y se habilitan las acciones.
- Centro activo: alta de `CenterAdmin`. Centro desactivado: solo se muestran sus
  usuarios (reactivar el centro para dar de alta nuevos).
- Listado de usuarios: solo `CenterAdmin` y `Center` (nunca `SuperAdmin`).
- Acciones por usuario: **reactivar/desactivar** y **resetear contraseña**
  (diálogo con nueva contraseña y confirmación, mínimo 4 caracteres).
- Permisos del reset (`CenterUserAccess`): el `SuperAdmin` puede resetear
  `CenterAdmin`/`Center`; el `CenterAdmin` solo usuarios `Center` de su centro.
- El reset no invalida sesiones ya abiertas: el nuevo password aplica al
  siguiente login.

## Cambio forzado de contraseña

- `CenterUser.MustChangePassword` marca que el usuario debe cambiar la contraseña
  en su próximo acceso. Se activa en **cualquier reset** (`ResetPassword`) y en
  las **altas** de `CenterAdmin`/`Center` (el administrador elige una contraseña
  temporal).
- El login devuelve `MustChangePassword` y, si es `true`, añade la claim
  `mustChangePassword` a la cookie del centro.
- **Popup, no pantalla**: al terminar el login, `CenterLoginForm` abre el
  `ChangePasswordDialog` (bloqueante) antes de navegar a `/app`. Si el usuario
  entra directo a `/app` o `/admin` con el flag, `AppLayout` abre el mismo popup
  como red de seguridad.
- El diálogo pide la nueva contraseña (y la actual si no vino del login) y llama
  a `POST /auth/change-password`, que verifica, ejecuta `ChangePassword` en el
  dominio (limpia el flag) y **re-emite la cookie** sin la claim.
- No hay página `/change-password` ni redirección de middleware.
- Limpieza en BD: migración `0003_center_user_must_change_password.sql`
  (`must_change_password BOOLEAN NOT NULL DEFAULT FALSE`).

## Datos y migración

`0002_user_roles.sql` (expand/contract):

- `centers.active` (baja lógica de centros).
- `center_users.role` (`SuperAdmin`/`CenterAdmin`/`Center`) y `center_users.active`.
- `center_users.center_id` pasa a nullable (SuperAdmin).
- Se elimina `workers.role`.

`0003_center_user_must_change_password.sql`:

- `center_users.must_change_password` (cambio forzado tras reset o alta).

## Seed

`DatabaseInitializer` es idempotente y crea:

- SuperAdmin: `superadmin` (config `Persistence:SuperAdminUsername` /
  `Persistence:SuperAdminPassword`).
- CenterAdmin del centro demo: `carlos.ruiz` (`Persistence:CenterAdminUsername` /
  `Persistence:CenterAdminPassword`).
- Usuario kiosco del centro demo: `centro` (`Persistence:CenterUsername` /
  `Persistence:CenterPassword`).
- En desarrollo, trabajadores demo María García (`1111`) y Ana López (`2222`).

Carlos Ruiz deja de existir como trabajador: su identidad pasa a ser el
`CenterAdmin` del centro. El antiguo usuario `admin` deja de sembrarse; en una
base de datos existente puede eliminarse a mano o reiniciando el volumen de
desarrollo (`docker compose down -v`).

## Tests

- Los tests de servicios usan `FakeCurrentUser` (por defecto `CenterAdmin`) y
  pueden cambiar a `CenterWorker` con `User.ActAsWorker(workerId)`.
- Se cubren login por rol, permisos de gestión y el scoping por worker en
  autoservicio.
- `AccountTests` cubre el cambio de contraseña (actual incorrecta, nueva corta,
  igual a la actual y el flujo reset → login con flag → cambio → flag limpio).
