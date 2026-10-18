using GestionPersonal.Application.Workers;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class LoginTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public LoginTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Login_WithValidPin_ReturnsWorkerStatus()
    {
        var result = await Workers.LoginAsync(new LoginCommand(1, "1111"));

        AssertSuccess(result);
        Assert.Equal("María García", result.Value.FullName);
    }

    [Fact]
    public async Task Login_WithUnknownPin_ReturnsValidationError()
    {
        var result = await Workers.LoginAsync(new LoginCommand(1, "5555"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task Login_WithUnknownWorker_ReturnsNotFound()
    {
        var result = await Workers.LoginAsync(new LoginCommand(TestWorkerIds.New(), "1111"));

        AssertFailure(result, "NotFound");
    }

    [Fact]
    public async Task Login_WithInvalidPinFormat_ReturnsValidationError()
    {
        var result = await Workers.LoginAsync(new LoginCommand(1, "12ab"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task GetWorkerStatus_WithValidId_ReturnsWorkerStatus()
    {
        var result = await Workers.GetStatusAsync(1);

        AssertSuccess(result);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("María García", result.Value.FullName);
    }

    [Fact]
    public async Task GetWorkerStatus_WithUnknownId_ReturnsNotFound()
    {
        var result = await Workers.GetStatusAsync(TestWorkerIds.New());

        AssertFailure(result, "NotFound");
    }
}
