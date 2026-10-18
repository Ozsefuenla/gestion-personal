namespace GestionPersonal.Application.Account;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword);
