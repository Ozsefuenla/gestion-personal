using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class CentersEndpointsTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public CentersEndpointsTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCenters_ReturnsSeededCenters()
    {
        var centers = await _client.GetFromJsonAsync<List<CenterResponse>>("/api/centers");

        Assert.NotNull(centers);
        Assert.Contains(centers, c => c.Id == 1 && c.Name == "Peluquería Sol");
    }

    [Fact]
    public async Task GetCenterWorkers_ReturnsWorkersOfCenter()
    {
        var workers = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/centers/1/workers");

        Assert.NotNull(workers);
        Assert.Equal(3, workers.Count);
        Assert.All(workers, w => Assert.True(w.Id > 0));
    }

    [Fact]
    public async Task GetCenterWorkers_WithOtherCenter_ReturnsForbidden()
    {
        var response = await _client.GetAsync($"/api/centers/{TestWorkerIds.New()}/workers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
