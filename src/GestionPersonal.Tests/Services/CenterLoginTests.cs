using GestionPersonal.Application.Centers;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class CenterLoginTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public CenterLoginTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSession()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("admin", "admin"));

        AssertSuccess(result);
        Assert.Equal(1, result.Value.CenterId);
        Assert.Equal("Peluquería Sol", result.Value.CenterName);
        Assert.Equal("admin", result.Value.Username);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsValidationError()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("admin", "incorrecta"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsNotFound()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("noexiste", "admin"));

        AssertFailure(result, "NotFound");
    }

    [Fact]
    public async Task Login_WithEmptyFields_ReturnsValidationError()
    {
        var result = await Centers.LoginAsync(new CenterLoginCommand("", ""));

        AssertFailure(result, "Validation");
    }
}
