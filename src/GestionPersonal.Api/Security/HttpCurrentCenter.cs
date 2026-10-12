using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.Api.Security;

public sealed class HttpCurrentCenter : ICurrentCenter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentCenter(IHttpContextAccessor httpContextAccessor)
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
