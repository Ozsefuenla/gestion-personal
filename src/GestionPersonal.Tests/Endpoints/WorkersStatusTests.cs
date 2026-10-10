using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class WorkersStatusTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorkersStatusTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWorkersStatus_ReturnsAllWorkers()
    {
        var statuses = await _client.GetFromJsonAsync<List<WorkerStatusResponse>>("/api/workers/status");

        Assert.NotNull(statuses);
        Assert.True(statuses.Count >= 3);
        Assert.All(statuses, s =>
        {
            Assert.False(string.IsNullOrWhiteSpace(s.CurrentStatus));
            Assert.Equal(TimeSpan.FromHours(8), s.DailyHours);
        });
    }

    [Fact]
    public async Task GetWorkersStatus_ReflectsStatusTransitions()
    {
        var workerId = await CreateWorkerAsync();

        var status = await GetStatusAsync(workerId);
        Assert.Equal("idle", status.CurrentStatus);
        Assert.Null(status.StartedAt);
        Assert.Equal(TimeSpan.Zero, status.WorkedTime);
        Assert.Empty(status.Timeline);

        await _client.PostAsJsonAsync("/api/time-entries/start", new { workerId });
        status = await GetStatusAsync(workerId);
        Assert.Equal("in-progress", status.CurrentStatus);
        Assert.NotNull(status.StartedAt);
        Assert.Equal("working", Assert.Single(status.Timeline).Type);

        await _client.PostAsJsonAsync("/api/time-entries/pause", new { workerId });
        status = await GetStatusAsync(workerId);
        Assert.Equal("paused", status.CurrentStatus);
        Assert.NotNull(status.CurrentPausedAt);
        Assert.Equal(new[] { "working", "paused" }, status.Timeline.Select(t => t.Type));

        await _client.PostAsJsonAsync("/api/time-entries/end", new { workerId });
        status = await GetStatusAsync(workerId);
        Assert.Equal("finished", status.CurrentStatus);
        Assert.Equal(new[] { "working", "paused" }, status.Timeline.Select(t => t.Type));
    }

    private async Task<int> CreateWorkerAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new { fullName = "Transition Worker", pin = "6001", role = "worker" });
        response.EnsureSuccessStatusCode();

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>()
            ?? throw new InvalidOperationException("No se pudo crear el trabajador.");

        return worker.Id;
    }

    private async Task<WorkerStatusResponse> GetStatusAsync(int workerId)
    {
        var statuses = await _client.GetFromJsonAsync<List<WorkerStatusResponse>>("/api/workers/status")
            ?? throw new InvalidOperationException("No se pudo obtener el estado de los trabajadores.");

        return statuses.Single(s => s.Id == workerId);
    }
}
