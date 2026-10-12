namespace GestionPersonal.Application.Workers;

public sealed record LoginCommand(int WorkerId, string Pin);
