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

### Filtro de mes/año

- Arriba del listado: `MudSelect` de **mes** (12 meses, en español) y `MudSelect` de **año**
  (desde el año actual hasta `actual - 5`).
- Al cambiar cualquiera de los dos se recarga el resumen (`GET /api/time-entries/monthly`).
- A la derecha de los selects, botón **"Exportar"** (icono `FileDownload`), solo visual por ahora
  (`Disabled`).

### Listado diario

- Una fila por cada día del mes (siempre el total de días del mes: 28–31).
- Cada fila, de izquierda a derecha:
  1. **Número de día** (peso semibold).
  2. **Contenido del día**:
     - **Sin datos** (`HasEntries == false`): texto "Día sin datos registrados", sin barra ni delta.
     - **Con datos**: `MudProgressLinear` (verde) cuyo llenado es `trabajado / horasDiarias` (cap 100%),
       el texto "Tiempo total trabajado: `Xh Ym`" y el **delta** en minutos.
  3. **Botón de detalle**: icono de **ojo** (`Visibility`), sin lógica (se implementa más adelante).

### Horas diarias y delta

- Cada worker tiene un campo `DailyHours` (objetivo diario; seed inicial 8h) expuesto en `WorkerStatus`.
- El **delta** a la derecha de la barra es la diferencia entre lo trabajado y el objetivo, en **minutos**:
  - 8h30 trabajadas → `+30` (verde).
  - 7h45 trabajadas → `-15` (rojo).
- El texto "Tiempo total trabajado" usa **horas y minutos** (sin segundos): `8h 30m`.
