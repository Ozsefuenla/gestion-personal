using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Platform;

public sealed class ListAllCentersQueryHandler
{
    private readonly ICenterRepository _centerRepository;
    private readonly ICurrentUser _currentUser;

    public ListAllCentersQueryHandler(ICenterRepository centerRepository, ICurrentUser currentUser)
    {
        _centerRepository = centerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CenterResponse>>> Handle(CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireSuperAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var centers = await _centerRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<CenterResponse> response = centers
            .Select(CenterResponse.From)
            .ToList();

        return Result<IReadOnlyList<CenterResponse>>.Success(response);
    }
}
