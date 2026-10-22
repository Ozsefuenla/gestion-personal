using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Application.Abstractions;

public sealed record AppEvent(
    string Category,
    string EventType,
    string Severity,
    string Message,
    UserRole? ActorRole = null,
    int? ActorId = null,
    int? CenterId = null,
    IReadOnlyDictionary<string, object?>? Data = null,
    Guid? CorrelationId = null);
