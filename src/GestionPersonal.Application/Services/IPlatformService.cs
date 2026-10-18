using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Common;
using GestionPersonal.Application.Platform;

namespace GestionPersonal.Application.Services;

public interface IPlatformService
{
    Task<Result<IReadOnlyList<CenterResponse>>> ListCentersAsync(CancellationToken cancellationToken = default);

    Task<Result<CenterResponse>> CreateCenterAsync(CreateCenterCommand command, CancellationToken cancellationToken = default);

    Task<Result> SetCenterActiveAsync(SetCenterActiveCommand command, CancellationToken cancellationToken = default);

    Task<Result<CenterUserResponse>> CreateCenterAdminAsync(CreateCenterAdminCommand command, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CenterUserResponse>>> ListCenterUsersAsync(int centerId, CancellationToken cancellationToken = default);

    Task<Result<CenterUserResponse>> CreateCenterUserAsync(CreateCenterUserCommand command, CancellationToken cancellationToken = default);

    Task<Result> SetCenterUserActiveAsync(SetCenterUserActiveCommand command, CancellationToken cancellationToken = default);

    Task<Result> ResetCenterUserPasswordAsync(ResetCenterUserPasswordCommand command, CancellationToken cancellationToken = default);
}
