using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Services;

public interface ICentersService
{
    Task<Result<IReadOnlyList<CenterResponse>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<CenterSessionResponse>> LoginAsync(
        CenterLoginCommand command,
        CancellationToken cancellationToken = default);
}
