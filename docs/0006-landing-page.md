# 0006 — Landing page: copy, diseño y propuestas de venta

Contenido completo (en español) de la landing page del producto, pensado para
peluquerías, clínicas estéticas y centros de belleza. Incluye el copy listo para
publicar, los bloques de diseño sugeridos, las propuestas adicionales de venta y
el detalle técnico del componente de acceso (`CenterLoginDialog`).

> **Nota de arquitectura:** la API HTTP se fusionó en el monolito `GestionPersonal.Web`.
> Las rutas `/api/*` citadas abajo son el contrato histórico (ver `docs/0001-api-design.md`);
> hoy equivalen a operaciones de `Application/Services` ejecutadas in-process.

> **Marcas y placeholders.** El nombre de trabajo es **GestionPersonal**. Los
> textos usan `[placeholders]` que hay que reemplazar por datos reales antes de
> publicar (cifras de ahorro, precios, testimonios, logotipos). Recomiendo un
> nombre comercial más corto y recordable (por ejemplo *SalónTime*, *Bella* o
> *GestaSalón*), pero el copy funciona con cualquiera.

---

## 1. Hero

- **Titular (H1):** El control horario de tu centro de belleza, sin papel y sin errores.
- **Subtítulo:** Fichajes, vacaciones y ausencias de todo tu equipo desde el móvil,
  la tablet o el ordenador. Cumple el registro de jornada obligatorio y olvídate de
  las hojas de Excel y los grupos de WhatsApp.
- **CTA principal:** `Probar gratis 14 días`
- **CTA secundario:** `Solicitar demo`
- **Botón de cabecera:** `Acceso` (abre el modal de login del centro).
- **Microcopy de confianza (bajo el CTA):** Sin tarjeta de crédito. Configúralo en
  menos de [15] minutos.

---

## 2. Problema

Los dolores reales del sector:

1. **Fichajes en papel que nadie revisa.** Hojas que se pierden, horarios a mano y
   un encargado/a perdiendo tiempo reconstruyendo quién llegó a qué hora.
2. **Errores en nómina.** Horas mal anotadas se traducen en nóminas incorrectas y
   en disputas con el equipo a fin de mes.
3. **Inspecciones de trabajo.** Si llega una inspección, no tienes el registro de
   jornada a mano ni en un formato que sirva de prueba.
4. **Vacaciones por WhatsApp.** Solicitudes que se pierden en el chat, solapamientos
   de dos personas en la misma semana y quejas de "yo lo pedí primero".
5. **Sin visibilidad diaria.** No sabes de un vistazo quién está hoy en el centro,
   quién está de pausa o quién no ha llegado.

> Titular de la sección: **Gestionar un equipo no debería quitarte horas de trabajo.**

---

## 3. Solución

**GestionPersonal** centraliza el control horario y la gestión del equipo de tu
centro en una sola herramienta. Cada trabajador ficha con su PIN individual; tú
ves en tiempo real quién está, apruebas vacaciones y ausencias desde un calendario,
y exportas el registro de jornada cuando lo necesites. Todo desde el móvil, la
tablet o el ordenador, sin instalar nada en los equipos del personal.

> Titular de la sección: **Una sola herramienta para fichar, planificar y cumplir.**

---

## 4. Funcionalidades

Cada bloque con beneficio concreto:

### 4.1 Control de fichajes (entrada, salida y pausas)
El equipo ficha entrada, pausas y salida en segundos. Tú ves quién está trabajando,
quién está de pausa y quién no ha llegado, en tiempo real.
*Beneficio: dejas de perseguir hojas de papel y sabes la jornada real de cada persona.*

### 4.2 Gestión de vacaciones y ausencias
Solicitudes, aprobación y un calendario compartido. El equipo pide, tú apruebas o
rechazas, y todos ven el calendario actualizado.
*Beneficio: fin del WhatsApp para pedir vacaciones; sin solapamientos ni discusiones.*

### 4.3 Alta y baja de trabajadores
Da de alta a un trabajador nuevo o da de baja a quien se marcha en minutos, con su
PIN asociado y su histórico protegido.
*Beneficio: el equipo siempre está al día, y el registro histórico no se pierde.*

### 4.4 PIN individual por trabajador
Cada persona ficha con su PIN personal de 4 dígitos. Nadie puede fichar por otro.
*Beneficio: fichajes fiables y trazables a cada trabajador.*

### 4.5 Histórico de control horario para el trabajador
Cada trabajador consulta su propio histórico: qué días trabajó, cuántas horas y sus
pausas.
*Beneficio: transparencia y menos preguntas del tipo "¿cuántas horas llevo?".*

### 4.6 Fichaje desde móvil, tablet y escritorio
La interfaz es 100 % responsive y funciona igual en el móvil del trabajador, la
tablet de recepción o el ordenador de la trastienda.
*Beneficio: cada uno ficha desde el dispositivo que tenga más a mano.*

> Titular de la sección: **Todo lo que necesitas para gestionar tu equipo, en un solo sitio.**

---

## 5. Cómo funciona

Tres pasos:

1. **Crea tu centro y da de alta a tu equipo.** Configuras el centro y añades a tus
   trabajadores. Cada uno recibe su PIN individual.
2. **Tu equipo ficha con su PIN.** Entrada, pausas y salida desde el móvil, la tablet
   o el ordenador. Sin instalar nada.
3. **Gestiona y cumple.** Apruebas vacaciones, revisas las horas y exportas el
   registro de jornada cuando haga falta.

---

## 6. Seguridad y cumplimiento

- **PIN cifrado.** Los PIN se guardan cifrados (hash), nunca en texto plano.
- **Registro inalterable.** Los fichajes no se pueden editar ni borrar sin control:
  el histórico queda trazado para auditorías e inspecciones.
- **Registro de jornada (RD-ley 8/2019).** Cumple la obligación legal de registrar la
  jornada diaria de cada trabajador en España.
- **Copias de seguridad.** Copias de seguridad automáticas para no perder nunca los datos.
- **Control de accesos por roles.** El encargado/a y el trabajador ven y pueden hacer
  cosas distintas; cada rol tiene solo los permisos que necesita.

---

## 7. Multiplataforma

Móvil, tablet y escritorio, totalmente responsive.

- **Trabajadores:** fichan desde su móvil con su PIN.
- **Recepción:** tablet en modo kiosco para que el equipo fiche al entrar y salir.
- **Encargado/a:** gestiona vacaciones, ausencias y el registro desde el ordenador.

> Titular de la sección: **Ficha desde donde estés. Gestiona desde donde quieras.**

---

## 8. Prueba social

### Testimonios (placeholders)

> "Dejamos de anotar las horas a mano. Ahora las nóminas salen bien a la primera."
> — *[Nombre], propietaria de [Salón] en [Ciudad].*

> "La inspección nos pidió el registro de jornada y lo exportamos en un minuto."
> — *[Nombre], gerente de [Clínica] en [Ciudad].*

> "Mis empleadas fichan desde el móvil y yo apruebo las vacaciones desde casa."
> — *[Nombre], encargada de [Centro de belleza] en [Ciudad].*

### Logotipos (placeholders)

Fila de logotipos de clientes (en escala de grises): `[Logo 1] · [Logo 2] · [Logo 3] · [Logo 4] · [Logo 5]`

---

## 9. Precios

Planes orientativos (placeholders):

| Plan | Precio | Incluye |
|------|--------|---------|
| **Básico** | `[XX €/mes]` por centro | Fichajes con PIN, control horario, registro de jornada. Hasta `[X]` trabajadores. |
| **Profesional** | `[XX €/mes]` por centro | Todo lo de Básico + vacaciones y ausencias, calendario, exportación a informes. |
| **Enterprise** | `[A medida]` | Todo lo de Profesional + varios centros, roles avanzados, integración con nóminas y soporte prioritario. |

> Los 3 planes incluyen prueba gratuita de 14 días sin tarjeta de crédito.

---

## 10. FAQ

### 1. ¿Cumple con el registro de jornada obligatorio (RD-ley 8/2019)?
Sí. El registro diario de cada trabajador queda guardado de forma inalterable y lo
puedes exportar para presentarlo ante una inspección de trabajo.

### 2. ¿Cómo fichan los trabajadores? ¿Necesitan instalar algo?
No. Fichan con su PIN desde el móvil, la tablet o el ordenador, sin instalar ninguna
aplicación en su equipo.

### 3. ¿Qué pasa si un trabajador olvida su PIN?
El encargado/a puede restablecerlo desde su panel en segundos. El trabajador no
pierde su histórico.

### 4. ¿Puedo probar el software antes de pagar?
Sí. Ofrecemos una prueba gratuita de 14 días con todas las funcionalidades, sin
tarjeta de crédito.

### 5. ¿Mis datos están seguros? ¿Dónde se guardan?
Sí. Los PIN se guardan cifrados, el registro es inalterable y hacemos copias de
seguridad automáticas. El acceso se controla por roles.

### 6. ¿Puedo exportar el registro para una inspección de trabajo?
Sí. Generas un informe exportable con el registro de jornada de cualquier periodo,
listo para entregar.

### 7. ¿Sirve para varios centros o sucursales?
Sí. El plan Enterprise permite gestionar varios centros con datos separados y
usuarios propios por centro.

### 8. ¿Ofrecéis soporte y formación?
Sí. Soporte en español y un onboarding asistido para dejar el centro configurado y
al equipo fichando desde el primer día.

---

## 11. CTA final y footer

- **Titular:** Empieza hoy. Tu equipo fichando y tu registro al día, en menos de [15] minutos.
- **CTA:** `Probar gratis 14 días`
- **Sub-CTA:** `¿Prefieres verlo antes? Solicita una demo.`

### Footer

- Producto: Funcionalidades · Precios · Seguridad · Multiplataforma
- Empresa: Sobre nosotros · Contacto · Blog
- Legal: Aviso legal · Política de privacidad · Cookies · Términos de servicio
- Acceso: `Acceso` (login del centro)
- `© [Año] [Nombre de la empresa]. Todos los derechos reservados.`

---

## Diseño sugerido

### Paleta de colores

| Uso | Color | Hex |
|-----|-------|-----|
| Primario (acciones, CTA) | Violeta/plum, evoca belleza y sofisticación | `#5B3E8E` |
| Acento (destacados) | Rosa empolvado | `#E58FA4` |
| Éxito | Verde | `#2E9E6B` |
| Tinta (textos) | Casi negro azulado | `#1F1B2E` |
| Gris (texto secundario) | Gris medio | `#6B7280` |
| Fondo | Rosa/blanco muy suave | `#FAF7F9` |

### Tipografía

- **Titulares:** Poppins (o Montserrat) — geométrica, amable y de marca.
- **Cuerpo:** Inter (o Source Sans 3) — legible y neutra.

### Rejilla

- Rejilla de 12 columnas, ancho máximo `1200px`.
- Secciones con separación vertical generosa (`80–120px`).
- Hero centrado; funcionalidades en tarjetas (3 por fila en escritorio, 1 en móvil).
- Los CTA primarios siempre en el color primario; los secundarios en contorno (outline).

---

## Propuestas adicionales de venta

Seis (o más) elementos para nuevos compradores, con el perfil al que convencen:

1. **Prueba gratuita de 14/30 días.**
   El comprador quiere verlo funcionando antes de pagar. Baja la fricción de compra y
   deja que el propietario/a compruebe el ahorro real en su propio centro.

2. **Calculadora de ahorro de tiempo.**
   El comprador introduce cuántos trabajadores tiene y cuánto tarda hoy en cuadrar
   horas; la web le muestra el tiempo que ahorrará al mes. Convencen los datos
   concretos, no las promesas.

3. **Informes exportables para inspección laboral.**
   Resuelve el miedo concreto a una sanción. Convence al propietario/a y a su
   gestoría, que necesita el registro a mano y en formato válido.

4. **Integración con nóminas (Sage, A3, Holded).**
   Elimina el doble trabajo de pasar horas a la nómina. Convence al propietario/a y
   a la asesoría, que ya usan esas herramientas.

5. **App del trabajador con notificaciones.**
   El trabajador recibe avisos (recordatorio de fichar, solicitud de vacaciones
   aprobada). Convence al equipo y reduce el trabajo de "perseguir" al encargado/a.

6. **Modo kiosco con tablet en recepción.**
   Una tablet fija en recepción para que el equipo fiche al entrar y salir sin tocar
   su móvil. Convence a los centros que no quieren que el personal use el teléfono.

7. **Soporte en español y onboarding asistido.**
   El comprador no técnico teme "montar un software". Un onboarding guiado y soporte
   en su idioma disuelven esa barrera.

8. **Garantía de satisfacción.**
   Riesgo cero: si en el primer mes no le convence, se devuelve el dinero. Acelera la
   decisión de compra.

9. **Certificado de cumplimiento normativo.**
   Un documento que certifica que el producto cubre el registro de jornada (RD-ley
   8/2019). Da seguridad legal al comprador y a su gestoría.

---

## Componente de acceso (`CenterLoginDialog`)

El botón **"Acceso"** de la cabecera abre un modal (`MudDialog`) con el login del
centro (usuario + contraseña). El componente está en
`src/GestionPersonal.Web/Components/CenterLoginDialog.razor` (markup) y
`CenterLoginDialog.razor.cs` (code-behind).

Cumple: validación en tiempo real, mensajes de error claros, enlace "¿Olvidaste tu
contraseña?", cierre por botón X, tecla Escape y clic fuera, foco atrapado y ARIA
(todo heredado de `MudDialog`).

### Botón que lo activa (cabecera)

```razor
@inject IDialogService DialogService

<MudAppBar Elevation="1">
    <MudSpacer />
    <MudButton Variant="Variant.Outlined"
               Color="Color.Primary"
               StartIcon="@Icons.Material.Filled.Lock"
               OnClick="OpenAccessAsync">
        Acceso
    </MudButton>
</MudAppBar>

@code {
    private async Task OpenAccessAsync()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            BackdropClick = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<CenterLoginDialog>("Acceso al centro", options: options);
        var result = await dialog.Result;

        // result.Canceled => cerró sin entrar
        // result.Data is true => acceso correcto
    }
}
```

### Nota de estado (implementación)

El formulario de acceso (modal en la landing o página `/login`) valida contra el
servicio `ICentersService.LoginAsync` y firma una cookie HttpOnly en el Web. Mapeo
copy → estado actual:

| Afirmación de la landing | Estado actual en el código |
|--------------------------|----------------------------|
| PIN cifrado | Implementado: BCrypt en `PinHash` |
| Registro inalterable | Parcial: hay edición/borrado del último fichaje activo, trazable pero no append-only |
| Login del centro (usuario/contraseña) | Implementado: `ICentersService.LoginAsync` + cookie HttpOnly en el Web |
| Vacaciones y ausencias | Implementado: solicitud/aprobación/denegación |
| Integración con nóminas / informes exportables / app con notificaciones | No implementado (propuesta comercial) |
