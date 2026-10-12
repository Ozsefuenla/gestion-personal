using FluentValidation;
using GestionPersonal.Api.Extensions;
using GestionPersonal.Application.Centers;
using GestionPersonal.Application.Workers;

namespace GestionPersonal.Api.Endpoints;

public static class CentersEndpoints
{
    public static IEndpointRouteBuilder MapCentersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/centers", async (ListCentersQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListCentersQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        app.MapPost("/api/centers/login", async (
            CenterLoginCommand command,
            IValidator<CenterLoginCommand> validator,
            CenterLoginCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(
                    validationResult.Errors
                        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                        .ToDictionary(g => g.Key, g => g.ToArray()));
            }

            var result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        app.MapGet("/api/centers/{id:int}/workers", async (int id, ListWorkersByCenterQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListWorkersByCenterQuery(id), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        return app;
    }
}
