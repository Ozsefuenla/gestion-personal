using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class PauseTimeEntryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PauseTimeEntryTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostPause_WhenInProgress_ReturnsOk()
    {
        var workerId = Guid.NewGuid();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();

        Assert.NotNull(entry);
        Assert.Equal("paused", entry.Status);

        var pause = Assert.Single(entry.Pauses);
        Assert.NotEqual(default, pause.PausedAt);
        Assert.Null(pause.ResumedAt);
    }

    [Fact]
    public async Task PostPause_WhenNotStarted_ReturnsConflict()
    {
        var workerId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostPause_WhenAlreadyPaused_ReturnsConflict()
    {
        var workerId = Guid.NewGuid();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostPause_WithEmptyWorkerId_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId = Guid.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
