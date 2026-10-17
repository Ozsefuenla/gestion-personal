using GestionPersonal.Application.TimeEntries;
using GestionPersonal.Application.Vacations;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class VacationsTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public VacationsTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task RequestVacation_WithValidDates_CreatesPending()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var result = await Vacations.RequestAsync(
            new RequestVacationCommand(workerId, today.AddDays(1), today.AddDays(3), "vacations"));

        AssertSuccess(result);
        Assert.Equal("pending", result.Value.Status);
        Assert.Equal("vacations", result.Value.Type);
    }

    [Fact]
    public async Task RequestVacation_WithEndBeforeStart_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var result = await Vacations.RequestAsync(
            new RequestVacationCommand(workerId, today.AddDays(5), today.AddDays(1), "vacations"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task RequestVacation_AsAdmin_IsAutoApproved()
    {
        var workerId = await CreateWorkerAsync(role: "admin");
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        Assert.Equal("approved", vacation.Status);
    }

    [Fact]
    public async Task RequestVacation_OverlappingActive_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        await RequestVacationAsync(workerId, today.AddDays(5), today.AddDays(10));

        var result = await Vacations.RequestAsync(
            new RequestVacationCommand(workerId, today.AddDays(8), today.AddDays(12), "sick-leave"));

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task ListVacations_ReturnsDescendingAndExcludesDeleted()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var older = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        var newer = await RequestVacationAsync(workerId, today.AddDays(20), today.AddDays(21));

        var list = await Vacations.ListAsync(new ListVacationsQuery(workerId));

        AssertSuccess(list);
        Assert.Equal(2, list.Value.Count);
        Assert.Equal(newer.Id, list.Value[0].Id);
        Assert.Equal(older.Id, list.Value[1].Id);

        await Vacations.DeleteAsync(older.Id);

        var afterDelete = await Vacations.ListAsync(new ListVacationsQuery(workerId));

        AssertSuccess(afterDelete);
        Assert.Single(afterDelete.Value);
        Assert.Equal(newer.Id, afterDelete.Value[0].Id);
    }

    [Fact]
    public async Task DeleteVacation_WhenPending_Succeeds()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(3), today.AddDays(4));

        var result = await Vacations.DeleteAsync(vacation.Id);
        AssertSuccess(result);

        var list = await Vacations.ListAsync(new ListVacationsQuery(workerId));
        AssertSuccess(list);
        Assert.Empty(list.Value);
    }

    [Fact]
    public async Task DeleteVacation_WhenApprovedAndNotAdmin_ReturnsForbidden()
    {
        var workerId = await CreateWorkerAsync(role: "worker");
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(3), today.AddDays(4));

        var approve = await Vacations.ApproveAsync(vacation.Id);
        AssertSuccess(approve);

        var result = await Vacations.DeleteAsync(vacation.Id);

        AssertFailure(result, "Forbidden");
    }

    [Fact]
    public async Task DeleteVacation_WhenApprovedAndAdmin_Succeeds()
    {
        var workerId = await CreateWorkerAsync(role: "admin");
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(3), today.AddDays(4));
        Assert.Equal("approved", vacation.Status);

        var result = await Vacations.DeleteAsync(vacation.Id);

        AssertSuccess(result);
    }

    [Fact]
    public async Task MonthlySummary_IgnoresPendingAbsence()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridTestTime.CurrentMonth();
        var today = MadridTestTime.Today();

        await RequestVacationAsync(workerId, today, today);

        var summary = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(workerId, year, month));

        AssertSuccess(summary);
        Assert.False(summary.Value.Days.Single(d => d.Day == today.Day).IsAbsence);
    }

    [Fact]
    public async Task MonthlySummary_MarksApprovedAbsenceDay()
    {
        var workerId = await CreateWorkerAsync();
        var (year, month) = MadridTestTime.CurrentMonth();
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today, today);

        var approve = await Vacations.ApproveAsync(vacation.Id);
        AssertSuccess(approve);

        var summary = await TimeEntries.GetMonthlyAsync(new MonthlySummaryQuery(workerId, year, month));

        AssertSuccess(summary);
        var day = summary.Value.Days.Single(d => d.Day == today.Day);
        Assert.True(day.IsAbsence);
        Assert.Equal("vacations", day.AbsenceType);
    }

    [Fact]
    public async Task ApproveVacation_WhenPending_Approves()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        var result = await Vacations.ApproveAsync(vacation.Id);

        AssertSuccess(result);
        Assert.Equal("approved", result.Value.Status);
    }

    [Fact]
    public async Task RejectVacation_WhenPending_Rejects()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        var result = await Vacations.RejectAsync(vacation.Id);

        AssertSuccess(result);
        Assert.Equal("rejected", result.Value.Status);
    }

    [Fact]
    public async Task ApproveVacation_WhenNotPending_ReturnsConflict()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));

        await Vacations.ApproveAsync(vacation.Id);
        var result = await Vacations.ApproveAsync(vacation.Id);

        AssertFailure(result, "Conflict");
    }

    [Fact]
    public async Task ApproveVacation_WhenDeleted_ReturnsNotFound()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        await Vacations.DeleteAsync(vacation.Id);

        var result = await Vacations.ApproveAsync(vacation.Id);

        AssertFailure(result, "NotFound");
    }

    [Fact]
    public async Task ListCenterVacations_ReturnsAllNonDeleted()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var approved = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        await Vacations.ApproveAsync(approved.Id);

        var rejected = await RequestVacationAsync(workerId, today.AddDays(10), today.AddDays(11));
        await Vacations.RejectAsync(rejected.Id);

        var list = await Vacations.ListCenterAsync();

        AssertSuccess(list);
        var mine = list.Value.Where(v => v.WorkerId == workerId).ToList();
        Assert.Equal(2, mine.Count);
        Assert.Contains(mine, v => v.Status == "approved");
        Assert.Contains(mine, v => v.Status == "rejected");
    }

    [Fact]
    public async Task ListVacations_IncludesRejected()
    {
        var workerId = await CreateWorkerAsync();
        var today = MadridTestTime.Today();

        var vacation = await RequestVacationAsync(workerId, today.AddDays(1), today.AddDays(2));
        await Vacations.RejectAsync(vacation.Id);

        var list = await Vacations.ListAsync(new ListVacationsQuery(workerId));

        AssertSuccess(list);
        Assert.Single(list.Value);
        Assert.Equal("rejected", list.Value[0].Status);
    }

    private async Task<VacationResponse> RequestVacationAsync(
        int workerId,
        DateOnly startDate,
        DateOnly endDate,
        string type = "vacations")
    {
        var result = await Vacations.RequestAsync(new RequestVacationCommand(workerId, startDate, endDate, type));

        AssertSuccess(result);

        return result.Value;
    }
}
