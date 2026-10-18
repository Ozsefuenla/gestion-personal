using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Centers;

public sealed class ListCentersQueryHandler
{
    private readonly ICenterRepository _centerRepository;
    private readonly ICurrentUser _currentUser;

    public ListCentersQueryHandler(
        ICenterRepository centerRepository,
        ICurrentUser currentUser)
    {
        _centerRepository = centerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CenterResponse>>> Handle(
        ListCentersQuery query,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.CenterId is not { } centerId)
        {
            return Result<IReadOnlyList<CenterResponse>>.Success([]);
        }

        var center = await _centerRepository.GetByIdAsync(centerId, cancellationToken);

        if (center is null)
        {
            return Error.NotFound("El centro no existe.");
        }

        var response = new List<CenterResponse> { CenterResponse.From(center) };

        return Result<IReadOnlyList<CenterResponse>>.Success(response);
    }
}
