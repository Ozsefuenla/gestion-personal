using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestionPersonal.Application.Centers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public abstract class AuthenticatedApiTest : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    protected HttpClient _client = default!;

    protected AuthenticatedApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        var response = await _client.PostAsJsonAsync("/api/centers/login", new { username = "admin", password = "admin" });
        response.EnsureSuccessStatusCode();

        var session = await response.Content.ReadFromJsonAsync<CenterLoginResponse>()
            ?? throw new InvalidOperationException("No se pudo obtener la sesión.");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }
}
