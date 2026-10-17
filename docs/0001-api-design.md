# Diseño de API — Gestión de Personal por Centro

> **Histórico — contrato HTTP de referencia.** La API se fusionó en el monolito
> `GestionPersonal.Web` (la UI consume `Application/Services` en proceso). Este
> documento se conserva como contrato objetivo para una futura re-extracción de la
> API; las rutas no están expuestas actualmente. Ver `docs/0002-solution-structure.md`.

## Necesidades

1. **Multi-centro**: el software se vende por centro (peluquería / clínica). Cada
   centro tiene sus propios trabajadores y un usuario de acceso (`CenterUser`).
2. **Alta de trabajadores**: un `admin` del centro registra trabajadores (nombre,
   PIN de 4 dígitos y rol) asociados a su centro.
3. **Fichajes**: cada trabajador registra inicio, pausa y fin de su jornada.
4. **Fichajes propios**: cada trabajador consulta únicamente sus fichajes.
5. **Fichajes globales**: el `admin` del centro consulta los fichajes de los
   trabajadores de su centro.
6. **Solicitud de ausencias**: cada trabajador solicita días de ausencia (vacaciones,
   enfermedad u otro) y consulta las suyas.
7. **Aprobación de ausencias**: el `admin` aprueba solicitudes (pendiente de implementar).
8. **Identificación de rol y centro** (imprescindible): autenticación mínima para
   distinguir `admin` de `trabajador` y aislar cada centro.

## Endpoints

### Centros

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| GET | `/api/centers` | Listar centros (base para el futuro selector de la landing) | Público (temporal) |
| GET | `/api/centers/{id}/workers` | Listar los trabajadores del centro (selector previo al PIN) | Público (temporal) |

### Autenticación (diferida, pendiente de la landing)

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/auth/login` | Autenticar el usuario del centro (`username`/`password`) | Público |

### Trabajadores

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/workers` | Alta de un trabajador (nombre, PIN de 4 dígitos, rol, `centerId`) | admin |
| GET | `/api/workers` | Listar trabajadores (necesario para filtrar fichajes y validar solicitudes) | admin |
| GET | `/api/workers/status` | Estado actual de todos los trabajadores con resumen del día (trabajado/pausado) y timeline | admin |
| POST | `/api/workers/login` | Iniciar sesión de un trabajador seleccionado por PIN (`{ workerId, pin }` → estado) | Público |
| GET | `/api/workers/{id}/status` | Estado actual de un trabajador por id | admin |
| POST | `/api/workers/{id}/change-pin` | Cambiar el PIN de un trabajador (verifica el PIN actual) | trabajador |
| GET | `/api/workers/manage` | Listar todos los trabajadores del centro (activos e inactivos) | admin |
| PATCH | `/api/workers/{id}/name` | Renombrar un trabajador (`{ fullName }`) | admin |
| POST | `/api/workers/{id}/reset-pin` | Resetear el PIN de un trabajador (`{ pin }` temporal + fuerza cambio al entrar) | admin |
| PATCH | `/api/workers/{id}/activate` | Reactivar un trabajador inactivo | admin |
| PATCH | `/api/workers/{id}/deactivate` | Dar de baja a un trabajador (rechazado si es admin) | admin |

### Fichajes

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/time-entries/start` | Registrar inicio de jornada (409 si ya está iniciada) | trabajador |
| POST | `/api/time-entries/pause` | Registrar pausa de jornada (409 si no está en curso) | trabajador |
| POST | `/api/time-entries/resume` | Reanudar jornada en pausa (409 si no está en pausa) | trabajador |
| POST | `/api/time-entries/end` | Registrar fin de jornada (409 si no está iniciada; válido en curso o en pausa) | trabajador |
| POST | `/api/time-entries/quick-clock` | Fichaje rápido por trabajador y PIN (`{ workerId, pin, action }`) | trabajador |
| PATCH | `/api/time-entries/active/{workerId}` | Editar la hora de inicio del fichaje activo | trabajador |
| DELETE | `/api/time-entries/active/{workerId}` | Eliminar el último fichaje activo (deshacer) | trabajador |
| GET | `/api/time-entries/today` | Estado actual y resumen del día (con línea de tiempo de tramos) | trabajador |
| GET | `/api/time-entries/monthly` | Resumen mensual por día (tiempo trabajado por día + objetivo diario) | trabajador |
| GET | `/api/time-entries/me` | Consultar fichajes propios | trabajador |
| GET | `/api/time-entries` | Consultar fichajes de todos los trabajadores (filtro opcional por `workerId`) | admin |

### Ausencias (vacaciones / enfermedad / otro)

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| GET | `/api/vacations?workerId={id}` | Listar las ausencias del trabajador (pendientes, aprobadas y denegadas, por fecha descendente) | trabajador |
| GET | `/api/vacations/center` | Listar todas las ausencias no eliminadas del centro (para la gestión del admin) | admin |
| POST | `/api/vacations` | Crear solicitud de ausencia (`{ workerId, startDate, endDate, type }`) → queda `pending` | trabajador |
| DELETE | `/api/vacations/{id}` | Eliminar (borrado lógico) una ausencia: pendiente siempre; aprobada solo si el dueño es `admin` | trabajador/admin |
| PATCH | `/api/vacations/{id}/approve` | Aprobar una solicitud pendiente | admin |
| PATCH | `/api/vacations/{id}/reject` | Denegar una solicitud pendiente | admin |

## Notas

- Rutas en inglés por convención REST; los recursos son `centers`, `workers`,
  `time-entries` y `vacations`.
- Los `centerId`/`workerId`/`id` de trabajador son **enteros** (autoincremental).
  `TimeEntry.id` y `Vacation.id` siguen siendo `Guid`.
- **Modelo multi-centro**: cada `Worker` pertenece a un `Center` (`CenterId`). Cada
  `Center` tiene un `CenterUser` (usuario/contraseña) que dará acceso al centro en la
  landing (pendiente). Por ahora se opera contra un único centro demo (`id = 1`).
- **El PIN es por trabajador**: ya no identifica a un trabajador de forma global. Se
  valida contra el `PinHash` del trabajador seleccionado (`workerId` + `pin`). Se
  permiten PINs duplicados entre trabajadores.
- La autenticación se limita a emitir un token con el rol; no incluye recuperación de
  contraseña, registro self-service ni refresh tokens.
- Hasta implementar autenticación, `start`, `pause`, `resume` y `end` reciben
  `workerId` en el cuerpo de la petición.
- `start` no se permite si ya existe un fichaje abierto; `pause` solo si está en curso;
  `resume` solo si está en pausa (devuelven `409 Conflict` en caso contrario).
- `end` es válido estando en curso o en pausa; devuelve `409 Conflict` si no hay jornada abierta.
- Se guarda el historial de pausas: cada pausa conserva su `pausedAt` y su `resumedAt`,
  permitiendo controlar varias pausas en la misma jornada.
- `GET /api/time-entries/today` calcula "hoy" en la zona horaria `Europe/Madrid` (fija)
  y devuelve el estado actual más una línea de tiempo de tramos `working`/`paused`.
- Pendiente: fichaje abierto de un día anterior (el estado actual lo mostraría, pero el
  resumen/timeline solo cubren entradas de hoy).
- El PIN es secreto y se guarda en el campo `PinHash` (por ahora en claro); pendiente:
  hashearlo con BCrypt. Igual ocurre con `CenterUser.PasswordHash`.
- `POST /api/time-entries/quick-clock` recibe `{ "workerId": 1, "pin": "1234", "action": "start" }`
  con 3 acciones (`start` = iniciar/reanudar, `pause`, `end`). Devuelve `400` si el PIN
  está mal formado o no coincide con el trabajador; `404` si el trabajador no existe;
  `200` con `success=false` + `currentStatus` + `availableActions` si la acción no
  corresponde al estado; y `200` con `success=true` si se ejecutó.
- `POST /api/workers/login` recibe `{ "workerId": 1, "pin": "1234" }` y devuelve el
  `WorkerStatus` del trabajador (identidad + resumen del día); `404` si el trabajador no
  existe, `400` si el PIN no coincide. `GET /api/workers/{id}/status` devuelve lo mismo
  por id (para refrescar la card).
- `POST /api/workers/{id}/change-pin` recibe `{ "currentPin", "newPin" }`: valida que el
  PIN actual sea correcto, que el nuevo sea de 4 dígitos y distinto al actual; devuelve
  `204 NoContent` o `400` con el error.
- `PATCH /api/time-entries/active/{workerId}` recibe `{ "start" }` y edita la hora de
  inicio del fichaje activo (ajustando el tramo anterior); `DELETE /api/time-entries/active/{workerId}`
  elimina el último fichaje activo (deshacer). Ambos devuelven `204` o `400`.
- `GET /api/time-entries/monthly` recibe `{ workerId, year, month }` y devuelve el
  resumen mensual del trabajador (ver `docs/0004` para el detalle de UI).
- `POST /api/vacations` recibe `{ workerId, startDate, endDate, type }`, con `type` en
  `vacations` / `sick-leave` / `other`. La fecha de fin no puede ser anterior a la de
  inicio (`400`) y no puede solapar con otra ausencia pendiente o aprobada del mismo
  trabajador (`409`). La solicitud queda en estado `pending`; si la solicita un
  trabajador con rol `admin`, queda `approved` automáticamente.
- `GET /api/vacations?workerId={id}` devuelve las ausencias pendientes, aprobadas y
  denegadas del trabajador, ordenadas por fecha de inicio descendente (las eliminadas no
  se listan).
- `DELETE /api/vacations/{id}` aplica **borrado lógico** (pasa a `deleted`). Solo se
  pueden eliminar las pendientes; las aprobadas únicamente si el trabajador dueño es
  `admin` (`403` en caso contrario).
- Estados de ausencia: `pending`, `approved`, `rejected`, `deleted`. Tipos: `vacations`,
  `sick-leave`, `other`.
- `PATCH /api/vacations/{id}/approve` y `PATCH /api/vacations/{id}/reject` deciden sobre
  una solicitud `pending` (`409` si no lo está, `404` si fue eliminada). La restricción de
  rol `admin` se aplica en el frontend (la API está autenticada por centro, no por
  trabajador).
- `GET /api/vacations/center` lista las ausencias no eliminadas de todo el centro (cada
  una con su `workerId`) para la vista de gestión del admin.
- `GET /api/time-entries/monthly` marca los días cubiertos por una ausencia **aprobada**
  con `isAbsence` + `absenceType`; esos días no computan para las horas del trabajador.
- Gestión de trabajadores (`docs/0009`): `Worker` gana `MustChangePin` (bool). Los
  listados "activos" (`GET /api/workers`, `GET /api/centers/{id}/workers`,
  `GET /api/workers/status`) filtran `Active == true`; `GET /api/workers/manage` devuelve
  todos (activos e inactivos) para el panel del admin. El login y el fichaje rápido
  rechazan a los trabajadores inactivos (`403`). `reset-pin` guarda el PIN temporal y
  activa `MustChangePin`; al entrar, el trabajador debe cambiarlo (al cambiarlo se apaga
  el flag). `deactivate` no permite dar de baja a un `admin` (`400`). La restricción de
  rol `admin` de estos endpoints se aplica en el frontend (la API autentica por centro).
