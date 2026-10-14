using System.Net;
using System.Net.Http.Json;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Vacations;
using GestionPersonal.Application.Workers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Tests.Endpoints;

public sealed class VacationsTests : AuthenticatedApiTest, IClassFixture<WebApplicationFactory<Program>>
{
    public VacationsTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task RequestVacation_WithValidDates_CreatesPending()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var response = await _client.PostAsJsonAsync("/api/vacations",
            new { workerId, startDate = today.AddDays(1), endDate = today.AddDays(3), type = "vacations" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var vacation = await response.Content.ReadFromJsonAsync<VacationResponse>();
        Assert.NotNull(vacation);
        Assert.Equal("pending", vacation.Status);
        Assert.Equal("vacations", vacation.Type);
    }

    [Fact]
    public async Task RequestVacation_WithEndBeforeStart_ReturnsBadRequest()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var response = await _client.PostAsJsonAsync("/api/vacations",
            new { workerId, startDate = today.AddDays(5), endDate = today.AddDays(1), type = "vacations" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestVacation_AsAdmin_IsAutoApproved()
    {
        var workerId = await CreateWorkerAsync("admin");
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        Assert.Equal("approved", vacation.Status);
    }

    [Fact]
    public async Task RequestVacation_OverlappingActive_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        await RequestVacationAsync(workerId, today.AddDays(5), today.AddDays(10));

        var response = await _client.PostAsJsonAsync("/api/vacations",
            new { workerId, startDate = today.AddDays(8), endDate = today.AddDays(12), type = "sick-leave" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ListVacations_ReturnsDescendingAndExcludesDeleted()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var older = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        var newer = await RequestVacationAsync(workerId, today.AddDays(20), today.AddDays(21));

        var list = await _client.GetFromJsonAsync<List<VacationResponse>>($"/api/vacations?workerId={workerId}");

        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
        Assert.Equal(newer.Id, list[0].Id);
        Assert.Equal(older.Id, list[1].Id);

        await _client.DeleteAsync($"/api/vacations/{older.Id}");

        var afterDelete = await _client.GetFromJsonAsync<List<VacationResponse>>($"/api/vacations?workerId={workerId}");

        Assert.NotNull(afterDelete);
        Assert.Single(afterDelete);
        Assert.Equal(newer.Id, afterDelete[0].Id);
    }

    [Fact]
    public async Task DeleteVacation_WhenPending_Succeeds()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(3), today.AddDays(4));

        var response = await _client.DeleteAsync($"/api/vacations/{vacation.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var list = await _client.GetFromJsonAsync<List<VacationResponse>>($"/api/vacations?workerId={workerId}");
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task DeleteVacation_WhenApprovedAndNotAdmin_ReturnsForbidden()
    {
        var workerId = await CreateWorkerAsync("worker");
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(3), today.AddDays(4));
        ApproveVacation(vacation.Id);

        var response = await _client.DeleteAsync($"/api/vacations/{vacation.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteVacation_WhenApprovedAndAdmin_Succeeds()
    {
        var workerId = await CreateWorkerAsync("admin");
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(3), today.AddDays(4));
        ApproveVacation(vacation.Id);

        var response = await _client.DeleteAsync($"/api/vacations/{vacation.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task MonthlySummary_IgnoresPendingAbsence()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridNow();
        var today = MadridToday();

        await RequestVacationAsync(workerId, today, today);

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>(
            $"/api/time-entries/monthly?workerId={workerId}&year={year}&month={month}");

        Assert.NotNull(summary);
        Assert.False(summary.Days.Single(d => d.Day == today.Day).IsAbsence);
    }

    [Fact]
    public async Task MonthlySummary_MarksApprovedAbsenceDay()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridNow();
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today, today);
        ApproveVacation(vacation.Id);

        var summary = await _client.GetFromJsonAsync<MonthlySummaryResponse>(
            $"/api/time-entries/monthly?workerId={workerId}&year={year}&month={month}");

        Assert.NotNull(summary);
        var day = summary.Days.Single(d => d.Day == today.Day);
        Assert.True(day.IsAbsence);
        Assert.Equal("vacations", day.AbsenceType);
    }

    [Fact]
    public async Task ApproveVacation_WhenPending_Approves()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        var response = await _client.PatchAsync($"/api/vacations/{vacation.Id}/approve", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<VacationResponse>();
        Assert.NotNull(updated);
        Assert.Equal("approved", updated.Status);
    }

    [Fact]
    public async Task RejectVacation_WhenPending_Rejects()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        var response = await _client.PatchAsync($"/api/vacations/{vacation.Id}/reject", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<VacationResponse>();
        Assert.NotNull(updated);
        Assert.Equal("rejected", updated.Status);
    }

    [Fact]
    public async Task ApproveVacation_WhenNotPending_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        await _client.PatchAsync($"/api/vacations/{vacation.Id}/approve", null);
        var response = await _client.PatchAsync($"/api/vacations/{vacation.Id}/approve", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ApproveVacation_WhenDeleted_ReturnsNotFound()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        await _client.DeleteAsync($"/api/vacations/{vacation.Id}");

        var response = await _client.PatchAsync($"/api/vacations/{vacation.Id}/approve", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListCenterVacations_ReturnsAllNonDeleted()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var approved = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        await _client.PatchAsync($"/api/vacations/{approved.Id}/approve", null);

        var rejected = await RequestVacationAsync(workerId, today.AddDays(10), today.AddDays(11));
        await _client.PatchAsync($"/api/vacations/{rejected.Id}/reject", null);

        var list = await _client.GetFromJsonAsync<List<VacationResponse>>("/api/vacations/center");

        Assert.NotNull(list);
        var mine = list.Where(v => v.WorkerId == workerId).ToList();
        Assert.Equal(2, mine.Count);
        Assert.Contains(mine, v => v.Status == "approved");
        Assert.Contains(mine, v => v.Status == "rejected");
    }

    [Fact]
    public async Task ListVacations_IncludesRejected()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridToday();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        await _client.PatchAsync($"/api/vacations/{vacation.Id}/reject", null);

        var list = await _client.GetFromJsonAsync<List<VacationResponse>>($"/api/vacations?workerId={workerId}");

        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("rejected", list[0].Status);
    }

    private async Task<int> CreateWorkerAsync(string role = "worker")
    {
        var response = await _client.PostAsJsonAsync("/api/workers",
            new { fullName = "Test Worker", pin = "9999", role, centerId = 1 });

        response.EnsureSuccessStatusCode();

        var worker = await response.Content.ReadFromJsonAsync<WorkerResponse>()
            ?? throw new InvalidOperationException("No se pudo crear el trabajador.");

        return worker.Id;
    }

    private async Task<VacationResponse> RequestVacationAsync(int workerId, DateOnly startDate, DateOnly endDate, string type = "vacations")
    {
        var response = await _client.PostAsJsonAsync("/api/vacations",
            new { workerId, startDate, endDate, type });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<VacationResponse>()
            ?? throw new InvalidOperationException("No se pudo crear la ausencia.");
    }

    private void ApproveVacation(Guid id)
    {
        var repository = _factory.Services.GetRequiredService<IVacationRepository>();
        var vacation = repository.GetByIdAsync(id).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("No se encontró la ausencia.");

        vacation.Approve(DateTimeOffset.UtcNow);
    }

    private static DateOnly MadridToday()
    {
        var zone = ResolveMadrid();
        var local = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    private static (int Year, int Month) MadridNow()
    {
        var zone = ResolveMadrid();
        var local = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        return (local.Year, local.Month);
    }

    private static TimeZoneInfo ResolveMadrid()
    {
        foreach (var id in new[] { "Europe/Madrid", "Romance Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new TimeZoneNotFoundException("No se encontró la zona horaria de Madrid.");
    }
}
