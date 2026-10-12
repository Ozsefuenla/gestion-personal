namespace GestionPersonal.Web.Models;

public sealed record CenterSessionResponse(string Token, int CenterId, string CenterName, string Username);
