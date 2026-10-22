namespace GestionPersonal.Application.Abstractions;

public sealed record RequestMetric(
    string Path,
    string HttpMethod,
    int StatusCode,
    double DurationMs,
    int? CenterId = null);
