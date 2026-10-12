# Reglas de UI del frontend — base compartida (Blazor + MudBlazor)

Documenta las reglas de interfaz comunes a todos los roles (fichaje, estados,
formatos y timeline). Las reglas específicas del **trabajador logueado** están en
`docs/0004-frontend-ui-worker.md`.

## Framework y estructura

- Blazor Web App (.NET 10, interactividad Server) + MudBlazor.
- Proyecto `GestionPersonal.Web` en `src/`.
- Carpetas:
  - `Models/` → DTOs del cliente (`WorkerStatus`, `TimelineSegment`, `QuickClockResult`, `MonthlySummary`).
  - `Services/` → `ApiClient` (cliente HTTP tipado).
  - `Components/` → `Pages/Home.razor` (página), `WorkerCard.razor` (card),
    `QuickClockPanel.razor` (panel de fichaje rápido), `QuickClockDialog.razor` (modal de PIN),
    `DonutProgress.razor` (donut), `TimeControlPanel.razor` (control horario).

## Conexión con la API

- `appsettings.json` → `"Api": { "BaseUrl": "http://localhost:5243" }`.
- `Program.cs` → `AddHttpClient<ApiClient>(...)` con la base de `Api:BaseUrl`.
- Las llamadas son server-to-server (Blazor Server), **sin CORS**.
- Endpoint del panel: `GET /api/workers/status` (devuelve workers + estado + resumen + timeline).
- Endpoint de centros: `GET /api/centers` (lista de centros; por ahora se usa el primero como demo).
- Endpoint del selector de trabajadores: `GET /api/centers/{centerId}/workers`.
- Endpoint de fichaje rápido: `POST /api/time-entries/quick-clock` (`{ workerId, pin, action }`).

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
- Se muestra solo la **hora de inicio** de cada tramo (la de fin se omite; coincide con el inicio del siguiente).
- Si `currentStatus == "finished"`: se agrega una línea "Finalizado" (círculo azul) con la hora de fin.
- Si el timeline está vacío: "Sin actividad hoy.".

## Selector de trabajadores (`WorkerSelectorDialog`)

- Antes de pedir el PIN, se abre un **`MudDialog`** "Selecciona tu trabajador".
- Carga la lista de trabajadores del centro (`GET /api/centers/{centerId}/workers`) y
  los muestra como botones con su nombre (`Icons.Material.Filled.Person`).
- Al pinchar un trabajador se cierra devolviendo el `WorkerSummary` (`id`, `fullName`)
  seleccionado, y se abre el dialog de PIN (`QuickClockDialog`) con ese `WorkerId`.

## Fichaje rápido (`QuickClockPanel` + `QuickClockDialog`)

- Panel con 3 botones grandes (todos habilitados): Iniciar (Play), Pausar (Pause), Finalizar (Stop).
- Al pulsar un botón se abre primero el **selector de trabajadores** (`WorkerSelectorDialog`) y,
  tras elegir uno, el **`MudDialog`** "Introduce tu PIN de trabajador" con un input numérico
  (`maxlength=4`, `AutoFocus`, auto-submit al 4º dígito).
- El dialog tiene **2 modos** (según el parámetro `Action`):
  - **Fichaje** (`Action` = `start`/`pause`/`end`): título "Fichaje rápido"; ejecuta `quick-clock`
    con `{ workerId, pin, action }`.
  - **Login** (`Action` = `null`): título "Logarse"; llama `POST /api/workers/login` con
    `{ workerId, pin }` y devuelve el `WorkerStatus`.
- Flujo del PIN:
  - **PIN mal formado / no coincide** (400): muestra el mensaje, **borra el input y reenfoca**.
  - **Trabajador inexistente** (404): muestra el mensaje.
  - **Acción inválida** (200 `success=false`): muestra el estado actual y **botones con las acciones correctas**
    (pulsar uno reenvía `quick-clock` con el mismo `workerId` + `pin` + esa acción).
  - **Éxito** (200 `success=true`): cierra el modal + snackbar "Fichaje realizado correctamente".
- Vocabulario de 3 acciones: `start` (= iniciar/reanudar), `pause`, `end`.

## Acceso / Login

- Botón "Login" (tarjeta "Acceso") abre primero el **selector de trabajadores** y, tras elegir
  uno, el `QuickClockDialog` en modo login ("Logarse").
- Al loguear, se **oculta el panel de Fichaje rápido** y se muestra la card personal del trabajador
  (ver `docs/0004-frontend-ui-worker.md`).

## PINs de prueba (en memoria)

- Centro demo `1` ("Peluquería Sol"). Trabajadores:
  - `1` → María García · PIN `1111`
  - `2` → Ana López · PIN `2222`
  - `3` → Carlos Ruiz · PIN `3333`
- El PIN es por trabajador (pueden repetirse); se valida contra el trabajador seleccionado.

## Pendientes

- Landing page (producto, features, login de centro) y seguridad (JWT + cookie).
- Hash del PIN con BCrypt (hoy el PIN viaja y se guarda en claro).
- Autenticación del centro (`CenterUser` con `username`/`password`); por ahora se opera
  contra un único centro demo.
