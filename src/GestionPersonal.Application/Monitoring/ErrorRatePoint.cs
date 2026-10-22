namespace GestionPersonal.Application.Monitoring;

public sealed record ErrorRatePoint(DateTimeOffset Hour, int Requests, int Errors);
