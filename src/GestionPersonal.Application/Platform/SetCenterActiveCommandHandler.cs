using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Platform;

public sealed class SetCenterActiveCommandHandler
{
    private readonly ICenterRepository _centerRepository;
    private readonly ICurrentUser _currentUser;

    public SetCenterActiveCommandHandler(ICenterRepository centerRepository, ICurrentUser currentUser)
    {
        _centerRepository = centerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetCenterActiveCommand command, CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireSuperAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var center = await _centerRepository.GetByIdAsync(command.CenterId, cancellationToken);

        if (center is null)
        {
            return Error.NotFound("El centro no existe.");
        }

        if (command.Active)
        {
            center.Activate();
        }
        else
        {
            center.Deactivate();
        }

        await _centerRepository.UpdateAsync(center, cancellationToken);

        return Result.Success();
    }
}
