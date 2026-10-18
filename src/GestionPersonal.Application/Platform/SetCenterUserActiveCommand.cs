namespace GestionPersonal.Application.Platform;

public sealed record SetCenterUserActiveCommand(int UserId, bool Active);
