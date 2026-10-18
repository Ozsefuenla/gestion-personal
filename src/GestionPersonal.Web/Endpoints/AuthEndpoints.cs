using System.Security.Claims;
using GestionPersonal.Application.Account;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Services;
using GestionPersonal.Application.Workers;
using GestionPersonal.Web.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;

namespace GestionPersonal.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            context.Response.Headers.CacheControl = "no-store";

            var tokens = antiforgery.GetAndStoreTokens(context);

            return Results.Ok(new { token = tokens.RequestToken });
        });

        app.MapPost("/auth/login", async (CenterLoginRequest request, ICentersService centers, IAntiforgery antiforgery, ILoggerFactory loggerFactory, HttpContext httpContext) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Json(new { error = "Token de seguridad inválido." }, statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await centers.LoginAsync(new CenterLoginCommand(request.Username, request.Password));

            if (result.IsFailure)
            {
                loggerFactory.CreateLogger("CentersLogin").LogWarning(
                    "Fallo de login de centro '{Username}': {Error}", request.Username, result.Error.Message);

                return Results.Json(new { error = result.Error.Message }, statusCode: StatusCodes.Status400BadRequest);
            }

            var session = result.Value;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
                new(ClaimTypes.Name, session.Username),
                new(ClaimTypes.Role, session.Role.ToString())
            };

            if (session.CenterId is { } centerId)
            {
                claims.Add(new Claim("centerId", centerId.ToString()));
                claims.Add(new Claim("centerName", session.CenterName));
            }

            if (session.MustChangePassword)
            {
                claims.Add(new Claim("mustChangePassword", "true"));
            }

            var identity = new ClaimsIdentity(claims, AuthSchemes.Center);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                AuthSchemes.Center,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            return Results.Ok(new { ok = true, mustChangePassword = session.MustChangePassword });
        }).RequireRateLimiting("login");

        app.MapPost("/auth/change-password", async (ChangeCenterPasswordRequest request, IAccountService account, IAntiforgery antiforgery, HttpContext httpContext) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Json(new { error = "Token de seguridad inválido." }, statusCode: StatusCodes.Status400BadRequest);
            }

            var role = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (role is not ("SuperAdmin" or "CenterAdmin" or "Center"))
            {
                return Results.Json(new { error = "No autorizado." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await account.ChangePasswordAsync(
                new ChangePasswordCommand(request.CurrentPassword, request.NewPassword));

            if (result.IsFailure)
            {
                return Results.Json(new { error = result.Error.Message }, statusCode: StatusCodes.Status400BadRequest);
            }

            var claims = httpContext.User.Claims
                .Where(claim => claim.Type != "mustChangePassword")
                .ToList();

            var refreshed = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthSchemes.Center));

            await httpContext.SignInAsync(
                AuthSchemes.Center,
                refreshed,
                new AuthenticationProperties { IsPersistent = false });

            return Results.Ok(new { ok = true });
        });

        app.MapPost("/auth/worker-login", async (WorkerLoginRequest request, IWorkersService workers, IAntiforgery antiforgery, HttpContext httpContext) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Json(new { error = "Token de seguridad inválido." }, statusCode: StatusCodes.Status400BadRequest);
            }

            var centerId = httpContext.User.FindFirst("centerId")?.Value;

            if (centerId is null)
            {
                return Results.Json(new { error = "No hay una sesión de centro activa." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await workers.LoginAsync(new LoginCommand(request.WorkerId, request.Pin));

            if (result.IsFailure)
            {
                return Results.Json(new { error = result.Error.Message }, statusCode: StatusCodes.Status400BadRequest);
            }

            var worker = result.Value;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, worker.Id.ToString()),
                new(ClaimTypes.Name, worker.FullName),
                new(ClaimTypes.Role, "CenterWorker"),
                new("centerId", centerId)
            };

            var identity = new ClaimsIdentity(claims, AuthSchemes.Worker);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                AuthSchemes.Worker,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            return Results.Ok(new { ok = true });
        }).RequireRateLimiting("login");

        app.MapPost("/auth/logout", async (HttpContext httpContext, IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Json(new { error = "Token de seguridad inválido." }, statusCode: StatusCodes.Status400BadRequest);
            }

            await httpContext.SignOutAsync(AuthSchemes.Worker);
            await httpContext.SignOutAsync(AuthSchemes.Center);
            return Results.Ok(new { ok = true });
        });

        app.MapPost("/auth/worker-logout", async (HttpContext httpContext, IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Json(new { error = "Token de seguridad inválido." }, statusCode: StatusCodes.Status400BadRequest);
            }

            await httpContext.SignOutAsync(AuthSchemes.Worker);
            return Results.Ok(new { ok = true });
        });

        return app;
    }
}

public sealed record CenterLoginRequest(string Username, string Password);

public sealed record WorkerLoginRequest(int WorkerId, string Pin);

public sealed record ChangeCenterPasswordRequest(string CurrentPassword, string NewPassword);
