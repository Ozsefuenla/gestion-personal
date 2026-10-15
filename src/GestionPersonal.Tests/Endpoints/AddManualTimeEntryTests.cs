using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestionPersonal.Tests.Endpoints;

public sealed class AddManualTimeEntryTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public AddManualTimeEntryTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddManual_Start_NoOpenEntry_Succeeds()
    {
        var workerId = await CreateWorkerAsync("8001");

        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(9, 0)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();
        Assert.NotNull(entry);
        Assert.Equal(workerId, entry.WorkerId);
        Assert.Equal("in-progress", entry.Status);
        Assert.NotEqual(default, entry.StartedAt);
    }

    [Fact]
    public async Task AddManual_Start_WhenOpen_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync("8002");

        await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(9, 30)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_Start_OverlappingFinishedEntry_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync("8003");

        await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "end", Yesterday, endTime: new TimeOnly(18, 0)));

        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(10, 0)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_Pause_NoOpenEntry_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync("8004");

        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "pause", Yesterday, new TimeOnly(13, 0)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_Pause_WithEndGreaterThanStart_Succeeds()
    {
        var workerId = await CreateWorkerAsync("8005");

        await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "pause", Yesterday, new TimeOnly(13, 0), new TimeOnly(14, 0)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entry = await response.Content.ReadFromJsonAsync<TimeEntryResponse>();
        Assert.NotNull(entry);
        Assert.Equal("in-progress", entry.Status);
    }

    [Fact]
    public async Task AddManual_Pause_WithEndNotGreaterThanStart_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("8006");

        await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "pause", Yesterday, new TimeOnly(13, 0), new TimeOnly(12, 0)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_End_NoOpenEntry_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync("8007");

        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "end", Yesterday, endTime: new TimeOnly(18, 0)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_End_BeforeStart_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("8008");

        await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday, new TimeOnly(9, 0)));
        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "end", Yesterday, endTime: new TimeOnly(8, 0)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_FutureDate_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("8009");

        var future = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", future, new TimeOnly(9, 0)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_InvalidType_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("8010");

        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "foo", Yesterday, new TimeOnly(9, 0)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_Start_MissingStartTime_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("8011");

        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "start", Yesterday));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddManual_End_MissingEndTime_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync("8012");

        var response = await _client.PostAsJsonAsync("/api/time-entries/manual", BuildBody(workerId, "end", Yesterday));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static DateOnly Yesterday => DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

    private static object BuildBody(int workerId, string type, DateOnly date, TimeOnly? startTime = null, TimeOnly? endTime = null) =>
        new { workerId, type, date, startTime, endTime };

    private async Task<int> CreateWorkerAsync(string pin)
    {
        var response = await _client.PostAsJsonAsync("/api/workers", new { fullName = $"Worker {pin}", pin = pin, role = "worker", centerId = 1 });
        response.EnsureSuccessStatusCode();

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>()
            ?? throw new InvalidOperationException("No se pudo crear el trabajador.");

        return worker.Id;
    }
}
