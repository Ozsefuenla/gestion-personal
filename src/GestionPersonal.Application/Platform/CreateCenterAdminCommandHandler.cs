using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Platform;

public sealed class CreateCenterAdminCommandHandler
{
    private readonly ICenterRepository _centerRepository;
    private readonly ICenterUserRepository _centerUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public CreateCenterAdminCommandHandler(
        ICenterRepository centerRepository,
        ICenterUserRepository centerUserRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _centerRepository = centerRepository;
        _centerUserRepository = centerUserRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result<CenterUserResponse>> Handle(CreateCenterAdminCommand command, CancellationToken cancellationToken = default)
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

        if (await _centerUserRepository.GetByUsernameAsync(command.Username.Trim(), cancellationToken) is not null)
        {
            return Error.Conflict("El nombre de usuario ya está en uso.");
        }

        var user = CenterUser.Create(0, center.Id, command.Username, _passwordHasher.Hash(command.Password), UserRole.CenterAdmin, mustChangePassword: true);
        var persisted = await _centerUserRepository.AddAsync(user, cancellationToken);

        return CenterUserResponse.From(persisted);
    }
}
