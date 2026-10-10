using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class WorkersEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorkersEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWorkers_ReturnsOkWithSeededWorkers()
    {
        var response = await _client.GetAsync("/api/workers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var workers = await response.Content.ReadFromJsonAsync<List<WorkerResponse>>();

        Assert.NotNull(workers);
        Assert.Equal(3, workers.Count);
    }

    [Fact]
    public async Task GetWorkers_ReturnsWorkersWithExpectedFields()
    {
        var workers = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers");

        Assert.NotNull(workers);
        Assert.All(workers, worker =>
        {
            Assert.True(worker.Id > 0);
            Assert.False(string.IsNullOrWhiteSpace(worker.FullName));
            Assert.False(string.IsNullOrWhiteSpace(worker.Role));
        });
    }

    [Fact]
    public async Task GetWorkers_ReturnsSeededWorkerByName()
    {
        var workers = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers");

        Assert.NotNull(workers);
        Assert.Contains(workers, worker => worker.FullName == "María García");
        Assert.Contains(workers, worker => worker.Role == "admin");
    }
}
