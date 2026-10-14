using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestionPersonal.Application.Centers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public abstract class AuthenticatedApiTest : IAsyncLifetime
{
    private static readonly SemaphoreSlim LoginLock = new(1, 1);
    private static string? _cachedToken;

    protected readonly WebApplicationFactory<Program> _factory;

    protected HttpClient _client = default!;

    protected AuthenticatedApiTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    private async Task<string> GetTokenAsync()
    {
        if (_cachedToken is not null)
        {
            return _cachedToken;
        }

        await LoginLock.WaitAsync();

        try
        {
            if (_cachedToken is not null)
            {
                return _cachedToken;
            }

            using var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/centers/login", new { username = "admin", password = "admin" });
            response.EnsureSuccessStatusCode();

            var session = await response.Content.ReadFromJsonAsync<CenterLoginResponse>()
                ?? throw new InvalidOperationException("No se pudo obtener la sesión.");

            _cachedToken = session.Token;
            return _cachedToken;
        }
        finally
        {
            LoginLock.Release();
        }
    }
}
