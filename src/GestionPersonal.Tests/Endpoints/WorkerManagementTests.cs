using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class WorkerManagementTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public WorkerManagementTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task RenameWorker_WithValidName_Succeeds()
    {
        var workerId = await CreateWorkerAsync();

        var response = await _client.PatchAsJsonAsync($"/api/workers/{workerId}/name", new { fullName = "Nuevo Nombre" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var manage = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers/manage");
        Assert.Contains(manage!, w => w.Id == workerId && w.FullName == "Nuevo Nombre");
    }

    [Fact]
    public async Task RenameWorker_WithEmptyName_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync();

        var response = await _client.PatchAsJsonAsync($"/api/workers/{workerId}/name", new { fullName = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPin_ForcesChangeOnNextLogin()
    {
        var workerId = await CreateWorkerAsync();

        var reset = await _client.PostAsJsonAsync($"/api/workers/{workerId}/reset-pin", new { pin = "5555" });

        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/workers/login", new { workerId, pin = "5555" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var status = await login.Content.ReadFromJsonAsync<WorkerStatusResponse>();
        Assert.True(status!.MustChangePin);
    }

    [Fact]
    public async Task ResetPin_ThenChangePin_ClearsMustChangePin()
    {
        var workerId = await CreateWorkerAsync();

        await _client.PostAsJsonAsync($"/api/workers/{workerId}/reset-pin", new { pin = "5555" });

        var change = await _client.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin = "5555", newPin = "6666" });
        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/workers/login", new { workerId, pin = "6666" });
        var status = await login.Content.ReadFromJsonAsync<WorkerStatusResponse>();
        Assert.False(status!.MustChangePin);
    }

    [Fact]
    public async Task ResetPin_WithInvalidFormat_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync();

        var response = await _client.PostAsJsonAsync($"/api/workers/{workerId}/reset-pin", new { pin = "12ab" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateWorker_ThenLogin_ReturnsForbidden()
    {
        var workerId = await CreateWorkerAsync();

        var deactivate = await _client.PatchAsync($"/api/workers/{workerId}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/workers/login", new { workerId, pin = "6001" });
        Assert.Equal(HttpStatusCode.Forbidden, login.StatusCode);
    }

    [Fact]
    public async Task DeactivateWorker_ExcludesFromActiveLists_ButPresentInManage()
    {
        var workerId = await CreateWorkerAsync();

        await _client.PatchAsync($"/api/workers/{workerId}/deactivate", null);

        var active = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers");
        Assert.DoesNotContain(active!, w => w.Id == workerId);

        var selector = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/centers/1/workers");
        Assert.DoesNotContain(selector!, w => w.Id == workerId);

        var manage = await _client.GetFromJsonAsync<List<WorkerResponse>>("/api/workers/manage");
        Assert.Contains(manage!, w => w.Id == workerId && !w.Active);
    }

    [Fact]
    public async Task ActivateWorker_Reactivates()
    {
        var workerId = await CreateWorkerAsync();

        await _client.PatchAsync($"/api/workers/{workerId}/deactivate", null);

        var activate = await _client.PatchAsync($"/api/workers/{workerId}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/workers/login", new { workerId, pin = "6001" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task DeactivateWorker_Admin_ReturnsBadRequest()
    {
        const int adminId = 3;

        var response = await _client.PatchAsync($"/api/workers/{adminId}/deactivate", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<int> CreateWorkerAsync(string pin = "6001")
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new
        {
            fullName = "Worker Management",
            pin,
            role = "worker",
            centerId = 1
        });

        response.EnsureSuccessStatusCode();

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>()
            ?? throw new InvalidOperationException("No se pudo crear el trabajador.");

        return worker.Id;
    }
}
