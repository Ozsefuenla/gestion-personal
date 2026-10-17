using System.Security.Claims;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
                new(ClaimTypes.NameIdentifier, session.CenterId.ToString()),
                new(ClaimTypes.Name, session.Username),
                new("centerId", session.CenterId.ToString()),
                new("centerName", session.CenterName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
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

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { ok = true });
        });

        return app;
    }
}

public sealed record CenterLoginRequest(string Username, string Password);
