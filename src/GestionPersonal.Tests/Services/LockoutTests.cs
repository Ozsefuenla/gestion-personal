using GestionPersonal.Application.Workers;
using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class LockoutTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public LockoutTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task WorkerLogin_AfterTooManyFailures_IsLockedOut()
    {
        const int workerId = 1;

        for (var i = 0; i < 5; i++)
        {
            var failed = await Workers.LoginAsync(new LoginCommand(workerId, "0000"));
            AssertFailure(failed, "Validation");
        }

        var locked = await Workers.LoginAsync(new LoginCommand(workerId, "1111"));
        AssertFailure(locked, "Forbidden");
    }
}
