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
            email = "laura@example.com",
            role = "worker"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>();

        Assert.NotNull(worker);
        Assert.NotEqual(Guid.Empty, worker.Id);
        Assert.Equal("Laura Pérez", worker.FullName);
        Assert.Equal("laura@example.com", worker.Email);
        Assert.Equal("worker", worker.Role);
    }

    [Fact]
    public async Task PostWorker_WithExistingEmail_ReturnsConflict()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Otra María",
            email = "maria@example.com",
            role = "worker"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostWorker_WithEmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "",
            email = "x@example.com",
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
            email = "laura-rol@example.com",
            role = "supervisor"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostWorker_ThenGet_ReturnsWorkerInList()
    {
        var email = "nueva@example.com";

        await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Nueva Trabajadora",
            email = email,
            role = "worker"
        });

        var workers = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers");

        Assert.NotNull(workers);
        Assert.Contains(workers, worker => worker.Email == email);
    }
}
