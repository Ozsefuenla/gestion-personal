using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Common;

namespace GestionPersonal.Application.Services;

public sealed class CentersService : ICentersService
{
    private readonly ListCentersQueryHandler _listCentersHandler;
    private readonly CenterLoginCommandHandler _loginHandler;
    private readonly IRequestValidator _requestValidator;

    public CentersService(
        ListCentersQueryHandler listCentersHandler,
        CenterLoginCommandHandler loginHandler,
        IRequestValidator requestValidator)
    {
        _listCentersHandler = listCentersHandler;
        _loginHandler = loginHandler;
        _requestValidator = requestValidator;
    }

    public Task<Result<IReadOnlyList<CenterResponse>>> ListAsync(CancellationToken cancellationToken = default) =>
        _listCentersHandler.Handle(new ListCentersQuery(), cancellationToken);

    public async Task<Result<CenterSessionResponse>> LoginAsync(
        CenterLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationError = await _requestValidator.ValidateAsync(command, cancellationToken);

        if (validationError is not null)
        {
            return validationError;
        }

        return await _loginHandler.Handle(command, cancellationToken);
    }
}
