using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Entities;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Platform;

public sealed class CreateCenterUserCommandHandler
{
    private readonly ICenterUserRepository _centerUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public CreateCenterUserCommandHandler(
        ICenterUserRepository centerUserRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _centerUserRepository = centerUserRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result<CenterUserResponse>> Handle(
        CreateCenterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (AccessControl.RequireCenterAdmin(_currentUser) is { } error)
        {
            return error;
        }

        if (await _centerUserRepository.GetByUsernameAsync(command.Username.Trim(), cancellationToken) is not null)
        {
            return Error.Conflict("El nombre de usuario ya está en uso.");
        }

        var user = CenterUser.Create(
            0,
            _currentUser.CenterId!.Value,
            command.Username,
            _passwordHasher.Hash(command.Password),
            UserRole.Center,
            mustChangePassword: true);

        var persisted = await _centerUserRepository.AddAsync(user, cancellationToken);

        return CenterUserResponse.From(persisted);
    }
}
