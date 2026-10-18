using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Platform;

public sealed class ResetCenterUserPasswordCommandHandler
{
    private readonly ICenterUserRepository _centerUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public ResetCenterUserPasswordCommandHandler(
        ICenterUserRepository centerUserRepository,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _centerUserRepository = centerUserRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        ResetCenterUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _centerUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("El usuario no existe.");
        }

        if (CenterUserAccess.EnsureCanManage(_currentUser, user) is { } error)
        {
            return error;
        }

        user.ResetPassword(_passwordHasher.Hash(command.Password));

        await _centerUserRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
