using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace GestionPersonal.Application.Common;

public sealed class RequestValidator : IRequestValidator
{
    private readonly IServiceProvider _serviceProvider;

    public RequestValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Error?> ValidateAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class
    {
        var validator = _serviceProvider.GetService<IValidator<TRequest>>();

        if (validator is null)
        {
            return null;
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return null;
        }

        var message = validationResult.Errors
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error));

        return Error.Validation(message ?? "Los datos introducidos no son válidos.");
    }
}
