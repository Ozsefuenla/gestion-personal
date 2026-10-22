using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Monitoring;

namespace GestionPersonal.TestSupport.Fakes;

public sealed class FakeMonitoringRepository : IMonitoringRepository
{
    public PlatformOverviewResponse Overview { get; set; } =
        new(0, 0, 0, 0, 0, 0, 0, 0, []);

    public List<ExceptionListItem> Exceptions { get; } = [];

    public List<ErrorRatePoint> Series { get; } = [];

    public int? LastLimit { get; private set; }

    public int? LastOffset { get; private set; }

    public Task<PlatformOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Overview);

    public Task<IReadOnlyList<ExceptionListItem>> ListExceptionsAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        LastLimit = limit;
        LastOffset = offset;

        IReadOnlyList<ExceptionListItem> page = Exceptions
            .Skip(offset)
            .Take(limit)
            .ToList();

        return Task.FromResult(page);
    }

    public Task<IReadOnlyList<ErrorRatePoint>> GetErrorRateSeriesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ErrorRatePoint> series = Series.ToList();

        return Task.FromResult(series);
    }
}
