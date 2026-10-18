using GestionPersonal.Application.Workers;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class WorkerManagementTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public WorkerManagementTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Rename_WithValidName_Succeeds()
    {
        var workerId = await CreateWorkerAsync();

        var result = await Workers.RenameAsync(workerId, new RenameWorkerCommand("Nuevo Nombre"));
        AssertSuccess(result);

        var manage = await Workers.ListAllAsync();
        AssertSuccess(manage);
        Assert.Contains(manage.Value, w => w.Id == workerId && w.FullName == "Nuevo Nombre");
    }

    [Fact]
    public async Task Rename_WithEmptyName_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await Workers.RenameAsync(workerId, new RenameWorkerCommand(""));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task ResetPin_ForcesChangeOnNextLogin()
    {
        var workerId = await CreateWorkerAsync();

        var reset = await Workers.ResetPinAsync(workerId, new ResetWorkerPinCommand("5555"));
        AssertSuccess(reset);

        var login = await Workers.LoginAsync(new LoginCommand(workerId, "5555"));
        AssertSuccess(login);
        Assert.True(login.Value.MustChangePin);
    }

    [Fact]
    public async Task ResetPin_ThenChangePin_ClearsMustChangePin()
    {
        var workerId = await CreateWorkerAsync();

        await Workers.ResetPinAsync(workerId, new ResetWorkerPinCommand("5555"));

        var change = await Workers.ChangePinAsync(workerId, new ChangePinCommand("5555", "6666"));
        AssertSuccess(change);

        var login = await Workers.LoginAsync(new LoginCommand(workerId, "6666"));
        AssertSuccess(login);
        Assert.False(login.Value.MustChangePin);
    }

    [Fact]
    public async Task ResetPin_WithInvalidFormat_ReturnsValidationError()
    {
        var workerId = await CreateWorkerAsync();

        var result = await Workers.ResetPinAsync(workerId, new ResetWorkerPinCommand("12ab"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task Deactivate_ThenLogin_ReturnsForbidden()
    {
        var workerId = await CreateWorkerAsync();

        var deactivate = await Workers.DeactivateAsync(workerId);
        AssertSuccess(deactivate);

        var login = await Workers.LoginAsync(new LoginCommand(workerId, "6001"));
        AssertFailure(login, "Forbidden");
    }

    [Fact]
    public async Task Deactivate_ExcludesFromActiveLists_ButPresentInManage()
    {
        var workerId = await CreateWorkerAsync();

        await Workers.DeactivateAsync(workerId);

        var active = await Workers.ListActiveAsync();
        AssertSuccess(active);
        Assert.DoesNotContain(active.Value, w => w.Id == workerId);

        var selector = await Workers.ListByCenterAsync(ApplicationFixture.TestCenterId);
        AssertSuccess(selector);
        Assert.DoesNotContain(selector.Value, w => w.Id == workerId);

        var manage = await Workers.ListAllAsync();
        AssertSuccess(manage);
        Assert.Contains(manage.Value, w => w.Id == workerId && !w.Active);
    }

    [Fact]
    public async Task Activate_Reactivates()
    {
        var workerId = await CreateWorkerAsync();

        await Workers.DeactivateAsync(workerId);

        var activate = await Workers.ActivateAsync(workerId);
        AssertSuccess(activate);

        var login = await Workers.LoginAsync(new LoginCommand(workerId, "6001"));
        AssertSuccess(login);
    }

    [Fact]
    public async Task Deactivate_AsNonAdmin_ReturnsForbidden()
    {
        var workerId = await CreateWorkerAsync();

        User.ActAsWorker(workerId);

        var result = await Workers.DeactivateAsync(workerId);

        AssertFailure(result, "Forbidden");
    }
}
