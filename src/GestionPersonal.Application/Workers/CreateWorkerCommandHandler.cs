using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Workers;

public sealed class CreateWorkerCommandHandler
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ICenterRepository _centerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public CreateWorkerCommandHandler(
        IWorkerRepository workerRepository,
        ICenterRepository centerRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _workerRepository = workerRepository;
        _centerRepository = centerRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkerResponse>> Handle(
        CreateWorkerCommand command,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterAdmin(_currentUser) is { } error)
        {
            return error;
        }

        if (command.CenterId != _currentUser.CenterId!.Value)
        {
            return Error.Forbidden("No puedes crear trabajadores en otro centro.");
        }

        var center = await _centerRepository.GetByIdAsync(command.CenterId, cancellationToken);
        if (center is null)
        {
            return Error.NotFound("El centro no existe.");
        }

        var worker = Worker.Create(command.CenterId, command.FullName, _passwordHasher.Hash(command.Pin), mustChangePin: command.MustChangePin);

        var persisted = await _workerRepository.AddAsync(worker, cancellationToken);

        return WorkerResponse.From(persisted);
    }
}
