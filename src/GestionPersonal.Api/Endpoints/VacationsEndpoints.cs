using FluentValidation;
using GestionPersonal.Api.Extensions;
using GestionPersonal.Application.Vacations;

namespace GestionPersonal.Api.Endpoints;

public static class VacationsEndpoints
{
    public static IEndpointRouteBuilder MapVacationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/vacations", async (
            int workerId,
            ListVacationsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListVacationsQuery(workerId), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapGet("/api/vacations/center", async (
            ListCenterVacationsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new ListCenterVacationsQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapPost("/api/vacations", async (
            RequestVacationCommand command,
            IValidator<RequestVacationCommand> validator,
            RequestVacationCommandHandler handler,
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
                ? Results.Created($"/api/vacations/{result.Value.Id}", result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapDelete("/api/vacations/{id:guid}", async (
            Guid id,
            DeleteVacationCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(id, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapPatch("/api/vacations/{id:guid}/approve", async (
            Guid id,
            ApproveVacationCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(id, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        app.MapPatch("/api/vacations/{id:guid}/reject", async (
            Guid id,
            RejectVacationCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(id, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        }).RequireAuthorization();

        return app;
    }
}
