# Diseño de API — Gestión de Trabajadores de Peluquería

## Necesidades

1. **Alta de trabajadores**: un usuario `admin` registra trabajadores (nombre, PIN de 4 dígitos y rol) en el sistema.
2. **Fichajes**: cada trabajador registra inicio, pausa y fin de su jornada.
3. **Fichajes propios**: cada trabajador consulta únicamente sus fichajes.
4. **Fichajes globales**: el `admin` consulta los fichajes de todos los trabajadores.
5. **Solicitud de vacaciones**: cada trabajador solicita días de vacaciones.
6. **Aprobación de vacaciones**: el `admin` aprueba o rechaza solicitudes.
7. **Identificación de rol** (imprescindible): autenticación mínima para distinguir `admin` de `trabajador`.

## Endpoints

### Autenticación (mínima, solo para identificar rol)

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/auth/login` | Autenticar usuario y obtener token que identifica su rol | Público |

### Trabajadores

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/workers` | Alta de un trabajador (nombre, PIN de 4 dígitos, rol) | admin |
| GET | `/api/workers` | Listar trabajadores (necesario para filtrar fichajes y validar solicitudes) | admin |

### Fichajes

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/time-entries/start` | Registrar inicio de jornada (409 si ya está iniciada) | trabajador |
| POST | `/api/time-entries/pause` | Registrar pausa de jornada (409 si no está en curso) | trabajador |
| POST | `/api/time-entries/resume` | Reanudar jornada en pausa (409 si no está en pausa) | trabajador |
| POST | `/api/time-entries/end` | Registrar fin de jornada (409 si no está iniciada; válido en curso o en pausa) | trabajador |
| GET | `/api/time-entries/today` | Estado actual y resumen del día (con línea de tiempo de tramos) | trabajador |
| GET | `/api/time-entries/me` | Consultar fichajes propios | trabajador |
| GET | `/api/time-entries` | Consultar fichajes de todos los trabajadores (filtro opcional por `workerId`) | admin |

### Vacaciones

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/vacations` | Crear solicitud de vacaciones | trabajador |
| GET | `/api/vacations` | Listar solicitudes (filtro opcional por estado) | admin |
| PATCH | `/api/vacations/{id}/approve` | Aprobar solicitud | admin |
| PATCH | `/api/vacations/{id}/reject` | Rechazar solicitud | admin |

## Notas

- Rutas en inglés por convención REST; los recursos son `workers`, `time-entries` y `vacations`.
- La autenticación se limita a emitir un token con el rol; no incluye recuperación de contraseña, registro self-service ni refresh tokens.
- El `GET /api/workers` se incluye únicamente porque `admin` lo necesita para filtrar fichajes y validar solicitudes; no implica CRUD completo de trabajadores.
- Hasta implementar autenticación, `start`, `pause`, `resume` y `end` reciben `workerId` en el cuerpo de la petición.
- `start` no se permite si ya existe un fichaje abierto; `pause` solo si está en curso; `resume` solo si está en pausa (devuelven `409 Conflict` en caso contrario).
- `end` es válido estando en curso o en pausa; devuelve `409 Conflict` si no hay jornada abierta.
- Se guarda el historial de pausas: cada pausa conserva su `pausedAt` y su `resumedAt`, permitiendo controlar varias pausas en la misma jornada.
- `GET /api/time-entries/today` calcula "hoy" en la zona horaria `Europe/Madrid` (fija) y devuelve el estado actual más una línea de tiempo de tramos `working`/`paused`; el tramo `idle` (huecos y fuera del horario) lo resuelve el frontend. El estado actual (`currentStatus`) es: `in-progress`, `paused`, `finished` (hay fichajes cerrados hoy) o `idle` (sin fichajes hoy).
- Pendiente: fichaje abierto de un día anterior (el estado actual lo mostraría, pero el resumen/timeline solo cubren entradas de hoy).
- Los trabajadores se identifican por un PIN de 4 dígitos (se eliminó el `email`). El PIN es secreto y se guarda en el campo `PinHash` (por ahora en claro); pendiente: hashearlo con BCrypt.
- Por ahora se permiten trabajadores duplicados (sin unicidad por PIN).
- Pendiente: flujo de verificación de PIN (el frontend selecciona un trabajador, teclea el PIN y el backend verifica que coincide).
- Pendiente: al implementar seguridad, el login (`/api/auth/login`) dejará de usar `email`/`password` y pasará a basarse en el PIN.
