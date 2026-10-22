using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Monitoring;

public sealed class ListExceptionsQueryHandler
{
    private const int MaxLimit = 200;

    private readonly IMonitoringRepository _repository;
    private readonly ICurrentUser _currentUser;

    public ListExceptionsQueryHandler(IMonitoringRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ExceptionListItem>>> Handle(
        ListExceptionsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireSuperAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var limit = Math.Clamp(query.Limit, 1, MaxLimit);
        var offset = Math.Max(query.Offset, 0);

        var exceptions = await _repository.ListExceptionsAsync(limit, offset, cancellationToken);

        return Result<IReadOnlyList<ExceptionListItem>>.Success(exceptions);
    }
}
