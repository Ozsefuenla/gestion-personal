namespace GestionPersonal.Application.Platform;

public sealed record ResetCenterUserPasswordCommand(int UserId, string Password);
