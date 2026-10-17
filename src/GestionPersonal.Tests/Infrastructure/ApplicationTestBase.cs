using GestionPersonal.Application.Common;
using GestionPersonal.Application.Services;
using GestionPersonal.Application.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Tests.Infrastructure;

public abstract class ApplicationTestBase : IDisposable
{
    private readonly IServiceScope _scope;

    protected ApplicationTestBase(ApplicationFixture fixture)
    {
        _scope = fixture.Provider.CreateScope();
    }

    protected ICentersService Centers => _scope.ServiceProvider.GetRequiredService<ICentersService>();

    protected IWorkersService Workers => _scope.ServiceProvider.GetRequiredService<IWorkersService>();

    protected ITimeEntriesService TimeEntries => _scope.ServiceProvider.GetRequiredService<ITimeEntriesService>();

    protected IVacationsService Vacations => _scope.ServiceProvider.GetRequiredService<IVacationsService>();

    protected async Task<int> CreateWorkerAsync(
        string pin = "6001",
        string role = "worker",
        string fullName = "Test Worker",
        bool mustChangePin = false,
        int centerId = ApplicationFixture.TestCenterId)
    {
        var result = await Workers.CreateAsync(new CreateWorkerCommand(fullName, pin, role, centerId, mustChangePin));

        AssertSuccess(result);

        return result.Value.Id;
    }

    protected static void AssertSuccess(Result result)
    {
        Assert.True(
            result.IsSuccess,
            result.IsFailure ? $"{result.Error.Code}: {result.Error.Message}" : "Se esperaba un resultado exitoso.");
    }

    protected static void AssertFailure(Result result, string errorCode)
    {
        Assert.True(result.IsFailure, "Se esperaba un resultado fallido.");
        Assert.Equal(errorCode, result.Error.Code);
    }

    public void Dispose() => _scope.Dispose();
}
