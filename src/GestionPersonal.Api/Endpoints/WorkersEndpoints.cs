using FluentValidation;
using GestionPersonal.Api.Extensions;
using GestionPersonal.Application.Workers;

namespace GestionPersonal.Api.Endpoints;

public static class WorkersEndpoints
{
    public static IEndpointRouteBuilder MapWorkersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/workers", async (ListWorkersQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListWorkersQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        app.MapGet("/api/workers/status", async (GetWorkersStatusQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        app.MapGet("/api/workers/{id:guid}/status", async (Guid id, GetWorkerStatusQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(id, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        app.MapPost("/api/workers/login", async (
            LoginCommand command,
            IValidator<LoginCommand> validator,
            LoginCommandHandler handler,
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

        app.MapPost("/api/workers", async (
            CreateWorkerCommand command,
            IValidator<CreateWorkerCommand> validator,
            CreateWorkerCommandHandler handler,
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
                ? Results.Created($"/api/workers/{result.Value.Id}", result.Value)
                : result.ToProblemDetails();
        });

        app.MapPost("/api/workers/{id:guid}/change-pin", async (
            Guid id,
            ChangePinCommand command,
            IValidator<ChangePinCommand> validator,
            ChangePinCommandHandler handler,
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

            var result = await handler.Handle(id, command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblemDetails();
        });

        return app;
    }
}
