# 0007 — Seguridad (OWASP Top 10, Web y API)

Documenta la estrategia y el estado de seguridad del proyecto, basada en el
**OWASP Top 10 (2021)** para el frontend/Web y el **OWASP API Security Top 10 (2023)**
para la API. Cada item se marca con su estado y la mitigación aplicada o prevista.

## Alcance

- Web (`GestionPersonal.Web`): Blazor Server + MudBlazor, login del centro con cookie.
- API (`GestionPersonal.Api`): Minimal APIs, persistencia en memoria (sin ORM/SQL aún).
- No se cubre infraestructura (red, hosting) más allá de lo indicado.

## Decisiones de diseño

- **Contraseñas/PINs**: BCrypt (`IPasswordHasher` + `BcryptPasswordHasher`, `BCrypt.Net-Next`).
  El PIN del trabajador es de 4 dígitos (entropía baja: 10.000 combinaciones); BCrypt
  ralentiza el offline, pero la protección real ante fuerza bruta es el rate limiting +
  futuro lockout.
- **Sesión del centro**: cookie HttpOnly (`SameSite=Lax`, `SecurePolicy=SameAsRequest`,
  7 días con `SlidingExpiration`). Para las llamadas a la API, la cookie lleva el JWT
  como claim `jwt`; el Web lo reenvía como `Authorization: Bearer`.
- **JWT**: HS256 (`Microsoft.AspNetCore.Authentication.JwtBearer`), claims `centerId`,
  `centerName`, `username` y role `center`. Secreto en `Jwt:Key` (dev en
  `appsettings.Development.json`; en prod usar user-secrets/entorno).
- **Rate limiting**: `AddRateLimiter` (fixed window por IP) en los endpoints de login.
- **Lockout del PIN**: `ILoginAttemptTracker` (en memoria): 5 fallos bloquean al trabajador
  15 minutos.
- **Headers**: `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`. CSP queda
  pendiente por la complejidad de Blazor + Tailwind CDN (Fase 3).

## Estado actual (resumen)

| Control | Estado |
|---------|--------|
| Contraseña del centro (BCrypt) | ✅ |
| PIN del trabajador (BCrypt) | ✅ |
| Cookie HttpOnly + antiforgery | ✅ |
| Rate limiting en login (centro y trabajador) | ✅ |
| Headers de seguridad básicos | ✅ |
| Logging de intentos de login fallidos | ✅ |
| JWT / autenticación de la API | ✅ |
| Scoping por centro (BOLA/IDOR) | ✅ |
| Lockout por trabajador | ✅ |
| CSP | ❌ |

## Mapeo OWASP Top 10 (2021) — Web

| # | Riesgo | Estado | Mitigación |
|---|--------|--------|------------|
| A01 | Broken Access Control | ✅ | `/app` con `[Authorize]` + cookie; endpoints de API con `RequireAuthorization` + scoping por centro |
| A02 | Cryptographic Failures | ✅ | PIN y contraseña con BCrypt; HTTPS + HSTS en prod |
| A03 | Injection | ✅ | Persistencia en memoria (sin SQL). Al pasar a SQL: parámetros, nunca concatenación |
| A04 | Insecure Design | ✅ | Rate limiting en login + lockout del PIN por trabajador |
| A05 | Security Misconfiguration | ⚠️ | Headers básicos añadidos; detalle de error solo en Dev; CSP pendiente (→ Fase 3) |
| A06 | Vulnerable Components | ⚠️ | Revisar `dotnet list package --vulnerable` de forma periódica |
| A07 | Identification & Auth Failures | ✅ | Cookie HttpOnly + JWT + rate limiting + lockout del PIN |
| A08 | Software & Data Integrity | ⚠️ | Antiforgery en Web; verificar integridad de dependencias |
| A09 | Security Logging | ⚠️ | Logging de fallos de login (centro y trabajador). Ampliar a eventos de autorización |
| A10 | SSRF | ✅ | `ApiClient` apunta a `Api:BaseUrl` de config (origen controlado) |

## Mapeo OWASP API Security Top 10 (2023) — API

| # | Riesgo | Estado | Mitigación |
|---|--------|--------|------------|
| API1 | BOLA (Object Level Authorization) | ✅ | `GetByIdAndCenterAsync` + `WorkerScope` verifican que el recurso pertenece al centro |
| API2 | Broken Authentication | ✅ | JWT HS256 por centro; `RequireAuthorization` en todos los endpoints |
| API3 | BOPLA (Property Level Authorization) | ⚠️ | Commands como `record` con campos explícitos + FluentValidation; revisar en Fase 3 |
| API4 | Unrestricted Resource Consumption | ⚠️ | Rate limiting en login; considerar límite global |
| API5 | BFLA (Function Level Authorization) | ✅ | Todos los endpoints requieren centro autenticado (claim `centerId` + role `center`) |
| API6 | Sensitive Business Flows | ✅ | Fichajes protegidos con lockout del PIN + scoping |
| API7 | SSRF | ✅ | La API no consume URLs externas |
| API8 | Security Misconfiguration | ⚠️ | Headers/CORS explícito; detalle de error solo en Dev |
| API9 | Improper Inventory | ⚠️ | Documentar/versionar endpoints; inventario en el doc de API |
| API10 | Unsafe Consumption | ✅ | Sin consumo de APIs de terceros |

## Roadmap

- **Fase 1 (hecho)**: BCrypt del PIN, rate limiting en login, headers de seguridad,
  logging de intentos fallidos.
- **Fase 2 (hecho)**: JWT por centro en la API + scoping por centro en todos los endpoints
  (BOLA/BFLA), lockout del PIN por trabajador.
- **Fase 3**: CSP, BOPLA, CORS explícito, versionado/inventario de API, escaneo de dependencias.

## Referencias

- OWASP Top 10 (2021): https://owasp.org/Top10/
- OWASP API Security Top 10 (2023): https://owasp.org/API-Security/
- ASP.NET Core rate limiting: https://learn.microsoft.com/aspnet/core/performance/rate-limit
