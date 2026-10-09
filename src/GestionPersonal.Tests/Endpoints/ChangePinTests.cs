using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class ChangePinTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ChangePinTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ChangePin_WithCorrectCurrentPin_Succeeds()
    {
        var workerId = await GetWorkerIdAsync("2222");

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "2222", newPin = "9999" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/workers/login", new { pin = "9999" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task ChangePin_WithWrongCurrentPin_ReturnsBadRequest()
    {
        var workerId = await GetWorkerIdAsync("3333");

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "0000", newPin = "4444" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePin_WithSamePin_ReturnsBadRequest()
    {
        var workerId = await GetWorkerIdAsync("1111");

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "1111", newPin = "1111" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePin_WithInvalidFormat_ReturnsBadRequest()
    {
        var workerId = await GetWorkerIdAsync("1111");

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "1111", newPin = "12ab" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> GetWorkerIdAsync(string pin)
    {
        var login = await _client.PostAsJsonAsync("/api/workers/login", new { pin });
        var worker = await login.Content.ReadFromJsonAsync<WorkerStatusResponse>();

        return worker!.Id;
    }
}
