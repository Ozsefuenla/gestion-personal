using GestionPersonal.Application.Account;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Platform;
using GestionPersonal.Domain.Enums;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class AccountTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    private const int CenterAdminId = 2;
    private const int CenterUserId = 3;

    public AccountTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ChangePassword_WithTemporaryPassword_ClearsFlagAndAllowsNewLogin()
    {
        User.ActAsSuperAdmin();

        var reset = await Platform.ResetCenterUserPasswordAsync(
            new ResetCenterUserPasswordCommand(CenterAdminId, "temporal"));

        AssertSuccess(reset);

        User.ActAsCenterAdmin();

        var before = await Centers.LoginAsync(new CenterLoginCommand("carlos.ruiz", "temporal"));
        AssertSuccess(before);
        Assert.True(before.Value.MustChangePassword);

        var change = await Account.ChangePasswordAsync(new ChangePasswordCommand("temporal", "definitiva"));
        AssertSuccess(change);

        var after = await Centers.LoginAsync(new CenterLoginCommand("carlos.ruiz", "definitiva"));
        AssertSuccess(after);
        Assert.False(after.Value.MustChangePassword);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrent_ReturnsValidation()
    {
        User.ActAsCenterUser(CenterUserId, UserRole.Center);

        var result = await Account.ChangePasswordAsync(
            new ChangePasswordCommand("incorrecta", "nueva-clave"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task ChangePassword_WithShortNew_ReturnsValidation()
    {
        User.ActAsCenterUser(CenterUserId, UserRole.Center);

        var result = await Account.ChangePasswordAsync(
            new ChangePasswordCommand("centro", "abc"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task ChangePassword_WithSamePassword_ReturnsValidation()
    {
        User.ActAsCenterUser(CenterUserId, UserRole.Center);

        var result = await Account.ChangePasswordAsync(
            new ChangePasswordCommand("centro", "centro"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task ChangePassword_AsCenterWorker_ReturnsForbidden()
    {
        User.ActAsWorker(1);

        var result = await Account.ChangePasswordAsync(
            new ChangePasswordCommand("1111", "nueva-clave"));

        AssertFailure(result, "Forbidden");
    }
}
