using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class LoginTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LoginTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidPin_ReturnsWorkerStatus()
    {
        var response = await _client.PostAsJsonAsync("/api/workers/login", new { pin = "1111" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var worker = await response.Content.ReadFromJsonAsync<WorkerStatusResponse>();
        Assert.NotNull(worker);
        Assert.Equal("María García", worker.FullName);
        Assert.Equal("worker", worker.Role);
    }

    [Fact]
    public async Task Login_WithUnknownPin_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/workers/login", new { pin = "5555" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPinFormat_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/workers/login", new { pin = "12ab" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkerStatus_WithValidId_ReturnsWorkerStatus()
    {
        var login = await _client.PostAsJsonAsync("/api/workers/login", new { pin = "1111" });
        var worker = await login.Content.ReadFromJsonAsync<WorkerStatusResponse>();
        Assert.NotNull(worker);

        var response = await _client.GetAsync($"/api/workers/{worker.Id}/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var status = await response.Content.ReadFromJsonAsync<WorkerStatusResponse>();
        Assert.NotNull(status);
        Assert.Equal(worker.Id, status.Id);
        Assert.Equal("María García", status.FullName);
    }

    [Fact]
    public async Task GetWorkerStatus_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/workers/{TestWorkerIds.New()}/status");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
