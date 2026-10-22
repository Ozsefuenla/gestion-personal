using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Abstractions;

public sealed record ReportedException(
    string ExceptionType,
    string Message,
    string? StackTrace = null,
    string? RequestPath = null,
    string? HttpMethod = null,
    int? StatusCode = null,
    UserRole? ActorRole = null,
    int? ActorId = null,
    int? CenterId = null,
    Guid? CorrelationId = null);
