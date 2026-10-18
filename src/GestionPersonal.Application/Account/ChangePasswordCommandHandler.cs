using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;
using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Account;

public sealed class ChangePasswordCommandHandler
{
    private readonly ICenterUserRepository _centerUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public ChangePasswordCommandHandler(
        ICenterUserRepository centerUserRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _centerUserRepository = centerUserRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.SubjectId;

        if (userId is null || _currentUser.Role is null || _currentUser.Role == UserRole.CenterWorker)
        {
            return Error.Forbidden("No autorizado.");
        }

        var user = await _centerUserRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("El usuario no existe.");
        }

        if (!user.Active)
        {
            return Error.Forbidden("El usuario está desactivado.");
        }

        if (!_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            return Error.Validation("La contraseña actual no es correcta.");
        }

        if (command.NewPassword == command.CurrentPassword)
        {
            return Error.Validation("La nueva contraseña debe ser distinta de la actual.");
        }

        user.ChangePassword(_passwordHasher.Hash(command.NewPassword));

        await _centerUserRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
