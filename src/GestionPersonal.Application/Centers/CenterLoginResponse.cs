namespace GestionPersonal.Application.Centers;

public sealed record CenterLoginResponse(string Token, int CenterId, string CenterName, string Username);
