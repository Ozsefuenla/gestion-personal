using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Centers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class CenterLoginTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CenterLoginTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSession()
    {
        var response = await _client.PostAsJsonAsync("/api/centers/login", new { username = "admin", password = "admin" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var session = await response.Content.ReadFromJsonAsync<CenterSessionResponse>();
        Assert.NotNull(session);
        Assert.Equal(1, session.CenterId);
        Assert.Equal("Peluquería Sol", session.CenterName);
        Assert.Equal("admin", session.Username);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/centers/login", new { username = "admin", password = "incorrecta" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/centers/login", new { username = "noexiste", password = "admin" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyFields_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/centers/login", new { username = "", password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
