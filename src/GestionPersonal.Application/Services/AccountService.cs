using GestionPersonal.Application.Account;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Services;

public sealed class AccountService : IAccountService
{
    private readonly ChangePasswordCommandHandler _changePasswordHandler;
    private readonly IRequestValidator _requestValidator;

    public AccountService(
        ChangePasswordCommandHandler changePasswordHandler,
        IRequestValidator requestValidator)
    {
        _changePasswordHandler = changePasswordHandler;
        _requestValidator = requestValidator;
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        return validationError ?? await _changePasswordHandler.Handle(command, cancellationToken);
    }
}
