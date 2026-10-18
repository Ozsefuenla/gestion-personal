using GestionPersonal.Application;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Tests.Infrastructure;

public sealed class ApplicationFixture : IDisposable
{
    public const int TestCenterId = 1;

    public ApplicationFixture()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInMemoryTestInfrastructure();
        services.AddSingleton<ICurrentCenter>(new FakeCurrentCenter(TestCenterId));

        Provider = services.BuildServiceProvider();
    }

    public ServiceProvider Provider { get; }

    public void Dispose() => Provider.Dispose();
}

public sealed class FakeCurrentCenter : ICurrentCenter
{
    public FakeCurrentCenter(int? centerId)
    {
        CenterId = centerId;
    }

    public int? CenterId { get; }
}
