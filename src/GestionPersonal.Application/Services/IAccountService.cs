using GestionPersonal.Application.Account;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Services;

public interface IAccountService
{
    Task<Result> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default);
}
