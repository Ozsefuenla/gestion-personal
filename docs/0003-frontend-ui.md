# Reglas de UI del frontend (Blazor + MudBlazor)

Documenta las reglas de interfaz del panel de fichaje para mantener la coherencia
en el frontend y que el asistente las encuentre rápido.

## Framework y estructura

- Blazor Web App (.NET 10, interactividad Server) + MudBlazor.
- Proyecto `GestionPersonal.Web` en `src/`.
- Carpetas:
  - `Models/` → DTOs del cliente (`WorkerStatus`, `TimelineSegment`).
  - `Services/` → `ApiClient` (cliente HTTP tipado).
  - `Components/` → `Pages/Home.razor` (página), `WorkerCard.razor` (card).

## Conexión con la API

- `appsettings.json` → `"Api": { "BaseUrl": "http://localhost:5243" }`.
- `Program.cs` → `AddHttpClient<ApiClient>(...)` con la base de `Api:BaseUrl`.
- Las llamadas son server-to-server (Blazor Server), **sin CORS**.
- Endpoint del panel: `GET /api/workers/status` (devuelve workers + estado + resumen + timeline).

## Máquina de estados → acciones habilitadas

| Estado (`currentStatus`) | Acciones habilitadas |
|--------------------------|----------------------|
| `idle` | Iniciar |
| `in-progress` | Pausar, Finalizar |
| `paused` | Reanudar, Finalizar |
| `finished` | ninguna (terminal) |

## Estado → color y etiqueta (chip de estado)

| Estado | Color (MudBlazor) | Etiqueta |
|--------|-------------------|----------|
| `idle` | `Color.Default` (gris) | Sin iniciar |
| `in-progress` | `Color.Success` (verde) | En curso |
| `paused` | `Color.Warning` (ámbar) | En pausa |
| `finished` | `Color.Info` (azul) | Finalizado |

## Botones de acción

| Botón | Color | Habilitado cuando |
|-------|-------|-------------------|
| Iniciar | `Color.Success` | `idle` |
| Pausar | `Color.Warning` | `in-progress` |
| Reanudar | `Color.Info` | `paused` |
| Finalizar | `Color.Secondary` | `in-progress` o `paused` |

## Card (`WorkerCard.razor`)

Todo visible por defecto (sin click-para-expandir):

1. Nombre, rol y chip de estado (color según tabla).
2. Resumen: `Trabajado: <b>…</b> · Pausado: <b>…</b>`.
3. Mini progressbar (si hay timeline).
4. Lista de timeline (o "Sin actividad hoy.").
5. Los 4 botones de acción (siempre visibles, deshabilitados según estado).

## Formato de duraciones (`FormatDuration`)

- ≥ 1 h → `4h 30m 15s`
- ≥ 1 min → `45m 20s`
- < 1 min → `5s`

## Formato de horas (`MadridTime.Format`)

- Convierte a `Europe/Madrid` (fallback `Romance Standard Time` en Windows).
- Formato: `HH:mm:ss`.

## Timeline

- Cada segmento:
  - `working` → círculo verde, etiqueta "Trabajando".
  - `paused` → círculo ámbar, etiqueta "Pausa".
- Rango: `HH:mm:ss – HH:mm:ss`.
- Si `currentStatus == "finished"`: se agrega una línea "Finalizado" (círculo azul) con la hora de fin (último `to` del timeline).
- Si el timeline está vacío: "Sin actividad hoy.".

## Mini progressbar

- Barra de **ratio agregado** (bloques contiguos): verde = % trabajado, ámbar = % pausado.
  - `verde% = WorkedTime / (WorkedTime + PausedTime)`, `ámbar% = PausedTime / (…)`.
- Colores hardcodeados: verde `#43a047`, ámbar `#fb8c00` (los `--mud-palette-*` no están en el CSS y se inyectan vía tema; hardcodear evita que quede transparente).
- Siempre visible; si no hay actividad (`WorkedTime + PausedTime = 0`), muestra una barra gris vacía (`#e0e0e0`).
- Se actualiza automáticamente al cambiar de estado (refresh de la card).
- Los porcentajes se formatean con `CultureInfo.InvariantCulture` (la cultura es-ES usa coma decimal y eso rompe el `width` de CSS).

## Pendientes

- Verificación de PIN antes de fichar.
- Seguridad (JWT + cookie, hash del PIN con BCrypt).
