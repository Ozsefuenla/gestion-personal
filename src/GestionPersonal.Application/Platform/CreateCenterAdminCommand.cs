namespace GestionPersonal.Application.Platform;

public sealed record CreateCenterAdminCommand(int CenterId, string Username, string Password);
