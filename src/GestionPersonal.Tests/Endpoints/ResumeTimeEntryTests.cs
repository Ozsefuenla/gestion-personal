using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class ResumeTimeEntryTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public ResumeTimeEntryTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task PostResume_WhenPaused_ReturnsOk()
    {
        var workerId = await CreateWorkerAsync();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();

        Assert.NotNull(entry);
        Assert.Equal("in-progress", entry.Status);

        var pause = Assert.Single(entry.Pauses);
        Assert.NotNull(pause.ResumedAt);
        Assert.True(pause.ResumedAt.Value >= pause.PausedAt);
    }

    [Fact]
    public async Task PostResume_WhenNotStarted_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        var response = await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostResume_WhenInProgress_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostResume_WithEmptyWorkerId_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FullCycle_TracksMultiplePausesWithDates()
    {
        var workerId = await CreateWorkerAsync();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/resume", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();

        Assert.NotNull(entry);
        Assert.Equal("finished", entry.Status);
        Assert.Equal(2, entry.Pauses.Count);

        foreach (var pause in entry.Pauses)
        {
            Assert.True(pause.ResumedAt.HasValue);
            Assert.True(pause.ResumedAt.Value >= pause.PausedAt);
        }
    }

    private async Task<int> CreateWorkerAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new { fullName = "Test Worker", pin = "7777", role = "worker", centerId = 1 });
        response.EnsureSuccessStatusCode();

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>()
            ?? throw new InvalidOperationException("No se pudo crear el trabajador.");

        return worker.Id;
    }
}
