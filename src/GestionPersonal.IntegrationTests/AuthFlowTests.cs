using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.IntegrationTests;

public sealed class AuthFlowTests : IClassFixture<GestionPersonalWebAppFactory>
{
    private readonly GestionPersonalWebAppFactory _factory;

    public AuthFlowTests(GestionPersonalWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Landing_and_login_pages_are_available()
    {
        using var client = CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/login")).StatusCode);
    }

    [Fact]
    public async Task Csrf_endpoint_returns_token_and_cookie()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/auth/csrf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Set-Cookie"));

        var payload = await response.Content.ReadFromJsonAsync<CsrfResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Token));
    }

    [Fact]
    public async Task Protected_page_redirects_to_login_when_anonymous()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/app");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Login_without_antiforgery_token_is_rejected()
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("/auth/login", new { username = "carlos.ruiz", password = "sol" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_antiforgery_token_signs_in_and_allows_app()
    {
        using var client = CreateClient();
        var token = await GetCsrfTokenAsync(client);

        var login = await PostLoginAsync(client, token);

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.True(login.Headers.Contains("Set-Cookie"));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/app")).StatusCode);
    }

    [Fact]
    public async Task Logout_with_antiforgery_token_signs_out()
    {
        using var client = CreateClient();
        var loginToken = await GetCsrfTokenAsync(client);
        Assert.Equal(HttpStatusCode.OK, (await PostLoginAsync(client, loginToken)).StatusCode);

        var logoutToken = await GetCsrfTokenAsync(client);
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        logoutRequest.Headers.Add("RequestVerificationToken", logoutToken);

        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(logoutRequest)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/app")).StatusCode);
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static async Task<string> GetCsrfTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/auth/csrf");
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CsrfResponse>();
        return payload!.Token;
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/login")
        {
            Content = JsonContent.Create(new { username = "carlos.ruiz", password = "sol" })
        };
        request.Headers.Add("RequestVerificationToken", token);
        return await client.SendAsync(request);
    }

    private sealed record CsrfResponse(string Token);
}
