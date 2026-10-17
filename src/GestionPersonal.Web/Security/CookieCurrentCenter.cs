using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.Web.Security;

public sealed class CookieCurrentCenter : ICurrentCenter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieCurrentCenter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? CenterId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirst("centerId")?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
