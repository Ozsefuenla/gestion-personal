using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class TodaySummaryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TodaySummaryTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetToday_WhenIdle_ReturnsEmptySummary()
    {
        var workerId = TestWorkerIds.New();

        var summary = await _client.GetFromJsonAsync<TodaySummaryResponse>($"/api/time-entries/today?workerId={workerId}");

        Assert.NotNull(summary);
        Assert.Equal("idle", summary.CurrentStatus);
        Assert.Null(summary.StartedAt);
        Assert.Null(summary.CurrentPausedAt);
        Assert.Equal(TimeSpan.Zero, summary.WorkedTime);
        Assert.Equal(TimeSpan.Zero, summary.PausedTime);
        Assert.Equal(0, summary.PauseCount);
        Assert.Equal(0, summary.CompletedEntries);
        Assert.Empty(summary.Timeline);
    }

    [Fact]
    public async Task GetToday_WhenInProgress_ReturnsWorkingStatus()
    {
        var workerId = TestWorkerIds.New();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        var summary = await _client.GetFromJsonAsync<TodaySummaryResponse>($"/api/time-entries/today?workerId={workerId}");

        Assert.NotNull(summary);
        Assert.Equal("in-progress", summary.CurrentStatus);
        Assert.NotNull(summary.StartedAt);
        Assert.Null(summary.CurrentPausedAt);
        Assert.True(summary.WorkedTime >= TimeSpan.Zero);
        Assert.Contains(summary.Timeline, s => s.Type == "working");
    }

    [Fact]
    public async Task GetToday_WhenPaused_ReturnsPausedStatus()
    {
        var workerId = TestWorkerIds.New();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        var summary = await _client.GetFromJsonAsync<TodaySummaryResponse>($"/api/time-entries/today?workerId={workerId}");

        Assert.NotNull(summary);
        Assert.Equal("paused", summary.CurrentStatus);
        Assert.NotNull(summary.CurrentPausedAt);
        Assert.True(summary.PausedTime >= TimeSpan.Zero);
        Assert.Contains(summary.Timeline, s => s.Type == "paused");
    }

    [Fact]
    public async Task GetToday_FullCycle_ReturnsTimelineWithMultiplePauses()
    {
        var workerId = TestWorkerIds.New();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });

        var summary = await _client.GetFromJsonAsync<TodaySummaryResponse>($"/api/time-entries/today?workerId={workerId}");

        Assert.NotNull(summary);
        Assert.Equal("finished", summary.CurrentStatus);
        Assert.Equal(2, summary.PauseCount);
        Assert.Equal(1, summary.CompletedEntries);
        Assert.Equal(new[] { "working", "paused", "working", "paused", "working" }, summary.Timeline.Select(t => t.Type));
    }

    [Fact]
    public async Task GetToday_WithEmptyWorkerId_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/time-entries/today?workerId=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
