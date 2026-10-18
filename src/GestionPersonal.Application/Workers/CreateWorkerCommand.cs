namespace GestionPersonal.Application.Workers;

public sealed record CreateWorkerCommand(string FullName, string Pin, int CenterId, bool MustChangePin = false);
