using GestionPersonal.Application.Common;
using GestionPersonal.Application.Monitoring;

namespace GestionPersonal.Application.Services;

public sealed class MonitoringService : IMonitoringService
{
    private readonly GetPlatformOverviewQueryHandler _overviewHandler;
    private readonly ListExceptionsQueryHandler _exceptionsHandler;
    private readonly GetErrorRateSeriesQueryHandler _seriesHandler;

    public MonitoringService(
        GetPlatformOverviewQueryHandler overviewHandler,
        ListExceptionsQueryHandler exceptionsHandler,
        GetErrorRateSeriesQueryHandler seriesHandler)
    {
        _overviewHandler = overviewHandler;
        _exceptionsHandler = exceptionsHandler;
        _seriesHandler = seriesHandler;
    }

    public Task<Result<PlatformOverviewResponse>> GetOverviewAsync(CancellationToken cancellationToken = default) =>
        _overviewHandler.Handle(cancellationToken);

    public Task<Result<IReadOnlyList<ExceptionListItem>>> ListExceptionsAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default) =>
        _exceptionsHandler.Handle(new ListExceptionsQuery(limit, offset), cancellationToken);

    public Task<Result<IReadOnlyList<ErrorRatePoint>>> GetErrorRateSeriesAsync(
        CancellationToken cancellationToken = default) =>
        _seriesHandler.Handle(cancellationToken);
}
