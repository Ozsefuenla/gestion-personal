using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Centers;

public sealed class CenterLoginCommandHandler
{
    private readonly ICenterUserRepository _centerUserRepository;
    private readonly ICenterRepository _centerRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CenterLoginCommandHandler(
        ICenterUserRepository centerUserRepository,
        ICenterRepository centerRepository,
        IPasswordHasher passwordHasher)
    {
        _centerUserRepository = centerUserRepository;
        _centerRepository = centerRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<CenterSessionResponse>> Handle(
        CenterLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _centerUserRepository.GetByUsernameAsync(command.Username.Trim(), cancellationToken);

        if (user is null)
        {
            return Error.NotFound("El usuario del centro no existe.");
        }

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Error.Validation("La contraseña no es correcta.");
        }

        var center = await _centerRepository.GetByIdAsync(user.CenterId, cancellationToken);

        return new CenterSessionResponse(user.CenterId, center?.Name ?? "", user.Username);
    }
}
