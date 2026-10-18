using GestionPersonal.Application.Centers;
using GestionPersonal.Domain.Enums;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class CenterLoginTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public CenterLoginTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Login_WithValidCenterAdminCredentials_ReturnsSession()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("carlos.ruiz", "sol"));

        AssertSuccess(result);
        Assert.Equal(1, result.Value.CenterId);
        Assert.Equal("Peluquería Sol", result.Value.CenterName);
        Assert.Equal("carlos.ruiz", result.Value.Username);
        Assert.Equal(UserRole.CenterAdmin, result.Value.Role);
        Assert.False(result.Value.MustChangePassword);
    }

    [Fact]
    public async Task Login_WithValidCenterKioskCredentials_ReturnsCenterRole()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("centro", "centro"));

        AssertSuccess(result);
        Assert.Equal(1, result.Value.CenterId);
        Assert.Equal(UserRole.Center, result.Value.Role);
    }

    [Fact]
    public async Task Login_WithSuperAdminCredentials_ReturnsNoCenter()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("superadmin", "superadmin"));

        AssertSuccess(result);
        Assert.Null(result.Value.CenterId);
        Assert.Equal(UserRole.SuperAdmin, result.Value.Role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsValidationError()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("carlos.ruiz", "incorrecta"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsNotFound()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("noexiste", "sol"));

        AssertFailure(result, "NotFound");
    }

    [Fact]
    public async Task Login_WithEmptyFields_ReturnsValidationError()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("", ""));

        AssertFailure(result, "Validation");
    }
}
