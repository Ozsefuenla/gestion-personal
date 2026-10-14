# Reglas de UI del frontend — admin

Documenta las reglas de interfaz específicas del **admin** del centro (rol `admin`).
Las reglas comunes están en `docs/0003-frontend-ui.md` y las del trabajador logueado
en `docs/0004-frontend-ui-worker.md`.

## Marcajes de hoy (`TodayWorkersPanel.razor`)

Panel que aparece **debajo del donut** (columna izquierda del dashboard `/app`), solo
cuando el trabajador logueado tiene `Role == "admin"`.

- Lista una fila por cada **trabajador no-admin** del centro (el propio admin no aparece).
- Por fila, sin ausencia: **nombre**, **estado actual** (chip En curso / En pausa /
  Finalizado / Sin iniciar), **Trabajadas** (`Xh Ym`) y **Diferencia** (`±N min`,
  verde/rojo) respecto al objetivo diario (`DailyHours`).
- Si el trabajador tiene una **ausencia aprobada hoy**, la fila muestra solo **nombre** +
  chip **"Ausencia - {Motivo}"** (Vacaciones / Enfermedad / Otro); se omiten el estado,
  las Trabajadas y la Diferencia.
- Los datos salen de `GET /api/workers/status` (ya devuelve el `WorkedTime`/`PausedTime`
  de hoy de todos los trabajadores del centro) y de `GET /api/vacations/center` para las
  ausencias aprobadas que cubren el día. **No hay endpoint nuevo.**
- **En directo**: se refresca automáticamente cada **30 s** con un `PeriodicTimer`
  (cancelado en `DisposeAsync`). No es una vista estática.

## Ver horarios trabajadores (`WorkersTimeControlDialog.razor`)

Solo disponible cuando `Worker.Role == "admin"`.

- En la cabecera del panel **Control Horario** (`TimeControlPanel.razor`) aparece el botón
  **"Ver horarios trabajadores"** (icono `People`).
- Abre un `MudDialog` con `MudTabs`: una tab por cada **trabajador no-admin** del centro
  (el admin no aparece), ordenado por nombre.
- **Filtro Mes/Año a nivel de diálogo** (arriba), que aplica a todas las tabs a la vez.
- Cada tab muestra el resumen mensual del trabajador (Previstas/Trabajadas/Diferencia) y el
  listado diario, reutilizando el componente compartido `MonthlyTimeControlView`.
- Los datos de cada trabajador se cargan con `GET /api/time-entries/monthly?workerId&year&month`.

### Componente compartido (`MonthlyTimeControlView.razor`)

- Extrae el **resumen mensual + listado diario** que antes vivía dentro de
  `TimeControlPanel.razor`, para reutilizarlo tanto en el Control Horario propio como en
  cada tab del diálogo del admin.
- Recibe `DailyHours`, `Summary`, `Year` y `Month` y renderiza: resumen
  (Previstas/Trabajadas/Diferencia) y las filas diarias (fecha → `SegmentedBar` →
  Previstas/Trabajadas/Diferencia → ojo de detalle).

## Exportar a PDF (`TimeControlPdfExporter`)

Disponible para **todos los roles**.

- Generación server-side con **QuestPDF** (paquete `QuestPDF`, licencia Community).
- Servicio `Services/TimeControlPdfExporter.cs`: construye el PDF a partir de
  `TimeControlExportWorker` (nombre, año, mes, objetivo diario y resumen mensual).
- **Contenido del PDF**: Nombre del trabajador, Mes, Año y el resumen completo del mes
  (tabla por día: Fecha, Previstas, Trabajadas, Diferencia; días de ausencia / no
  laborables / sin datos como texto) más el total trabajado y la diferencia mensual.
- **Descarga**: JS interop `downloadFileFromStream` (definido en `App.razor`) con
  `DotNetStreamReference`.

### Puntos de exportación

| Dónde | Qué exporta |
|-------|-------------|
| Botón **"Exportar"** del Control Horario propio (`TimeControlPanel`) | PDF de un solo trabajador para el mes/año seleccionados |
| Botón **"Exportar PDF"** del diálogo `WorkersTimeControlDialog` (admin) | PDF con todos los trabajadores no-admin del mes/año seleccionados |

- El botón queda deshabilitado mientras no haya datos cargados (`Summary == null`).
- Nombre de archivo: `control-horario-{año}-{mes}.pdf` (propio) y
  `control-horario-trabajadores-{año}-{mes}.pdf` (admin).

## Permisos

- La restricción "solo admin ve los marcajes de hoy y los horarios de los trabajadores" es
  de **frontend** (la API autentica por centro, no por trabajador; no hay identidad de
  trabajador en el JWT). Documentado en `0001`/`0004`.
- El export a PDF no tiene restricción de rol (cualquier trabajador exporta su propio
  control horario; el admin además exporta el de todos).
