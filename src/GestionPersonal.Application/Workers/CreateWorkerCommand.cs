namespace GestionPersonal.Application.Workers;

public sealed record CreateWorkerCommand(string FullName, string Email, string Role);
