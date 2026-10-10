using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class QuickClockTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public QuickClockTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task QuickClock_WithInvalidPinFormat_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin = "12ab", action = "start" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task QuickClock_WithUnknownPin_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin = "5555", action = "start" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task QuickClock_StartWhenIdle_Succeeds()
    {
        var pin = await CreateWorkerAsync("7001");

        var response = await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin, action = "start" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<QuickClockResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task QuickClock_StartWhenInProgress_ReturnsAvailableActions()
    {
        var pin = await CreateWorkerAsync("7002");

        await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin, action = "start" });
        var response = await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin, action = "start" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<QuickClockResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("in-progress", result.CurrentStatus);
        Assert.Equal(new[] { "pause", "end" }, result.AvailableActions);
    }

    [Fact]
    public async Task QuickClock_StartActsAsResume_WhenPaused()
    {
        var pin = await CreateWorkerAsync("7003");

        await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin, action = "start" });
        await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin, action = "pause" });

        var response = await _client.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin, action = "start" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<QuickClockResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    private async Task<string> CreateWorkerAsync(string pin)
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new { fullName = $"Worker {pin}", pin, role = "worker" });
        response.EnsureSuccessStatusCode();
        return pin;
    }
}
