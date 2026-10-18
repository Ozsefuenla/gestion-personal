using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Platform;
using GestionPersonal.Domain.Enums;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class PlatformTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    private const int SuperAdminId = 1;
    private const int CenterAdminId = 2;
    private const int CenterUserId = 3;

    public PlatformTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ListCenterUsers_AsSuperAdmin_ReturnsOnlyCenterAdminAndCenter()
    {
        User.ActAsSuperAdmin();

        var result = await Platform.ListCenterUsersAsync(ApplicationFixture.TestCenterId);

        AssertSuccess(result);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, u => u.Username == "carlos.ruiz" && u.Role == UserRole.CenterAdmin);
        Assert.Contains(result.Value, u => u.Username == "centro" && u.Role == UserRole.Center);
        Assert.DoesNotContain(result.Value, u => u.Role == UserRole.SuperAdmin);
    }

    [Fact]
    public async Task ResetCenterUserPassword_AsSuperAdmin_AllowsLoginWithNewPassword()
    {
        User.ActAsSuperAdmin();

        var reset = await Platform.ResetCenterUserPasswordAsync(
            new ResetCenterUserPasswordCommand(CenterAdminId, "nueva-clave"));

        AssertSuccess(reset);

        var login = await Centers.LoginAsync(new CenterLoginCommand("carlos.ruiz", "nueva-clave"));
        AssertSuccess(login);
        Assert.True(login.Value.MustChangePassword);

        var oldPassword = await Centers.LoginAsync(new CenterLoginCommand("carlos.ruiz", "sol"));
        AssertFailure(oldPassword, "Validation");
    }

    [Fact]
    public async Task ResetCenterUserPassword_AsCenterAdmin_ForTabletUser_AllowsLogin()
    {
        var reset = await Platform.ResetCenterUserPasswordAsync(
            new ResetCenterUserPasswordCommand(CenterUserId, "tablet-nueva"));

        AssertSuccess(reset);

        var login = await Centers.LoginAsync(new CenterLoginCommand("centro", "tablet-nueva"));
        AssertSuccess(login);
    }

    [Fact]
    public async Task ResetCenterUserPassword_AsCenterAdmin_ForAnotherCenterAdmin_ReturnsForbidden()
    {
        var reset = await Platform.ResetCenterUserPasswordAsync(
            new ResetCenterUserPasswordCommand(CenterAdminId, "otra-clave"));

        AssertFailure(reset, "Forbidden");
    }

    [Fact]
    public async Task ResetCenterUserPassword_AsSuperAdmin_ForSuperAdmin_ReturnsForbidden()
    {
        User.ActAsSuperAdmin();

        var reset = await Platform.ResetCenterUserPasswordAsync(
            new ResetCenterUserPasswordCommand(SuperAdminId, "otra-clave"));

        AssertFailure(reset, "Forbidden");
    }

    [Fact]
    public async Task ResetCenterUserPassword_WithShortPassword_ReturnsValidation()
    {
        User.ActAsSuperAdmin();

        var reset = await Platform.ResetCenterUserPasswordAsync(
            new ResetCenterUserPasswordCommand(CenterAdminId, "abc"));

        AssertFailure(reset, "Validation");
    }

    [Fact]
    public async Task ResetCenterUserPassword_ForUnknownUser_ReturnsNotFound()
    {
        User.ActAsSuperAdmin();

        var reset = await Platform.ResetCenterUserPasswordAsync(
            new ResetCenterUserPasswordCommand(9999, "clave-valida"));

        AssertFailure(reset, "NotFound");
    }
}
