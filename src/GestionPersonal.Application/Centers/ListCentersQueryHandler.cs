using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Centers;

public sealed class ListCentersQueryHandler
{
    private readonly ICenterRepository _centerRepository;

    public ListCentersQueryHandler(ICenterRepository centerRepository)
    {
        _centerRepository = centerRepository;
    }

    public async Task<Result<IReadOnlyList<CenterResponse>>> Handle(
        ListCentersQuery query,
        CancellationToken cancellationToken = default)
    {
        var centers = await _centerRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<CenterResponse> response = centers
            .Select(CenterResponse.From)
            .ToList();

        return Result<IReadOnlyList<CenterResponse>>.Success(response);
    }
}
