# 0013 — Actualización en tiempo real de fichajes

Documenta cómo las vistas del mismo centro se refrescan al instante cuando un
fichaje cambia, sin polling.

## Contexto

El panel "Marcajes de hoy" del `CenterAdmin` y la vista personal del
`CenterWorker` muestran el estado de los fichajes. Antes, el panel del
administrador consultaba el estado cada 30 segundos con un `PeriodicTimer`, de
modo que un fichaje hecho en la tablet tardaba en reflejarse.

Blazor Server ya mantiene un circuito SignalR por cliente. Por eso no hace falta
un hub adicional para empujar cambios dentro de la misma aplicación: alcanza con
un bus de eventos en proceso que avise a los componentes suscritos.

## Diseño

- `ITimeEntryChangeNotifier` (`Application/Abstractions`) expone:
  - `Raise(int centerId)`
  - `IDisposable Subscribe(Action<int> handler)`
- `TimeEntryChangeNotifier` (`Application/Common`) es la implementación
  thread-safe, registrada como **singleton** en `AddApplication()`. Es C# puro:
  no depende de ASP.NET ni de SignalR.
- `TimeEntriesService` es el punto único por el que pasan todas las mutaciones
  (`Start`, `Pause`, `Resume`, `End`, `QuickClock`, `AddManual`, `EditActive`,
  `DeleteActive`). Tras cada mutación exitosa llama a `NotifyChanged()`, que
  publica el `centerId` obtenido de `ICurrentCenter`.
  - `QuickClock` solo publica cuando `QuickClockResponse.Success` es `true`,
    porque las respuestas con acciones no permitidas no modifican nada.
- El bus aísla las excepciones de cada suscriptor: una notificación fallida
  nunca rompe la operación de fichaje.

## Suscripciones en la UI

- `TodayWorkersPanel` (vista `CenterAdmin`): se suscribe al inicializar y, al
  recibir un evento de su mismo `centerId`, re-ejecuta `LoadAsync()` mediante
  `InvokeAsync`. Se elimina el `PeriodicTimer` de 30 s. El botón de refresco
  manual se conserva.
- `Dashboard` (vista `CenterWorker`): se suscribe para refrescar el estado del
  propio trabajador cuando un administrador edita o elimina un marcaje.

Reglas de Blazor que aplican:

- El bus es singleton y compartido entre circuitos. El suscriptor **debe** usar
  `InvokeAsync` para volver a su propio contexto de render.
- Cada componente desuscribe en `DisposeAsync` para evitar fugas.

## Ticking en vivo del tiempo trabajado

El estado del fichaje se refresca con el bus de eventos, pero el tiempo
trabajado se calcula en el servidor solo cuando se consulta. Para que las cifras
sumen minutos y segundos sin golpear la base cada segundo:

- `WorkerStatusResponse` incluye `ComputedAt`, el instante UTC (`now`) con el que
  se calcularon `WorkedTime` y `PausedTime`.
- `LiveTime.Compute(worker, utcNow)` (en `Application/Workers`) proyecta el tiempo
  a partir de la foto: mientras `CurrentStatus` es `in-progress` suma
  `utcNow - ComputedAt` al trabajado; mientras es `paused` lo suma al pausado; en
  `finished`/`idle` no suma. Un `elapsed` negativo (desfase de reloj) no resta.
- En el cliente, `DonutProgress` y `TodayWorkersPanel` mantienen un
  `PeriodicTimer` local de 1 segundo que solo hace `StateHasChanged`: no hay
  tráfico de red. Se formatea con `TimeFormatting.FormatLiveDuration`
  (`h/m/s`).
- El snapshot se reemplaza con cada mutación (event bus), de modo que la base de
  la proyección se corrige sola.

## Presentación en la UI

- `TodayWorkersPanel` es un grid de columnas fijas (`Trabajador`, `Estado`,
  `Trabajado`, `Restante`, `Pausado`) con `table-layout: fixed`,
  `white-space: nowrap` y `font-variant-numeric: tabular-nums`, para que el
  ticking no desplace nada. `Restante` = `jornada - trabajado` formateado en
  horas y minutos; si se supera la jornada se muestra `+Xh Ym` de exceso.
- `DonutProgress` representa el porcentaje trabajado sobre la jornada
  (`min(trabajado, jornada) / jornada`, tope 100%): el arco verde es lo
  completado y el gris de fondo lo faltante. En el centro se muestra el `%`. El
  bloque inferior es fijo: `Tiempo trabajando`, `Pausado` y `Restante`.

## Alcance y límites

- El filtro por `centerId` evita que un centro refresque vistas de otro.
- El estado vive en memoria del proceso: es válido para el despliegue actual de
  **una instancia**. Si en el futuro se escala horizontalmente, habrá que
  reemplazar el bus por un hub de SignalR con backplane (por ejemplo Redis).

## Pruebas

- `TimeEntryChangeNotifierTests`: alta/baja de suscripciones, múltiples handlers
  y aislamiento de excepciones.
- `TimeEntryChangeNotificationTests`: el servicio publica en mutaciones exitosas
  y no publica en conflictos o acciones no permitidas.
- `LiveTimeTests`: la proyección suma al trabajado en `in-progress`, al pausado
  en `paused`, no avanza en estados inactivos y no resta con desfase negativo.
