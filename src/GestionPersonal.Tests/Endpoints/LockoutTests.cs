using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class LockoutTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public LockoutTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task WorkerLogin_AfterTooManyFailures_IsLockedOut()
    {
        const int workerId = 1;

        for (var i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/workers/login", new { workerId, pin = "0000" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        var locked = await _client.PostAsJsonAsync("/api/workers/login", new { workerId, pin = "1111" });
        Assert.Equal(HttpStatusCode.Forbidden, locked.StatusCode);
    }
}
