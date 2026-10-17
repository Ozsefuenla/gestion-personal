using GestionPersonal.Application.Workers;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class CreateWorkerTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public CreateWorkerTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsWorker()
    {
        var result = await Workers.CreateAsync(
            new CreateWorkerCommand("Laura Pérez", "0421", "worker", ApplicationFixture.TestCenterId));

        AssertSuccess(result);
        Assert.True(result.Value.Id > 0);
        Assert.Equal("Laura Pérez", result.Value.FullName);
        Assert.Equal("worker", result.Value.Role);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsValidationError()
    {
        var result = await Workers.CreateAsync(
            new CreateWorkerCommand("", "0421", "worker", ApplicationFixture.TestCenterId));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task Create_WithInvalidRole_ReturnsValidationError()
    {
        var result = await Workers.CreateAsync(
            new CreateWorkerCommand("Laura", "0421", "supervisor", ApplicationFixture.TestCenterId));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task Create_WithInvalidPin_ReturnsValidationError()
    {
        var result = await Workers.CreateAsync(
            new CreateWorkerCommand("Laura", "12ab", "worker", ApplicationFixture.TestCenterId));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task Create_WithOtherCenter_ReturnsForbidden()
    {
        var result = await Workers.CreateAsync(
            new CreateWorkerCommand("Laura", "0421", "worker", TestWorkerIds.New()));

        AssertFailure(result, "Forbidden");
    }

    [Fact]
    public async Task Create_ThenList_ContainsWorker()
    {
        const string fullName = "Nueva Trabajadora";

        await Workers.CreateAsync(
            new CreateWorkerCommand(fullName, "1111", "worker", ApplicationFixture.TestCenterId));

        var result = await Workers.ListActiveAsync();

        AssertSuccess(result);
        Assert.Contains(result.Value, worker => worker.FullName == fullName);
    }

    [Fact]
    public async Task Create_WithMustChangePin_ForcesChangeOnFirstLogin()
    {
        var created = await Workers.CreateAsync(
            new CreateWorkerCommand("Nuevo Con Cambio", "7001", "worker", ApplicationFixture.TestCenterId, MustChangePin: true));

        AssertSuccess(created);
        Assert.True(created.Value.MustChangePin);

        var login = await Workers.LoginAsync(new LoginCommand(created.Value.Id, "7001"));
        AssertSuccess(login);
        Assert.True(login.Value.MustChangePin);

        var change = await Workers.ChangePinAsync(created.Value.Id, new ChangePinCommand("7001", "7002"));
        AssertSuccess(change);

        var loginAgain = await Workers.LoginAsync(new LoginCommand(created.Value.Id, "7002"));
        AssertSuccess(loginAgain);
        Assert.False(loginAgain.Value.MustChangePin);
    }
}
