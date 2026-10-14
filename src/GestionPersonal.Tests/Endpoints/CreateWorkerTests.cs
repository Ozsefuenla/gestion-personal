using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class CreateWorkerTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public CreateWorkerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task PostWorker_WithValidRequest_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Laura Pérez",
            pin = "0421",
            role = "worker",
            centerId = 1
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
            role = "worker",
            centerId = 1
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
            role = "supervisor",
            centerId = 1
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
            role = "worker",
            centerId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostWorker_WithOtherCenter_ReturnsForbidden()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Laura",
            pin = "0421",
            role = "worker",
            centerId = TestWorkerIds.New()
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostWorker_ThenGet_ReturnsWorkerInList()
    {
        var fullName = "Nueva Trabajadora";

        await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = fullName,
            pin = "1111",
            role = "worker",
            centerId = 1
        });

        var workers = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers");

        Assert.NotNull(workers);
        Assert.Contains(workers, worker => worker.FullName == fullName);
    }

    [Fact]
    public async Task PostWorker_WithMustChangePin_ForcesChangeOnFirstLogin()
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Nuevo Con Cambio",
            pin = "7001",
            role = "worker",
            centerId = 1,
            mustChangePin = true
        });

        response.EnsureSuccessStatusCode();

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>();
        Assert.NotNull(worker);
        Assert.True(worker.MustChangePin);

        var login = await _client.PostAsJsonAsync("/api/workers/login", new { workerId = worker.Id, pin = "7001" });
        var status = await login.Content.ReadFromJsonAsync<WorkerStatusResponse>();
        Assert.True(status!.MustChangePin);

        var change = await _client.PostAsJsonAsync($"/api/workers/{worker.Id}/change-pin", new { currentPin = "7001", newPin = "7002" });
        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);

        var loginAgain = await _client.PostAsJsonAsync("/api/workers/login", new { workerId = worker.Id, pin = "7002" });
        var statusAgain = await loginAgain.Content.ReadFromJsonAsync<WorkerStatusResponse>();
        Assert.False(statusAgain!.MustChangePin);
    }
}
