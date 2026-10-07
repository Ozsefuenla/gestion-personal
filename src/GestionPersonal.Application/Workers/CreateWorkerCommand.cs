namespace GestionPersonal.Application.Workers;

public sealed record CreateWorkerCommand(string FullName, string Pin, string Role);
