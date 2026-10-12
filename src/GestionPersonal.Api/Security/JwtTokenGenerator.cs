using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionPersonal.Application.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace GestionPersonal.Api.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Generate(int centerId, string centerName, string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, centerId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim("centerId", centerId.ToString()),
            new Claim("centerName", centerName),
            new Claim(ClaimTypes.Role, "center")
        };

        var expirationHours = int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 12;

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "GestionPersonal",
            audience: _configuration["Jwt:Audience"] ?? "GestionPersonal",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
