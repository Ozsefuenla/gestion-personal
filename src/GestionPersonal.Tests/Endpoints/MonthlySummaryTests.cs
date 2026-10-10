using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class MonthlySummaryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MonthlySummaryTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMonthly_WithNoEntries_ReturnsAllDaysEmpty()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridNow();

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>(
            $"/api/time-entries/monthly?workerId={workerId}&year={year}&month={month}");

        Assert.NotNull(summary);
        Assert.Equal(year, summary.Year);
        Assert.Equal(month, summary.Month);
        Assert.Equal(TimeSpan.FromHours(8), summary.DailyTarget);
        Assert.Equal(DateTime.DaysInMonth(year, month), summary.Days.Count);
        Assert.All(summary.Days, d =>
        {
            Assert.Equal(TimeSpan.Zero, d.WorkedTime);
            Assert.Equal(TimeSpan.Zero, d.PausedTime);
            Assert.False(d.HasEntries);
            Assert.Empty(d.Timeline);
        });
    }

    [Fact]
    public async Task GetMonthly_AfterStartEnd_TodayHasWorkedTime()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridNow();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>(
            $"/api/time-entries/monthly?workerId={workerId}&year={year}&month={month}");

        Assert.NotNull(summary);
        var activeDay = summary.Days.Single(d => d.HasEntries);
        Assert.True(activeDay.WorkedTime > TimeSpan.Zero);
    }

    [Fact]
    public async Task GetMonthly_AfterFullCycle_TodayHasPausedTimeAndTimeline()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridNow();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>(
            $"/api/time-entries/monthly?workerId={workerId}&year={year}&month={month}");

        Assert.NotNull(summary);
        var activeDay = summary.Days.Single(d => d.HasEntries);
        Assert.True(activeDay.WorkedTime > TimeSpan.Zero);
        Assert.True(activeDay.PausedTime > TimeSpan.Zero);
        Assert.Contains(activeDay.Timeline, s => s.Type == "working");
        Assert.Contains(activeDay.Timeline, s => s.Type == "paused");
    }

    [Fact]
    public async Task GetMonthly_WithInvalidMonth_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync();
        var (year, _) = MadridNow();

        var response = await _client.GetAsync(
            $"/api/time-entries/monthly?workerId={workerId}&year={year}&month=13");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMonthly_WithUnknownWorker_ReturnsNotFound()
    {
        var (year, month) = MadridNow();

        var response = await _client.GetAsync(
            $"/api/time-entries/monthly?workerId={Guid.NewGuid()}&year={year}&month={month}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> CreateWorkerAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new { fullName = "Test Worker", pin = "9999", role = "worker" });

        response.EnsureSuccessStatusCode();

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>()
            ?? throw new InvalidOperationException("No se pudo crear el trabajador.");

        return worker.Id;
    }

    private static (int Year, int Month) MadridNow()
    {
        var zone = ResolveMadrid();
        var local = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        return (local.Year, local.Month);
    }

    private static TimeZoneInfo ResolveMadrid()
    {
        foreach (var id in new[] { "Europe/Madrid", "Romance Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new TimeZoneNotFoundException("No se encontró la zona horaria de Madrid.");
    }
}
