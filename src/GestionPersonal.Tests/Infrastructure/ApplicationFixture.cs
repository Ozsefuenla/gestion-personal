using GestionPersonal.Application;
using GestionPersonal.Application.Abstractions;
using GestionPersonal.Domain.Enums;
using GestionPersonal.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Tests.Infrastructure;

public sealed class ApplicationFixture : IDisposable
{
    public const int TestCenterId = 1;
    public const int TestCenterAdminUserId = 2;

    public ApplicationFixture()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInMemoryTestInfrastructure();

        User = new FakeCurrentUser(TestCenterId, UserRole.CenterAdmin, TestCenterAdminUserId);

        services.AddSingleton<ICurrentUser>(User);
        services.AddSingleton<ICurrentCenter>(User);

        Provider = services.BuildServiceProvider();
    }

    public FakeCurrentUser User { get; }

    public ServiceProvider Provider { get; }

    public void Dispose() => Provider.Dispose();
}

public sealed class FakeCurrentUser : ICurrentUser, ICurrentCenter
{
    private readonly int? _defaultCenterId;
    private readonly int? _defaultSubjectId;

    public FakeCurrentUser(int? centerId, UserRole? role, int? subjectId = null)
    {
        _defaultCenterId = centerId;
        _defaultSubjectId = subjectId;
        CenterId = centerId;
        Role = role;
        SubjectId = subjectId;
    }

    public int? SubjectId { get; set; }

    public int? CenterId { get; set; }

    public UserRole? Role { get; set; }

    public int? WorkerId => Role == UserRole.CenterWorker ? SubjectId : null;

    public bool MustChangePassword { get; set; }

    public bool IsAuthenticated => Role is not null;

    public void ActAsCenterAdmin()
    {
        Role = UserRole.CenterAdmin;
        CenterId = _defaultCenterId;
        SubjectId = _defaultSubjectId;
    }

    public void ActAsCenter()
    {
        Role = UserRole.Center;
        CenterId = _defaultCenterId;
        SubjectId = _defaultSubjectId;
    }

    public void ActAsWorker(int workerId)
    {
        Role = UserRole.CenterWorker;
        CenterId = _defaultCenterId;
        SubjectId = workerId;
    }

    public void ActAsCenterUser(int userId, UserRole role)
    {
        Role = role;
        CenterId = _defaultCenterId;
        SubjectId = userId;
    }

    public void ActAsSuperAdmin()
    {
        Role = UserRole.SuperAdmin;
        CenterId = null;
        SubjectId = 1;
    }
}
