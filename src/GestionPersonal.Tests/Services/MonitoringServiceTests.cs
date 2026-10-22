using GestionPersonal.Application.Monitoring;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class MonitoringServiceTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public MonitoringServiceTests(ApplicationFixture fixture) : base(fixture)
    {
        MonitoringStore.Overview = new PlatformOverviewResponse(0, 0, 0, 0, 0, 0, 0, 0, []);
        MonitoringStore.Exceptions.Clear();
        MonitoringStore.Series.Clear();
    }

    [Fact]
    public async Task GetOverview_AsSuperAdmin_ReturnsOverview()
    {
        User.ActAsSuperAdmin();
        MonitoringStore.Overview = new PlatformOverviewResponse(
            TotalCenters: 3,
            ActiveCenters: 2,
            ActiveWorkers: 10,
            ClockInsToday: 4,
            PendingAbsences: 1,
            ExceptionsLast24h: 2,
            RequestsLast24h: 100,
            ErrorsLast24h: 5,
            SeverityBreakdown: [new SeverityCount("Info", 2)]);

        var result = await Monitoring.GetOverviewAsync();

        AssertSuccess(result);
        Assert.Equal(3, result.Value.TotalCenters);
        Assert.Equal(2, result.Value.ActiveCenters);
        Assert.Equal(10, result.Value.ActiveWorkers);
        Assert.Single(result.Value.SeverityBreakdown);
    }

    [Fact]
    public async Task GetOverview_AsCenterAdmin_ReturnsForbidden()
    {
        User.ActAsCenterAdmin();

        var result = await Monitoring.GetOverviewAsync();

        AssertFailure(result, "Forbidden");
    }

    [Fact]
    public async Task ListExceptions_AsSuperAdmin_ClampsLimitToMaximum()
    {
        User.ActAsSuperAdmin();
        MonitoringStore.Exceptions.Add(NewException(1));

        var result = await Monitoring.ListExceptionsAsync(limit: 1000);

        AssertSuccess(result);
        Assert.Single(result.Value);
        Assert.Equal(200, MonitoringStore.LastLimit);
        Assert.Equal(0, MonitoringStore.LastOffset);
    }

    [Fact]
    public async Task ListExceptions_AsSuperAdmin_AppliesLimitAndOffset()
    {
        User.ActAsSuperAdmin();

        for (var id = 1; id <= 5; id++)
        {
            MonitoringStore.Exceptions.Add(NewException(id));
        }

        var result = await Monitoring.ListExceptionsAsync(limit: 2, offset: 2);

        AssertSuccess(result);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(2, MonitoringStore.LastLimit);
        Assert.Equal(2, MonitoringStore.LastOffset);
    }

    [Fact]
    public async Task ListExceptions_AsCenterAdmin_ReturnsForbidden()
    {
        User.ActAsCenterAdmin();

        var result = await Monitoring.ListExceptionsAsync();

        AssertFailure(result, "Forbidden");
    }

    [Fact]
    public async Task GetErrorRateSeries_AsSuperAdmin_ReturnsPoints()
    {
        User.ActAsSuperAdmin();
        MonitoringStore.Series.Add(new ErrorRatePoint(DateTimeOffset.UtcNow, Requests: 10, Errors: 1));

        var result = await Monitoring.GetErrorRateSeriesAsync();

        AssertSuccess(result);
        Assert.Single(result.Value);
        Assert.Equal(10, result.Value[0].Requests);
        Assert.Equal(1, result.Value[0].Errors);
    }

    [Fact]
    public async Task GetErrorRateSeries_AsCenterAdmin_ReturnsForbidden()
    {
        User.ActAsCenterAdmin();

        var result = await Monitoring.GetErrorRateSeriesAsync();

        AssertFailure(result, "Forbidden");
    }

    private static ExceptionListItem NewException(long id) =>
        new(
            id,
            DateTimeOffset.UtcNow,
            "System.InvalidOperationException",
            $"Error {id}",
            "/admin",
            "GET",
            500,
            "SuperAdmin",
            null);
}
