namespace GestionPersonal.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string Generate(int centerId, string centerName, string username);
}
