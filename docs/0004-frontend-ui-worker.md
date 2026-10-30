# Reglas de UI del frontend — trabajador logueado

Documenta las reglas de interfaz que aplican cuando un **trabajador está logueado**
(card personal, donut, cambio de PIN, edición de marcajes y control horario).
Las reglas comunes (fichaje, estados, formatos, timeline) están en
`docs/0003-frontend-ui.md`.

> **Nota de arquitectura:** la API HTTP se fusionó en el monolito `GestionPersonal.Web`.
> Las rutas `/api/*` citadas abajo son el contrato histórico (ver `docs/0001-api-design.md`);
> hoy equivalen a operaciones de `Application/Services` ejecutadas in-process.

## Card personal (`WorkerCard.razor`)

Todo visible por defecto (sin click-para-expandir):

1. Nombre y chip de estado (color según tabla de `0003`).
2. Lista de timeline (o "Sin actividad hoy.").
3. Los 3 botones de acción (siempre visibles, deshabilitados según estado).
4. Grid **2x2** de botones de perfil (debajo de las acciones de fichaje):

| Botón | Icono | Comportamiento |
|-------|-------|----------------|
| Cambiar PIN | `Password` | abre `ChangePinDialog` |
| Control Horario | `Schedule` | muestra/oculta el panel `TimeControlPanel` debajo de la card |
| Ausencias | `EventBusy` | muestra/oculta el panel `AbsencesPanel` debajo de la card |
| Cerrar sesión | `Logout` | vuelve a la tarjeta de Login |

- La card se refresca tras cada acción con `GET /api/workers/{id}/status`.

## Donut (`DonutProgress.razor`)

- Anillo circular (SVG) con 2 segmentos por ratio: verde = % trabajado, ámbar = % pausado.
- Centro: estado actual ("En curso" / "En pausa" / "Finalizado" / "Sin iniciar").
- Sin actividad (`WorkedTime + PausedTime = 0`): anillo gris completo + centro "Sin iniciar".
- Debajo del donut: `Trabajado: … · Pausado: …`.
- Colores hardcodeados: verde `#43a047`, ámbar `#fb8c00`, gris `#e0e0e0`.
- Los valores SVG (`stroke-dasharray`/`stroke-dashoffset`) se formatean con `CultureInfo.InvariantCulture`
  (la coma decimal de es-ES rompe el SVG).
- Se muestra en el hueco de la izquierda cuando el trabajador está logueado (reemplaza al panel de Fichaje rápido).
- Si hay una **jornada anterior sin cerrar** (`StaleEntryDate`), el estado se calcula como
  `idle` ("Sin iniciar"): el contador "Trabajando" no arranca y el donut queda sin progreso,
  aunque exista una jornada abierta de un día anterior. La card muestra el aviso correspondiente.

## Finalizar jornada anterior (`CloseStaleDialog.razor`)

- Se abre al pulsar "Iniciar" cuando hay una jornada de un día anterior sin cerrar.
- Muestra el día sin cerrar y la **hora de inicio** de esa jornada.
- Pide la hora de finalización; la hora debe ser **posterior a la de inicio** (se bloquea
  en el cliente y se valida en el servidor con el error "La hora de final debe ser posterior
  al inicio de la jornada.").

## Cambiar PIN (`ChangePinDialog`)

- Dialog con 3 inputs: PIN actual, nuevo PIN y repetir nuevo PIN (numéricos, `maxlength=4`).
- Valida en el front: formato de 4 dígitos y que `nuevo == repetir`.
- Llama `POST /api/workers/{id}/change-pin` con `{ currentPin, newPin }`:
  - PIN actual incorrecto → 400 "El PIN actual no es correcto".
  - nuevo == actual → 400 "El nuevo PIN debe ser distinto".
  - Éxito → `204` + snackbar "PIN actualizado correctamente".

## Editar / Eliminar marcaje activo

- Solo se puede editar/borrar el **último tramo activo** (jornada en curso o en pausa).
- **Borrar** (papelera) → confirmación "¿Seguro de querer eliminar el marcaje {tipo} iniciado a las {hora}?"
  + Aceptar/Cancelar. Llama `DELETE /api/time-entries/active/{workerId}`.
- **Editar** (pincel) → `EditActiveDialog` con un `MudTimePicker` de hora de inicio. Si se cambia la hora
  → aviso "¿Quieres ajustar el anterior evento al nuevo horario cambiado?" + Confirmar/Cancelar.
  Llama `PATCH /api/time-entries/active/{workerId}` con `{ start }`.
- Tras cada cambio se refresca el estado del worker.

## Añadir marcaje manual (`AddMarkingDialog.razor`)

- El botón **"Añadir marcaje"** (icono `Add`) está en la fila de acciones de la card personal
  (`WorkerCard`), a la derecha del todo, siempre habilitado. Abre el modal **"Añadir marcaje"**.
- El modal tiene:
  - **Tipo** (`MudSelect`): Iniciar / Pausa / Finalizar.
  - **Fecha** (`MudDatePicker`): por defecto hoy, con `MaxDate = hoy` (no se permiten fechas futuras).
  - **Hora de inicio** / **Hora de final** (`MudTimePicker`), mostradas según el tipo:

    | Tipo | Hora inicio | Hora final |
    |------|-------------|------------|
    | Iniciar | requerida | oculta |
    | Pausa | requerida | opcional |
    | Finalizar | oculta | requerida |

- Validación en el front: tipo y fecha obligatorios, hora requerida según tipo, y para Pausa
  con hora final → **hora final > hora inicio**.
- Al **Añadir** → `POST /api/time-entries/manual` con `{ workerId, type, date, startTime?, endTime? }`.
  Reusa la máquina de estados (Iniciar abre jornada, Pausa pausa la abierta, Finalizar la cierra):
  - `start` → 409 si ya hay una jornada abierta o si el horario solapa con un marcaje existente.
  - `pause` → 409 si no hay jornada en curso o ya está en pausa.
  - `end` → 409 si no hay jornada abierta; 400 si la hora de final no es posterior al inicio.
- Tras añadir el marcaje se refresca la card (estado y timeline).

## Control Horario (`TimeControlPanel.razor`)

Panel que se muestra **debajo de la card personal** al pulsar "Control Horario" (toggle).

### Filtro de mes/año y resumen

- Arriba del listado: `MudSelect` de **mes** (12 meses, en español) y `MudSelect` de **año**
  (desde el año actual hasta `actual - 5`).
- Al cambiar cualquiera de los dos se recarga el resumen (`GET /api/time-entries/monthly`).
- Botón **"Exportar"** (icono `FileDownload`), solo visual por ahora (`Disabled`).
- A la derecha de Exportar, recuadro de **resumen mensual**:
  - **Previstas**: `8h × días laborables (L-V)` desde el día 1 del mes **hasta hoy** (0 si el mes es futuro).
  - **Trabajadas**: suma de lo trabajado del mes.
  - **Diferencia**: `trabajadas − previstas`, en **minutos** con signo (`0 min` / `+15 min` / `-15 min`).

### Listado diario

- Una fila por cada día del mes (siempre el total de días del mes: 28–31).
- Fecha en formato completo `dd/MM/yyyy`.
- Según el tipo de día:
  - **No laborable (S/D)**: texto "Día no laborable" (sin barra).
  - **Laborable sin datos** (`HasEntries == false`): texto "Día sin datos registrados" (sin barra).
    Aun así, cuenta como `0` trabajadas en el resumen (refleja el faltante en la Diferencia).
  - **Laborable con datos**, de izquierda a derecha: fecha → `SegmentedBar` (ver `0003`) →
    `Previstas: 8 h` → `Trabajadas: 8 h / 7 h 45 m` → `Diferencia: ±N min` → **ojo** (`Visibility`).

### Barra segmentada (`SegmentedBar.razor`)

- Barra horizontal sobre las **8 horas** asignadas (`DailyHours`), con 3 colores:
  - **Verde** `#43a047` = trabajado.
  - **Ámbar** `#fb8c00` = pausado.
  - **Gris** `#e0e0e0` = tiempo no consumido (`max(0, 8h − trabajado − pausado)`).
- Si trabajado + pausado supera las 8h se recorta (sin gris).
- Todas las barras del listado tienen el **mismo ancho**: las columnas de texto (`Previstas`,
  `Trabajadas`, `Diferencia`) usan ancho fijo (`flex: none` + `white-space: nowrap`) y la barra
  ocupa el hueco restante (`flex: 1 1 0%` + `min-width: 0`).

### Detalle del día (`DayDetailDialog.razor`)

- El **ojo** abre un `MudDialog` con el título "Detalle — dd/MM/yyyy" y:
  - La `SegmentedBar` del día.
  - La **lista de tramos** (como debajo del donut): "Trabajando HH:mm" / "Pausa HH:mm".
  - La línea final de estado:
    - Jornada **abierta** (`HasOpenEntry`) → "En curso" (verde, sin hora).
    - Jornada **terminada** → "Finalizado HH:mm" (azul) con la hora de salida.
  - `Previstas`, `Trabajadas` y `Diferencia`.

### Horas diarias y delta

- Cada worker tiene un campo `DailyHours` (objetivo diario; seed inicial 8h) expuesto en `WorkerStatus`.
- La **diferencia** (por día y en el resumen) es `trabajadas − previstas`, en **minutos** con signo:
  - 8h30 trabajadas → `+30 min` (verde).
  - 7h45 trabajadas → `-15 min` (rojo).
- El texto "Trabajadas" usa **horas y minutos** (`8 h` o `7 h 45 m`); "Previstas" solo horas (`8 h`).

### Días de ausencia

- Un día cubierto por una ausencia **aprobada** se muestra como chip **"Ausencia - Vacaciones"
  / "Ausencia - Enfermedad" / "Ausencia - Otro"** (en lugar de la barra segmentada).
- Los días de ausencia **no cuentan** en el cálculo de horas: quedan fuera de "Previstas"
  (no penalizan) y no aportan "Trabajadas".
- Solo las ausencias **aprobadas** marcan el día; las pendientes no afectan al control horario.

## Ausencias (`AbsencesPanel.razor` + `RequestAbsenceDialog.razor`)

Panel que se muestra **debajo de la card personal** al pulsar "Ausencias" (toggle), en el
mismo hueco que el Control Horario. Carga `GET /api/vacations?workerId={id}`.

### Listado

- Cabecera "Ausencias" + botón **"Solicitar nueva ausencia"** (y, para el admin, **"Gestionar
  Ausencias"**).
- Una fila por ausencia con estado **Pendiente**, **Aprobado** o **Denegado** (las eliminadas
  no se listan), ordenadas por fecha de inicio **descendente**:
  - Fecha de inicio y fin (`dd/MM/yyyy`, o una sola fecha si coinciden).
  - Chip de estado: **Pendiente** (ámbar) / **Aprobado** (verde) / **Denegado** (rojo).
  - Tipo: Vacaciones / Enfermedad / Otro.
  - **X** (icono `Close`) para eliminar, con confirmación previa.
- Estado vacío: "No tienes ausencias registradas."

### Permisos de borrado (botón X)

- Ausencia **pendiente** → X visible siempre (el trabajador borra la suya).
- Ausencia **aprobada** → X visible solo si `Worker.Role == "admin"`.
- Ausencia **denegada** → sin X (no es eliminable).
- `DELETE /api/vacations/{id}` aplica borrado lógico; `403` si una ausencia aprobada la
  intenta borrar un trabajador que no es admin.

### Solicitar nueva ausencia (`RequestAbsenceDialog`)

- **Tipo**: select (Vacaciones / Enfermedad / Otro).
- **Fecha de inicio**: `MudDatePicker`.
- **Fecha de final**: `MudDatePicker` **deshabilitado** hasta elegir la fecha de inicio; al
  elegirla se habilita y se precarga con la misma fecha de inicio. `MinDate` bloquea fechas
  anteriores al inicio.
- Validación: fin ≥ inicio (mensaje de error si no se cumple) y solape con otra ausencia
  pendiente/aprobada (error del servidor `409`).
- Al **Aceptar** → `POST /api/vacations` con `{ workerId, startDate, endDate, type }`; queda
  en estado `Pendiente` a la espera de que el admin la apruebe. Si quien la solicita es
  `admin`, queda **aprobada** directamente (no pasa por pendiente). Botones Aceptar/Cancelar.

## Gestión de ausencias del admin (`ManageAbsencesDialog.razor`)

Solo disponible cuando `Worker.Role == "admin"`.

- Si hay ausencias **pendientes** de cualquier trabajador del centro, el botón "Ausencias" de
  la card muestra un **badge rojo** con el número de pendientes, y el botón "Gestionar
  Ausencias" del panel aparece **remarcado** (color ámbar + badge).
- **"Gestionar Ausencias"** abre un `MudDialog` con `MudTabs`: una tab por cada **trabajador
  no-admin** del centro (el propio admin no aparece).
  - Cada tab (nombre del worker) muestra un **punto rojo** (`BadgeDot`) si ese worker tiene
    ausencias pendientes.
  - Dentro de cada tab: todas las ausencias del worker (fecha, estado, tipo) y, sobre las
    **pendientes**, botones **"Aprobar"** y **"Denegar"**.
- `PATCH /api/vacations/{id}/approve` → estado `approved`; `PATCH /api/vacations/{id}/reject`
  → estado `rejected`. Ambas recargan el listado y refrescan el badge de pendientes.
- La restricción de rol es de **frontend** (la API autentica por centro, no por trabajador).
