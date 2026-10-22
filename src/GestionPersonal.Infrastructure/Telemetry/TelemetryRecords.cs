namespace GestionPersonal.Infrastructure.Telemetry;

internal abstract record TelemetryRecord(DateTimeOffset OccurredAt);

internal sealed record TelemetryEventRecord(
    DateTimeOffset OccurredAt,
    string Category,
    string EventType,
    string Severity,
    string? ActorRole,
    int? ActorId,
    int? CenterId,
    string Message,
    string? DataJson,
    Guid? CorrelationId) : TelemetryRecord(OccurredAt);

internal sealed record TelemetryExceptionRecord(
    DateTimeOffset OccurredAt,
    string ExceptionType,
    string Message,
    string? StackTrace,
    string? RequestPath,
    string? HttpMethod,
    int? StatusCode,
    string? ActorRole,
    int? ActorId,
    int? CenterId,
    Guid? CorrelationId) : TelemetryRecord(OccurredAt);

internal sealed record RequestMetricRecord(
    DateTimeOffset OccurredAt,
    string Path,
    string HttpMethod,
    int StatusCode,
    double DurationMs,
    int? CenterId) : TelemetryRecord(OccurredAt);
