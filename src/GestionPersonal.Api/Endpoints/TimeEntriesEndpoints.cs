using FluentValidation;
using GestionPersonal.Api.Extensions;
using GestionPersonal.Application.TimeEntries;

namespace GestionPersonal.Api.Endpoints;

public static class TimeEntriesEndpoints
{
    public static IEndpointRouteBuilder MapTimeEntriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/time-entries/start", async (
            StartTimeEntryCommand command,
            IValidator<StartTimeEntryCommand> validator,
            StartTimeEntryCommandHandler handler,
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

        app.MapPost("/api/time-entries/pause", async (
            PauseTimeEntryCommand command,
            IValidator<PauseTimeEntryCommand> validator,
            PauseTimeEntryCommandHandler handler,
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

        app.MapPost("/api/time-entries/end", async (
            EndTimeEntryCommand command,
            IValidator<EndTimeEntryCommand> validator,
            EndTimeEntryCommandHandler handler,
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

        app.MapPost("/api/time-entries/resume", async (
            ResumeTimeEntryCommand command,
            IValidator<ResumeTimeEntryCommand> validator,
            ResumeTimeEntryCommandHandler handler,
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

        app.MapGet("/api/time-entries/today", async (
            Guid workerId,
            IValidator<TodaySummaryQuery> validator,
            TodaySummaryQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new TodaySummaryQuery(workerId);

            var validationResult = await validator.ValidateAsync(query, cancellationToken);

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(
                    validationResult.Errors
                        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                        .ToDictionary(g => g.Key, g => g.ToArray()));
            }

            var result = await handler.Handle(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblemDetails();
        });

        app.MapPost("/api/time-entries/quick-clock", async (
            QuickClockCommand command,
            IValidator<QuickClockCommand> validator,
            QuickClockCommandHandler handler,
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

        return app;
    }
}
