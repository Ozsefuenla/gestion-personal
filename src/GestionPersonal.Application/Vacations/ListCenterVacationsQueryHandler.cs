using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Vacations;

public sealed class ListCenterVacationsQueryHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly ICurrentUser _currentUser;

    public ListCenterVacationsQueryHandler(
        IWorkerRepository workerRepository,
        IVacationRepository vacationRepository,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _vacationRepository = vacationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<VacationResponse>>> Handle(
        ListCenterVacationsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var workers = await _workerRepository.GetByCenterAsync(_currentUser.CenterId!.Value, cancellationToken);
        var workerIds = workers.Select(w => w.Id).ToList();

        var vacations = await _vacationRepository.GetByWorkerIdsAsync(workerIds, cancellationToken);

        IReadOnlyList<VacationResponse> response = vacations
            .Select(VacationResponse.From)
            .ToList();

        return Result<IReadOnlyList<VacationResponse>>.Success(response);
    }
}
