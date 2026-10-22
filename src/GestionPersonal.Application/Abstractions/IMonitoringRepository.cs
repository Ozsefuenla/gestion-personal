using GestionPersonal.Application.Monitoring;

namespace GestionPersonal.Application.Abstractions;

public interface IMonitoringRepository
{
    Task<PlatformOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExceptionListItem>> ListExceptionsAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ErrorRatePoint>> GetErrorRateSeriesAsync(CancellationToken cancellationToken = default);
}
