using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Monitoring;

public sealed class GetPlatformOverviewQueryHandler
{
    private readonly IMonitoringRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetPlatformOverviewQueryHandler(IMonitoringRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<PlatformOverviewResponse>> Handle(CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireSuperAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var overview = await _repository.GetOverviewAsync(cancellationToken);

        return overview;
    }
}
