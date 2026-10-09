namespace GestionPersonal.Application.Workers;

public sealed record ChangePinCommand(string CurrentPin, string NewPin);
