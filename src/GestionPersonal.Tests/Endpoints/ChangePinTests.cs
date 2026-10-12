using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class ChangePinTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public ChangePinTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ChangePin_WithCorrectCurrentPin_Succeeds()
    {
        const int workerId = 2;

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "2222", newPin = "9999" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/workers/login", new { workerId, pin = "9999" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task ChangePin_WithWrongCurrentPin_ReturnsBadRequest()
    {
        const int workerId = 3;

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "0000", newPin = "4444" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePin_WithSamePin_ReturnsBadRequest()
    {
        const int workerId = 1;

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "1111", newPin = "1111" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePin_WithInvalidFormat_ReturnsBadRequest()
    {
        const int workerId = 1;

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "1111", newPin = "12ab" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
