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
            return Error.NotFound("El usuario no existe.");
        }

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Error.Validation("La contraseña no es correcta.");
        }

        if (!user.Active)
        {
            return Error.Forbidden("El usuario está desactivado.");
        }

        string centerName = string.Empty;

        if (user.CenterId is { } centerId)
        {
            var center = await _centerRepository.GetByIdAsync(centerId, cancellationToken);

            if (center is null || !center.Active)
            {
                return Error.Forbidden("El centro no está disponible.");
            }

            centerName = center.Name;
        }

        return new CenterSessionResponse(
            user.Id,
            user.CenterId,
            centerName,
            user.Username,
            user.Role,
            user.MustChangePassword);
    }
}
