# Diseño de API — Gestión de Trabajadores de Peluquería

## Necesidades

1. **Alta de trabajadores**: un usuario `admin` registra trabajadores en el sistema.
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
| POST | `/api/workers` | Alta de un trabajador | admin |
| GET | `/api/workers` | Listar trabajadores (necesario para filtrar fichajes y validar solicitudes) | admin |

### Fichajes

| Método | Ruta | Propósito | Rol |
|--------|------|-----------|-----|
| POST | `/api/time-entries/start` | Registrar inicio de jornada | trabajador |
| POST | `/api/time-entries/pause` | Registrar pausa de jornada | trabajador |
| POST | `/api/time-entries/end` | Registrar fin de jornada | trabajador |
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
