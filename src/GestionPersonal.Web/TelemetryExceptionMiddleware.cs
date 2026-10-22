using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.Web;

public sealed class TelemetryExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public TelemetryExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IExceptionReporter reporter,
        ICurrentUser currentUser)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            reporter.Report(new ReportedException(
                exception.GetType().FullName ?? exception.GetType().Name,
                exception.Message,
                exception.StackTrace,
                context.Request.Path.Value ?? string.Empty,
                context.Request.Method,
                StatusCodes.Status500InternalServerError,
                currentUser.Role,
                currentUser.SubjectId,
                currentUser.CenterId,
                TryGetCorrelationId(context)));

            throw;
        }
    }

    private static Guid? TryGetCorrelationId(HttpContext context) =>
        Guid.TryParse(context.TraceIdentifier, out var id) ? id : null;
}
