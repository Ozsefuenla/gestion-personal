# Modelo multi-centro (tenancy)

Documenta el modelo de datos y las decisiones de diseño para soportar que el
software se venda por centro (peluquería / clínica estética) y no por trabajador
individual.

## Motivación

Antes, el PIN de 4 dígitos identificaba a un trabajador de forma **global**. Eso
provocaba problemas de seguridad graves: con un único PIN se podía suplantar a
cualquier trabajador de cualquier centro, y dos trabajadores no podían compartir PIN.

Con el modelo multi-centro:

- Cada centro es una unidad de aislamiento (tenant).
- Los trabajadores pertenecen a un centro (`Worker.CenterId`).
- El PIN se valida contra el trabajador **seleccionado** dentro del centro, por lo que
  puede repetirse entre trabajadores sin ambigüedad.

## Entidades de dominio

### `Center`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` | Identificador del centro (autoincremental). |
| `Name` | `string` | Nombre comercial del centro. |

### `CenterUser`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | `int` | Identificador del usuario. |
| `CenterId` | `int` | Centro al que pertenece el usuario. |
| `Username` | `string` | Nombre de usuario para entrar al centro. |
| `PasswordHash` | `string` | Contraseña (por ahora en claro; pendiente BCrypt). |

### `Worker` (extendido)

- Añade `CenterId` a los campos existentes (`Id`, `FullName`, `PinHash`, `Role`, `Active`, `DailyHours`).
- `MustChangePin` (bool): marca que el trabajador debe cambiar su PIN en el próximo
  acceso (activado por el admin al resetear el PIN; se apaga al cambiarlo).

## Flujo de identificación

1. **Selección de centro** (pendiente — landing/login): el usuario del centro se
   autentica con `CenterUser.Username`/`PasswordHash`. En esta iteración se opera
   contra un único centro demo (`id = 1`).
2. **Selector de trabajador**: se listan los trabajadores del centro
   (`IWorkersService.ListByCenterAsync`).
3. **PIN**: se valida el PIN del trabajador elegido (`workerId` + `pin`).

## Operaciones afectadas

La API HTTP se fusionó en la aplicación; estas operaciones se ejecutan in-process
vía `Application/Services`:

- `IWorkersService.LoginAsync`: `{ workerId, pin }` (antes `{ pin }`).
- `ITimeEntriesService.QuickClockAsync`: `{ workerId, pin, action }` (antes `{ pin, action }`).
- `IWorkersService.CreateAsync`: incluye `centerId`.
- Nuevas operaciones: `ICentersService.ListAsync`, `IWorkersService.ListByCenterAsync`.

## Pendientes

- JWT del trabajador para fichaje en tablet (la sesión del centro hoy es solo cookie).
- Persistencia SQL real.
