using System.Security.Claims;
using GestionPersonal.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GestionPersonal.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (CenterLoginRequest request, ApiClient api, HttpContext httpContext) =>
        {
            try
            {
                var session = await api.LoginCenterAsync(request.Username, request.Password);

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
            }
            catch (HttpRequestException ex)
            {
                return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status400BadRequest);
            }
        });

        app.MapPost("/auth/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok(new { ok = true });
        });

        return app;
    }
}

public sealed record CenterLoginRequest(string Username, string Password);
