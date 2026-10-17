# Reglas de UI del frontend — base compartida (Blazor + MudBlazor)

Documenta las reglas de interfaz comunes a todos los roles (fichaje, estados,
formatos y timeline). Las reglas específicas del **trabajador logueado** están en
`docs/0004-frontend-ui-worker.md`.

## Framework y estructura

- Blazor Web App (.NET 10, interactividad Server) + MudBlazor.
- Proyecto `GestionPersonal.Web` en `src/` (monolito desplegable).
- Carpetas:
  - `Components/` → `Pages/LandingPage.razor`, `Pages/Dashboard.razor` (dashboard),
    `WorkerCard.razor` (card), `QuickClockPanel.razor` (panel de fichaje rápido),
    `QuickClockDialog.razor` (modal de PIN), `DonutProgress.razor` (donut),
    `TimeControlPanel.razor` (control horario).
  - `Services/` → utilitarios de presentación (`TimeControlPdfExporter`, formatos).
  - Los DTOs son los `record` de respuesta de `Application` (no hay `Models/` duplicado).

## Conexión con la capa de aplicación

- La UI inyecta los servicios de `Application`: `ICentersService`, `IWorkersService`,
  `ITimeEntriesService` e `IVacationsService`.
- Las llamadas son **in-process** (sin HTTP, sin CORS, sin `Api:BaseUrl`).
- `Program.cs` registra `AddApplication()` + `AddInfrastructure()` y `ICurrentCenter`
  (implementado por `CookieCurrentCenter` sobre la cookie del centro).
- Operaciones usadas por el panel:
  - `IWorkersService.GetStatusesAsync` (estado + resumen + timeline de todos los trabajadores).
  - `ICentersService.ListAsync` (centros; se usa el primero como demo).
  - `IWorkersService.ListByCenterAsync(centerId)` (selector de trabajadores).
  - `IWorkersService.LoginAsync`, `ITimeEntriesService.QuickClockAsync` y
    `ITimeEntriesService.AddManualAsync` (fichaje).
- El contrato HTTP histórico (para una futura API) está en `docs/0001-api-design.md`.

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
- Carga la lista de trabajadores del centro (`IWorkersService.ListByCenterAsync`) y
  los muestra como botones con su nombre (`Icons.Material.Filled.Person`).
- Al pinchar un trabajador se cierra devolviendo el `WorkerResponse` (`id`, `fullName`)
  seleccionado, y se abre el dialog de PIN (`QuickClockDialog`) con ese `WorkerId`.

## Fichaje rápido (`QuickClockPanel` + `QuickClockDialog`)

- Panel con 3 botones grandes (todos habilitados): Iniciar (Play), Pausar (Pause), Finalizar (Stop).
- Al pulsar un botón se abre primero el **selector de trabajadores** (`WorkerSelectorDialog`) y,
  tras elegir uno, el **`MudDialog`** "Introduce tu PIN de trabajador" con un input numérico
  (`maxlength=4`, `AutoFocus`, auto-submit al 4º dígito).
- El dialog tiene **2 modos** (según el parámetro `Action`):
  - **Fichaje** (`Action` = `start`/`pause`/`end`): título "Fichaje rápido"; ejecuta `quick-clock`
    con `{ workerId, pin, action }`.
  - **Login** (`Action` = `null`): título "Logarse"; llama `IWorkersService.LoginAsync`
    y devuelve el `WorkerStatusResponse`.
- Flujo del PIN:
  - **PIN mal formado / no coincide** (error `Validation`): muestra el mensaje, **borra el input y reenfoca**.
  - **Trabajador inexistente** (error `NotFound`): muestra el mensaje.
  - **Acción inválida** (`success=false`): muestra el estado actual y **botones con las acciones correctas**
    (pulsar uno reenvía `quick-clock` con el mismo `workerId` + `pin` + esa acción).
  - **Éxito** (`success=true`): cierra el modal + snackbar "Fichaje realizado correctamente".
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

- CSP (diferido por la complejidad de Blazor + Tailwind CDN).
- JWT del trabajador para fichaje en tablet.
- Persistencia SQL real.
