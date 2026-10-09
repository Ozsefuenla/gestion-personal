# Reglas de UI del frontend (Blazor + MudBlazor)

Documenta las reglas de interfaz del panel de fichaje para mantener la coherencia
en el frontend y que el asistente las encuentre rápido.

## Framework y estructura

- Blazor Web App (.NET 10, interactividad Server) + MudBlazor.
- Proyecto `GestionPersonal.Web` en `src/`.
- Carpetas:
  - `Models/` → DTOs del cliente (`WorkerStatus`, `TimelineSegment`, `QuickClockResult`).
  - `Services/` → `ApiClient` (cliente HTTP tipado).
  - `Components/` → `Pages/Home.razor` (página), `WorkerCard.razor` (card),
    `QuickClockPanel.razor` (panel de fichaje rápido), `QuickClockDialog.razor` (modal de PIN).

## Conexión con la API

- `appsettings.json` → `"Api": { "BaseUrl": "http://localhost:5243" }`.
- `Program.cs` → `AddHttpClient<ApiClient>(...)` con la base de `Api:BaseUrl`.
- Las llamadas son server-to-server (Blazor Server), **sin CORS**.
- Endpoint del panel: `GET /api/workers/status` (devuelve workers + estado + resumen + timeline).
- Endpoint de fichaje rápido: `POST /api/time-entries/quick-clock` (`{ pin, action }`).

## Máquina de estados → acciones habilitadas

| Estado (`currentStatus`) | Acciones habilitadas |
|--------------------------|----------------------|
| `idle` | Iniciar |
| `in-progress` | Pausar, Finalizar |
| `paused` | Iniciar (reanuda), Finalizar |
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
| Iniciar | `Color.Success` | `idle` (inicia) o `paused` (reanuda) |
| Pausar | `Color.Warning` | `in-progress` |
| Finalizar | `Color.Secondary` | `in-progress` o `paused` |

> "Iniciar" unifica `start` y `resume`: si el estado es `idle` ejecuta `start`; si es
> `paused` ejecuta `resume`.

## Card (`WorkerCard.razor`)

Todo visible por defecto (sin click-para-expandir):

1. Nombre, rol y chip de estado (color según tabla).
2. Resumen: `Trabajado: <b>…</b> · Pausado: <b>…</b>`.
3. Mini progressbar (si hay timeline).
4. Lista de timeline (o "Sin actividad hoy.").
5. Los 3 botones de acción (siempre visibles, deshabilitados según estado).

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

## Fichaje rápido (`QuickClockPanel` + `QuickClockDialog`)

- Panel con 3 botones grandes (todos habilitados): Iniciar (Play), Pausar (Pause), Finalizar (Stop).
- Al pulsar un botón se abre un **`MudDialog`** "Introduce tu PIN de trabajador" con un input numérico
  (`maxlength=4`, `AutoFocus`, auto-submit al 4º dígito).
- El dialog tiene **2 modos** (según el parámetro `Action`):
  - **Fichaje** (`Action` = `start`/`pause`/`end`): título "Fichaje rápido"; ejecuta `quick-clock`.
  - **Login** (`Action` = `null`): título "Logarse"; solo recoge el PIN y cierra (sin lógica todavía).
- Flujo del PIN:
  - **PIN mal formado / no encontrado** (400): muestra el mensaje, **borra el input y reenfoca**.
  - **Acción inválida** (200 `success=false`): muestra el estado actual y **botones con las acciones correctas**
    (pulsar uno reenvía `quick-clock` con el mismo PIN + esa acción).
  - **Éxito** (200 `success=true`): cierra el modal + snackbar "Fichaje realizado correctamente".
- Vocabulario de 3 acciones: `start` (= iniciar/reanudar), `pause`, `end`.

## Acceso / Login

- Botón "Login" (tarjeta "Acceso") abre el `QuickClockDialog` en modo login ("Logarse").
- Las cards de trabajadores están **ocultas por ahora** (se reintroducirán al implementar la vista detallada).

## PINs de prueba (en memoria)

- `1111` → María García · `2222` → Ana López · `3333` → Carlos Ruiz.

## Pendientes

- Hash del PIN con BCrypt (hoy el PIN viaja y se guarda en claro).
- Seguridad (JWT + cookie).
