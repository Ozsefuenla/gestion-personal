using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class EditDeleteActiveTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EditDeleteActiveTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task EditActive_WhenInProgress_Succeeds()
    {
        var workerId = await CreateWorkerAsync("4001");
        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });

        var newStart = DateTimeOffset.UtcNow.AddMinutes(-10);
        var response = await _client.PatchAsJsonAsync($"/api/time-entries/active/{workerId}", new { start = newStart });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task EditActive_WhenPaused_Succeeds()
    {
        var workerId = await CreateWorkerAsync("4002");
        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await Task.Delay(100);
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        await Task.Delay(100);

        var newStart = DateTimeOffset.UtcNow.AddMilliseconds(-50);
        var response = await _client.PatchAsJsonAsync($"/api/time-entries/active/{workerId}", new { start = newStart });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task EditActive_WithFutureStart_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("4003");
        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });

        var futureStart = DateTimeOffset.UtcNow.AddMinutes(10);
        var response = await _client.PatchAsJsonAsync($"/api/time-entries/active/{workerId}", new { start = futureStart });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditActive_WithNoOpenEntry_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("4004");

        var response = await _client.PatchAsJsonAsync($"/api/time-entries/active/{workerId}", new { start = DateTimeOffset.UtcNow });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteActive_WhenPaused_ReturnsToInProgress()
    {
        var workerId = await CreateWorkerAsync("4005");
        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });

        var response = await _client.DeleteAsync($"/api/time-entries/active/{workerId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var status = await GetStatusAsync(workerId);
        Assert.Equal("in-progress", status.CurrentStatus);
    }

    [Fact]
    public async Task DeleteActive_WhenInProgressNoPauses_ReturnsIdle()
    {
        var workerId = await CreateWorkerAsync("4006");
        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });

        var response = await _client.DeleteAsync($"/api/time-entries/active/{workerId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var status = await GetStatusAsync(workerId);
        Assert.Equal("idle", status.CurrentStatus);
    }

    [Fact]
    public async Task DeleteActive_WithNoOpenEntry_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("4007");

        var response = await _client.DeleteAsync($"/api/time-entries/active/{workerId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<int> CreateWorkerAsync(string pin)
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new { fullName = $"Worker {pin}", pin = pin, role = "worker" });
        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>();

        return worker!.Id;
    }

    private async Task<WorkerStatusResponse> GetStatusAsync(int workerId)
    {
        var status = await _client.GetFromJsonAsync<WorkerStatusResponse>($"/api/workers/{workerId}/status");

        return status!;
    }
}
