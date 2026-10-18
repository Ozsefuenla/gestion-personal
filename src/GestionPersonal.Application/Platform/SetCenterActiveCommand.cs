namespace GestionPersonal.Application.Platform;

public sealed record SetCenterActiveCommand(int CenterId, bool Active);
