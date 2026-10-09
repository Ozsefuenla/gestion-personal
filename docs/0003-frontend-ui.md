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

1. Nombre y chip de estado (color según tabla).
2. Lista de timeline (o "Sin actividad hoy.").
3. Los 3 botones de acción (siempre visibles, deshabilitados según estado).

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
- **Editar/borrar** solo el último tramo **activo** (jornada en curso o en pausa): iconos de pincel (editar) y papelera (borrar).
- Debajo del listado: botón **"+ Añadir marcaje"** (solo visual por ahora).

## Donut (`DonutProgress.razor`)

- Anillo circular (SVG) con 2 segmentos por ratio: verde = % trabajado, ámbar = % pausado.
- Centro: estado actual ("En curso" / "En pausa" / "Finalizado" / "Sin iniciar").
- Sin actividad (`WorkedTime + PausedTime = 0`): anillo gris completo + centro "Sin iniciar".
- Debajo del donut: `Trabajado: … · Pausado: …`.
- Colores hardcodeados: verde `#43a047`, ámbar `#fb8c00`, gris `#e0e0e0`.
- Los valores SVG (`stroke-dasharray`/`stroke-dashoffset`) se formatean con `CultureInfo.InvariantCulture` (la coma decimal de es-ES rompe el SVG).
- Se muestra en el hueco de la izquierda cuando el trabajador está logueado (reemplaza al panel de Fichaje rápido).

## Fichaje rápido (`QuickClockPanel` + `QuickClockDialog`)

- Panel con 3 botones grandes (todos habilitados): Iniciar (Play), Pausar (Pause), Finalizar (Stop).
- Al pulsar un botón se abre un **`MudDialog`** "Introduce tu PIN de trabajador" con un input numérico
  (`maxlength=4`, `AutoFocus`, auto-submit al 4º dígito).
- El dialog tiene **2 modos** (según el parámetro `Action`):
  - **Fichaje** (`Action` = `start`/`pause`/`end`): título "Fichaje rápido"; ejecuta `quick-clock`.
  - **Login** (`Action` = `null`): título "Logarse"; llama `POST /api/workers/login` y devuelve el `WorkerStatus`.
- Flujo del PIN:
  - **PIN mal formado / no encontrado** (400): muestra el mensaje, **borra el input y reenfoca**.
  - **Acción inválida** (200 `success=false`): muestra el estado actual y **botones con las acciones correctas**
    (pulsar uno reenvía `quick-clock` con el mismo PIN + esa acción).
  - **Éxito** (200 `success=true`): cierra el modal + snackbar "Fichaje realizado correctamente".
- Vocabulario de 3 acciones: `start` (= iniciar/reanudar), `pause`, `end`.

## Acceso / Login y card personal

- Botón "Login" (tarjeta "Acceso") abre el `QuickClockDialog` en modo login ("Logarse").
- Al loguear, se **oculta el panel de Fichaje rápido** y en su hueco se muestra el **donut**; la tarjeta "Acceso" se reemplaza por la card personal.
- La card se refresca tras cada acción con `GET /api/workers/{id}/status`.
- Botones de perfil (grid **2x2**, debajo de las acciones de fichaje):

| Botón | Icono | Comportamiento |
|-------|-------|----------------|
| Cambiar PIN | `Password` | abre `ChangePinDialog` |
| Control Horario | `Schedule` | visual (sin lógica) |
| Ausencias | `EventBusy` | visual (sin lógica) |
| Cerrar sesión | `Logout` | vuelve a la tarjeta de Login |

## Cambiar PIN (`ChangePinDialog`)

- Dialog con 3 inputs: PIN actual, nuevo PIN y repetir nuevo PIN (numéricos, `maxlength=4`).
- Valida en el front: formato de 4 dígitos y que `nuevo == repetir`.
- Llama `POST /api/workers/{id}/change-pin` con `{ currentPin, newPin }`:
  - PIN actual incorrecto → 400 "El PIN actual no es correcto".
  - nuevo == actual → 400 "El nuevo PIN debe ser distinto".
  - Éxito → `204` + snackbar "PIN actualizado correctamente".

## Editar / Eliminar marcaje activo

- Solo se puede editar/borrar el **último tramo activo** (jornada en curso o en pausa).
- **Borrar** (papelera) → confirmación "¿Seguro de querer eliminar el marcaje {tipo} iniciado a las {hora}?" + Aceptar/Cancelar. Llama `DELETE /api/time-entries/active/{workerId}`.
- **Editar** (pincel) → `EditActiveDialog` con un `MudTimePicker` de hora de inicio. Si se cambia la hora → aviso "¿Quieres ajustar el anterior evento al nuevo horario cambiado?" + Confirmar/Cancelar. Llama `PATCH /api/time-entries/active/{workerId}` con `{ start }`.
- Tras cada cambio se refresca el estado del worker.

## PINs de prueba (en memoria)

- `1111` → María García · `2222` → Ana López · `3333` → Carlos Ruiz.

## Pendientes

- Hash del PIN con BCrypt (hoy el PIN viaja y se guarda en claro).
- Seguridad (JWT + cookie).
