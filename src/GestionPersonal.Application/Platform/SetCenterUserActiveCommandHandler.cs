using GestionPersonal.Application.Abstractions;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Platform;

public sealed class SetCenterUserActiveCommandHandler
{
    private readonly ICenterUserRepository _centerUserRepository;
    private readonly ICurrentUser _currentUser;

    public SetCenterUserActiveCommandHandler(ICenterUserRepository centerUserRepository, ICurrentUser currentUser)
    {
        _centerUserRepository = centerUserRepository;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SetCenterUserActiveCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.SubjectId == command.UserId)
        {
            return Error.Validation("No puedes desactivar tu propio usuario.");
        }

        var user = await _centerUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Error.NotFound("El usuario no existe.");
        }

        if (CenterUserAccess.EnsureCanManage(_currentUser, user) is { } accessError)
        {
            return accessError;
        }

        if (command.Active)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await _centerUserRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
