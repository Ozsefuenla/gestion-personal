using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class StartTimeEntryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StartTimeEntryTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostStart_WithNoOpenEntry_ReturnsOk()
    {
        var workerId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();

        Assert.NotNull(entry);
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(workerId, entry.WorkerId);
        Assert.Equal("in-progress", entry.Status);
        Assert.NotEqual(default, entry.StartedAt);
    }

    [Fact]
    public async Task PostStart_WhenAlreadyStarted_ReturnsConflict()
    {
        var workerId = Guid.NewGuid();

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        var response = await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostStart_WithEmptyWorkerId_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId = Guid.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
