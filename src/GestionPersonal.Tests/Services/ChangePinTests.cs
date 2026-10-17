using GestionPersonal.Application.Workers;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class ChangePinTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public ChangePinTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ChangePin_WithCorrectCurrentPin_Succeeds()
    {
        const int workerId = 2;

        var result = await Workers.ChangePinAsync(workerId, new ChangePinCommand("2222", "9999"));
        AssertSuccess(result);

        var login = await Workers.LoginAsync(new LoginCommand(workerId, "9999"));
        AssertSuccess(login);
    }

    [Fact]
    public async Task ChangePin_WithWrongCurrentPin_ReturnsValidationError()
    {
        const int workerId = 3;

        var result = await Workers.ChangePinAsync(workerId, new ChangePinCommand("0000", "4444"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task ChangePin_WithSamePin_ReturnsValidationError()
    {
        const int workerId = 1;

        var result = await Workers.ChangePinAsync(workerId, new ChangePinCommand("1111", "1111"));

        AssertFailure(result, "Validation");
    }

    [Fact]
    public async Task ChangePin_WithInvalidFormat_ReturnsValidationError()
    {
        const int workerId = 1;

        var result = await Workers.ChangePinAsync(workerId, new ChangePinCommand("1111", "12ab"));

        AssertFailure(result, "Validation");
    }
}
