using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;

namespace GestionPersonal.Application.Platform;

public sealed class CreateCenterCommandHandler
{
    private readonly ICenterRepository _centerRepository;
    private readonly ICurrentUser _currentUser;

    public CreateCenterCommandHandler(ICenterRepository centerRepository, ICurrentUser currentUser)
    {
        _centerRepository = centerRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CenterResponse>> Handle(CreateCenterCommand command, CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireSuperAdmin(_currentUser) is { } error)
        {
            return error;
        }

        var center = Center.Create(0, command.Name);
        var persisted = await _centerRepository.AddAsync(center, cancellationToken);

        return CenterResponse.From(persisted);
    }
}
