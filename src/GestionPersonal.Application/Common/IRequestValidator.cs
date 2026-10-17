namespace GestionPersonal.Application.Common;

public interface IRequestValidator
{
    Task<Error?> ValidateAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class;
}
