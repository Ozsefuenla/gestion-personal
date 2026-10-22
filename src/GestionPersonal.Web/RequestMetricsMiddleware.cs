using System.Diagnostics;
using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.Web;

public sealed class RequestMetricsMiddleware
{
    private readonly RequestDelegate _next;

    public RequestMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRequestMetricsRecorder recorder,
        ICurrentUser currentUser)
    {
        if (!ShouldRecord(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var statusCode = StatusCodes.Status200OK;

        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode;
        }
        catch
        {
            statusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            recorder.Record(new RequestMetric(
                context.Request.Path.Value ?? string.Empty,
                context.Request.Method,
                statusCode,
                stopwatch.Elapsed.TotalMilliseconds,
                currentUser.CenterId));
        }
    }

    private static bool ShouldRecord(PathString path)
    {
        var value = path.Value ?? string.Empty;

        return !value.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("/_content", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("/images", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);
    }
}
