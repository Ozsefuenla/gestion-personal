using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Monitoring;

public sealed class GetErrorRateSeriesQueryHandler
{
    private readonly IMonitoringRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetErrorRateSeriesQueryHandler(IMonitoringRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ErrorRatePoint>>> Handle(CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireSuperAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var series = await _repository.GetErrorRateSeriesAsync(cancellationToken);

        return Result<IReadOnlyList<ErrorRatePoint>>.Success(series);
    }
}
