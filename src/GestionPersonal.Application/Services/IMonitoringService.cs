using GestionPersonal.Application.Common;
using GestionPersonal.Application.Monitoring;

namespace GestionPersonal.Application.Services;

public interface IMonitoringService
{
    Task<Result<PlatformOverviewResponse>> GetOverviewAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ExceptionListItem>>> ListExceptionsAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ErrorRatePoint>>> GetErrorRateSeriesAsync(
        CancellationToken cancellationToken = default);
}
