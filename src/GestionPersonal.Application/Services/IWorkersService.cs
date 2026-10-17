using GestionPersonal.Application.Common;
using GestionPersonal.Application.Workers;

namespace GestionPersonal.Application.Services;

public interface IWorkersService
{
    Task<Result<IReadOnlyList<WorkerResponse>>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<WorkerResponse>>> ListByCenterAsync(
        int centerId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<WorkerResponse>>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<WorkerStatusResponse>>> GetStatusesAsync(CancellationToken cancellationToken = default);

    Task<Result<WorkerStatusResponse>> GetStatusAsync(int workerId, CancellationToken cancellationToken = default);

    Task<Result<WorkerStatusResponse>> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<WorkerResponse>> CreateAsync(
        CreateWorkerCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePinAsync(
        int workerId,
        ChangePinCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> RenameAsync(
        int workerId,
        RenameWorkerCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPinAsync(
        int workerId,
        ResetWorkerPinCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> ActivateAsync(int workerId, CancellationToken cancellationToken = default);

    Task<Result> DeactivateAsync(int workerId, CancellationToken cancellationToken = default);
}
