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
        }).RequireAuthorization();

        app.MapGet("/api/workers/status", async (GetWorkersStatusQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapGet("/api/workers/manage", async (ListWorkersForAdminQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListWorkersForAdminQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapGet("/api/workers/{id:int}/status", async (int id, GetWorkerStatusQueryHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(id, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapPost("/api/workers/login", async (
            LoginCommand command,
            IValidator<LoginCommand> validator,
            LoginCommandHandler handler,
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
                loggerFactory.CreateLogger("WorkersLogin").LogWarning(
                    "Fallo de login de trabajador {WorkerId}: {Error}", command.WorkerId, result.Error.Message);
            }

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireRateLimiting("login").RequireAuthorization();

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
        }).RequireAuthorization();

        app.MapPost("/api/workers/{id:int}/change-pin", async (
            int id,
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
        }).RequireAuthorization();

        app.MapPatch("/api/workers/{id:int}/name", async (
            int id,
            RenameWorkerCommand command,
            IValidator<RenameWorkerCommand> validator,
            RenameWorkerCommandHandler handler,
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
        }).RequireAuthorization();

        app.MapPost("/api/workers/{id:int}/reset-pin", async (
            int id,
            ResetWorkerPinCommand command,
            IValidator<ResetWorkerPinCommand> validator,
            ResetWorkerPinCommandHandler handler,
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
        }).RequireAuthorization();

        app.MapPatch("/api/workers/{id:int}/activate", async (
            int id,
            ActivateWorkerCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(id, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapPatch("/api/workers/{id:int}/deactivate", async (
            int id,
            DeactivateWorkerCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(id, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblemDetails();
        }).RequireAuthorization();

        return app;
    }
}
