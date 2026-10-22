namespace GestionPersonal.Application.Monitoring;

public sealed record ExceptionListItem(
    long Id,
    DateTimeOffset OccurredAt,
    string ExceptionType,
    string Message,
    string? RequestPath,
    string? HttpMethod,
    int? StatusCode,
    string? ActorRole,
    int? CenterId);
