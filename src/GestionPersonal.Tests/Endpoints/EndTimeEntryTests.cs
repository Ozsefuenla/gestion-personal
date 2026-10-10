using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class EndTimeEntryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndTimeEntryTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostEnd_WhenInProgress_ReturnsOk()
    {
        var workerId = TestWorkerIds.New();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();

        Assert.NotNull(entry);
        Assert.Equal("finished", entry.Status);
        Assert.NotNull(entry.EndedAt);
    }

    [Fact]
    public async Task PostEnd_WhenNotStarted_ReturnsConflict()
    {
        var workerId = TestWorkerIds.New();

        var response = await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostEnd_WhenPaused_ReturnsOk()
    {
        var workerId = TestWorkerIds.New();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();

        Assert.NotNull(entry);
        Assert.Equal("finished", entry.Status);
        Assert.NotNull(entry.EndedAt);

        var pause = Assert.Single(entry.Pauses);
        Assert.NotNull(pause.ResumedAt);
    }

    [Fact]
    public async Task PostEnd_WithEmptyWorkerId_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
