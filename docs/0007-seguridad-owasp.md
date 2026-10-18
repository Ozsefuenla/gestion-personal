# 0007 — Seguridad (OWASP Top 10, aplicación Web)

Documenta la estrategia y el estado de seguridad de la aplicación, basada en el
**OWASP Top 10 (2021)**. La antigua API HTTP se fusionó en el monolito: hoy no
existe superficie de API separada. El contrato HTTP histórico, conservado como
referencia para una futura re-extracción, está en `docs/0001-api-design.md` y
`docs/0001-api-openapi.yaml`.

## Alcance

- Web (`GestionPersonal.Web`): Blazor Server + MudBlazor, login del centro con cookie
  y servicios de aplicación in-process.
- Persistencia PostgreSQL con Dapper (SQL parametrizado, sin ORM).
- No se cubre infraestructura (red, hosting) más allá de lo indicado.

## Decisiones de diseño

- **Contraseñas/PINs**: BCrypt (`IPasswordHasher` + `BcryptPasswordHasher`, `BCrypt.Net-Next`).
  El PIN del trabajador es de 4 dígitos (entropía baja: 10.000 combinaciones); BCrypt
  ralentiza el ataque offline, pero la protección real ante fuerza bruta es el rate
  limiting + el lockout.
- **Sesión**: dos cookies HttpOnly (`SameSite=Lax`, `SecurePolicy=SameAsRequest`):
  `gp.center` (usuarios del centro y SuperAdmin; 7 días deslizantes) y `gp.worker`
  (sesión efímera de 8 h del trabajador). Un esquema de política (`AppScheme`)
  resuelve la identidad activa: si existe `gp.worker` se evalúa esa; si no, la del
  centro. No hay JWT.
- **Autorización de casos de uso**: los handlers resuelven centro y rol con
  `ICurrentUser` (`CookieCurrentUser`, que también implementa `ICurrentCenter`).
  `AccessControl` aplica `RequireSuperAdmin`, `RequireCenterAdmin`,
  `RequireKiosk` y `RequireSelfOrAdmin`; un `CenterWorker` solo opera sobre su
  propio `workerId`. Ver `0012-modelo-roles.md`.
- **Validación de entrada**: FluentValidation en los servicios de aplicación
  (`IRequestValidator`), antes de invocar el handler.
- **Rate limiting**: `AddRateLimiter` (fixed window por IP) en `/auth/login`.
- **Lockout del PIN**: `ILoginAttemptTracker` (en memoria): 5 fallos bloquean al trabajador
  15 minutos. El login de trabajador ocurre in-process, así que esta es su protección real.
- **Headers**: `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`. CSP queda
  pendiente por la complejidad de Blazor + Tailwind CDN (Fase 3).

## Estado actual (resumen)

| Control | Estado |
|---------|--------|
| Contraseña del centro (BCrypt) | ✅ |
| PIN del trabajador (BCrypt) | ✅ |
| Cookie HttpOnly + antiforgery | ✅ |
| Rate limiting en login de centro | ✅ |
| Lockout del PIN por trabajador | ✅ |
| Headers de seguridad básicos | ✅ |
| Logging de intentos de login fallidos | ✅ |
| Scoping por centro (BOLA/IDOR) | ✅ |
| Autorización por rol en servidor (BFLA) | ✅ |
| Scoping por worker en autoservicio | ✅ |
| CSP | ❌ |

## Mapeo OWASP Top 10 (2021) — Web

| # | Riesgo | Estado | Mitigación |
|---|--------|--------|------------|
| A01 | Broken Access Control | ✅ | `/app` con `[Authorize]`, `/admin` con `[Authorize(Roles = "SuperAdmin")]`; handlers con `AccessControl` y scoping por centro/worker |
| A02 | Cryptographic Failures | ✅ | PIN y contraseña con BCrypt; HTTPS + HSTS en prod |
| A03 | Injection | ✅ | Dapper con SQL parametrizado; nunca concatenación de entrada |
| A04 | Insecure Design | ✅ | Rate limiting en login + lockout del PIN por trabajador |
| A05 | Security Misconfiguration | ⚠️ | Headers básicos; detalle de error solo en Dev (`/Error`); CSP pendiente (→ Fase 3) |
| A06 | Vulnerable Components | ✅ | Escaneo `dotnet list package --vulnerable --include-transitive` sin vulnerabilidades (revisar periódicamente) |
| A07 | Identification & Auth Failures | ✅ | Cookie HttpOnly + rate limiting + lockout del PIN |
| A08 | Software & Data Integrity | ⚠️ | Antiforgery; verificar integridad de dependencias |
| A09 | Security Logging | ⚠️ | Logging de fallos de login (centro y trabajador). Ampliar a eventos de autorización |
| A10 | SSRF | ✅ | La aplicación no consume URLs externas |

## Sobre OWASP API Security Top 10

El mapeo de **OWASP API Security Top 10 (2023)** y el inventario de endpoints quedan
archivados en `docs/0001-api-design.md` / `docs/0001-api-openapi.yaml` como contrato
objetivo para una futura re-extracción de la API. Hoy no hay endpoints `/api/*` expuestos.

## Roadmap

- **Fase 1 (hecho)**: BCrypt del PIN, rate limiting en login, headers de seguridad,
  logging de intentos fallidos.
- **Fase 2 (hecho)**: scoping por centro en todos los casos de uso (BOLA/BFLA) y lockout
  del PIN por trabajador. El JWT de la antigua API se eliminó al fusionar la API en el Web;
  la sesión se apoya solo en la cookie.
- **Fase 3 (hecho parcial)**: escaneo de dependencias (sin vulnerables), BOPLA auditada.
  **CSP diferido** (complejidad Blazor + Tailwind).

## Referencias

- OWASP Top 10 (2021): https://owasp.org/Top10/
- OWASP API Security Top 10 (2023): https://owasp.org/API-Security/
- ASP.NET Core rate limiting: https://learn.microsoft.com/aspnet/core/performance/rate-limit
