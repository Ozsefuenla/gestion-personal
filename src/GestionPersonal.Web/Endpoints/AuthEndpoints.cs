using System.Security.Claims;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;

namespace GestionPersonal.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (CenterLoginRequest request, ICentersService centers, ILoggerFactory loggerFactory, HttpContext httpContext) =>
        {
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
                new AuthenticationProperties { IsPersistent = true });

            return Results.Ok(new { ok = true });
        }).RequireRateLimiting("login");

        app.MapPost("/auth/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { ok = true });
        });

        return app;
    }
}

public sealed record CenterLoginRequest(string Username, string Password);
