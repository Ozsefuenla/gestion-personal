using FluentValidation;
using GestionPersonal.Api.Extensions;
using GestionPersonal.Application.Abstractions;
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
        }).RequireAuthorization();

        app.MapPost("/api/centers/login", async (
            CenterLoginCommand command,
            IValidator<CenterLoginCommand> validator,
            CenterLoginCommandHandler handler,
            IJwtTokenGenerator tokenGenerator,
            ILoggerFactory loggerFactory,
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

            if (result.IsFailure)
            {
                loggerFactory.CreateLogger("CentersLogin").LogWarning(
                    "Fallo de login de centro '{Username}': {Error}", command.Username, result.Error.Message);

                return result.ToProblemDetails();
            }

            var session = result.Value;
            var token = tokenGenerator.Generate(session.CenterId, session.CenterName, session.Username);

            return Results.Ok(new CenterLoginResponse(token, session.CenterId, session.CenterName, session.Username));
        }).RequireRateLimiting("login");

        app.MapGet("/api/centers/{id:int}/workers", async (int id, ListWorkersByCenterQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListWorkersByCenterQuery(id), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        return app;
    }
}
