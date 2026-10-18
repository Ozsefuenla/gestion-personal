using System.Security.Claims;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Enums;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionPersonal.Web.Security;

public sealed class CookieCurrentUser : ICurrentUser, ICurrentCenter
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public CookieCurrentUser(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int? SubjectId => ParseInt(Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

    public int? CenterId => ParseInt(Principal?.FindFirst("centerId")?.Value);

    public UserRole? Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirst(ClaimTypes.Role)?.Value, ignoreCase: false, out var role)
            ? role
            : null;

    public int? WorkerId => Role == UserRole.CenterWorker ? SubjectId : null;

    public bool MustChangePassword => Principal?.HasClaim("mustChangePassword", "true") ?? false;

    private ClaimsPrincipal? Principal
    {
        get
        {
            try
            {
                var task = _authenticationStateProvider.GetAuthenticationStateAsync();

                if (task.IsCompletedSuccessfully && task.Result.User.Identity?.IsAuthenticated == true)
                {
                    return task.Result.User;
                }
            }
            catch (InvalidOperationException)
            {
            }

            return _httpContextAccessor.HttpContext?.User;
        }
    }

    private static int? ParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}
