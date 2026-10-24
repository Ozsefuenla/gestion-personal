# Gestión de trabajadores (admin)

Documenta la funcionalidad de gestión de trabajadores del centro, disponible solo
para el rol `CenterAdmin` (usuario del centro con contraseña). Ver
[`0012-modelo-roles.md`](0012-modelo-roles.md). Complementa
`docs/0008-frontend-ui-admin.md` (reglas de UI) y `docs/0001-api-design.md`
(contrato HTTP histórico).

> **Nota de arquitectura:** la API HTTP se fusionó en el monolito `GestionPersonal.Web`.
> Las rutas `/api/*` citadas abajo son el contrato histórico (ver `docs/0001-api-design.md`);
> hoy equivalen a operaciones de `Application/Services` ejecutadas in-process.

## Motivación

El admin del centro necesita administrar su plantilla: cambiar el nombre de un
trabajador, resetear su PIN (por olvido o cambio de personal) y dar de baja /
reactivar trabajadores, controlando qué trabajadores aparecen en el fichaje y en los
listados del centro.

## Modelo de datos

### `Worker` (extendido)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Active` | `bool` | Ya existía; ahora se usa: los trabajadores inactivos no aparecen en los listados "activos" ni pueden fichar/loguearse. |
| `MustChangePin` | `bool` | Nuevo. `true` fuerza al trabajador a cambiar su PIN en el próximo acceso. |

Métodos de dominio añadidos a `Worker`:

- `Rename(string fullName)` — cambia `FullName` (recorta espacios).
- `ResetPin(string newPinHash)` — setea `PinHash` y `MustChangePin = true`.
- `Deactivate()` / `Activate()` — cambian `Active`.
- `ChangePin(string newPinHash)` — ahora además pone `MustChangePin = false`.

`MustChangePin` se eligió como flag booleano (columna `bit` en SQL) frente a un enum
de estado o un timestamp, por simplicidad y portabilidad directa a la futura BBDD SQL.

## Endpoints

| Método | Ruta | Propósito |
|--------|------|-----------|
| GET | `/api/workers/manage` | Listar todos los trabajadores del centro (activos e inactivos), con `Active` y `MustChangePin`. |
| POST | `/api/workers` | Alta de trabajador (`{ fullName, pin, role, centerId, mustChangePin }`). |
| PATCH | `/api/workers/{id}/name` | Renombrar (`{ fullName }`). |
| POST | `/api/workers/{id}/reset-pin` | Resetear PIN (`{ pin }` de 4 dígitos) y activar `MustChangePin`. |
| PATCH | `/api/workers/{id}/activate` | Reactivar. |
| PATCH | `/api/workers/{id}/deactivate` | Dar de baja. |

## Comportamiento

### Alta de trabajador (PIN inicial con cambio forzado)

- Desde "Gestión de trabajadores" → **"Añadir nuevo trabajador"**: el admin introduce
  **nombre** y **PIN** (con repetir PIN).
- La UI llama a `POST /api/workers` con `role = "worker"` y `mustChangePin = true`, de
  modo que el PIN inicial es temporal y el trabajador debe cambiarlo en su primer acceso
  (mismo flujo de cambio forzado que el reset de PIN).
- `CreateWorkerCommand`/`Worker.Create` ganan el flag opcional `MustChangePin`
  (default `false`), por lo que las altas sin el flag mantienen el comportamiento previo.

### Reset de PIN (flujo "debe cambiar la contraseña")

1. El admin introduce un **PIN temporal** de 4 dígitos.
2. El backend guarda su hash BCrypt y pone `MustChangePin = true`.
3. Al entrar (`POST /api/workers/login`), la respuesta devuelve `MustChangePin = true`.
4. El frontend abre `ChangePinDialog` de forma bloqueante; al confirmar el nuevo PIN,
   `change-pin` apaga el flag.

El fichaje rápido (`quick-clock`) también exige el cambio forzado: si
`MustChangePin == true`, no se ejecuta la acción de fichaje; la respuesta devuelve
`MustChangePin = true` y el kiosco abre `ChangePinDialog` de forma bloqueante
(con el PIN temporal ya validado, ocultando el campo "PIN actual"). Al confirmar
el nuevo PIN, el fichaje se ejecuta automáticamente con el nuevo PIN.

### Activos / Inactivos

- Los listados "activos" filtran `Active == true`: `GET /api/workers`,
  `GET /api/centers/{id}/workers` (selector) y `GET /api/workers/status` (panel de hoy).
- `GET /api/workers/manage` devuelve todos para que el admin vea también los inactivos.
- El login y el fichaje rápido rechazan a los inactivos (`403`).
- No hay "trabajadores admin": el administrador es un `CenterAdmin` (usuario del
  centro), por lo que cualquier trabajador puede darse de baja.

## Seguridad

- Scoping por centro: todos los handlers usan `WorkerScope` (`GetByIdAndCenterAsync`),
  de modo que no se puede operar sobre trabajadores de otro centro (BOLA).
- La restricción "solo admin" se aplica en el servidor: `AccessControl.RequireCenterAdmin`
  en los handlers de gestión (ver `0012-modelo-roles.md`). La UI solo la refleja.

## Tests

`GestionPersonal.Tests/Services/WorkerManagementTests.cs` cubre: renombrar (éxito y
nombre vacío), reset de PIN (fuerza cambio y lo limpia al cambiar), formato de PIN,
dar de baja (login `403` y exclusión de listados activos), reactivación y no poder dar
de baja a un admin.
