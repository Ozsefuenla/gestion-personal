using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Centers;

public sealed class ListCentersQueryHandler
{
    private readonly ICenterRepository _centerRepository;
    private readonly ICurrentCenter _currentCenter;

    public ListCentersQueryHandler(
        ICenterRepository centerRepository,
        ICurrentCenter currentCenter)
    {
        _centerRepository = centerRepository;
        _currentCenter = currentCenter;
    }

    public async Task<Result<IReadOnlyList<CenterResponse>>> Handle(
        ListCentersQuery query,
        CancellationToken cancellationToken = default)
    {
        var centerId = _currentCenter.CenterId;

        if (centerId is null)
        {
            return Error.Forbidden("No autenticado.");
        }

        var center = await _centerRepository.GetByIdAsync(centerId.Value, cancellationToken);

        if (center is null)
        {
            return Error.NotFound("El centro no existe.");
        }

        var response = new List<CenterResponse> { CenterResponse.From(center) };

        return Result<IReadOnlyList<CenterResponse>>.Success(response);
    }
}
