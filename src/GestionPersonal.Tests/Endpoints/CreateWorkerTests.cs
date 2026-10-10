using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class CreateWorkerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CreateWorkerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostWorker_WithValidRequest_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Laura Pérez",
            pin = "0421",
            role = "worker"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>();

        Assert.NotNull(worker);
        Assert.True(worker.Id > 0);
        Assert.Equal("Laura Pérez", worker.FullName);
        Assert.Equal("worker", worker.Role);
    }

    [Fact]
    public async Task PostWorker_WithEmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "",
            pin = "0421",
            role = "worker"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostWorker_WithInvalidRole_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Laura",
            pin = "0421",
            role = "supervisor"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostWorker_WithInvalidPin_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Laura",
            pin = "12ab",
            role = "worker"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostWorker_ThenGet_ReturnsWorkerInList()
    {
        var fullName = "Nueva Trabajadora";

        await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = fullName,
            pin = "1111",
            role = "worker"
        });

        var workers = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers");

        Assert.NotNull(workers);
        Assert.Contains(workers, worker => worker.FullName == fullName);
    }
}
