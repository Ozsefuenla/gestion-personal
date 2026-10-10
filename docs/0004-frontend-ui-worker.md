# Reglas de UI del frontend — trabajador logueado

Documenta las reglas de interfaz que aplican cuando un **trabajador está logueado**
(card personal, donut, cambio de PIN, edición de marcajes y control horario).
Las reglas comunes (fichaje, estados, formatos, timeline) están en
`docs/0003-frontend-ui.md`.

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
| Ausencias | `EventBusy` | visual (sin lógica) |
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

### Detalle del día (`DayDetailDialog.razor`)

- El **ojo** abre un `MudDialog` con el título "Detalle — dd/MM/yyyy" y:
  - La `SegmentedBar` del día.
  - La **lista de tramos** (como debajo del donut): "Trabajando HH:mm" / "Pausa HH:mm".
  - `Previstas`, `Trabajadas` y `Diferencia`.

### Horas diarias y delta

- Cada worker tiene un campo `DailyHours` (objetivo diario; seed inicial 8h) expuesto en `WorkerStatus`.
- La **diferencia** (por día y en el resumen) es `trabajadas − previstas`, en **minutos** con signo:
  - 8h30 trabajadas → `+30 min` (verde).
  - 7h45 trabajadas → `-15 min` (rojo).
- El texto "Trabajadas" usa **horas y minutos** (`8 h` o `7 h 45 m`); "Previstas" solo horas (`8 h`).
