using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity;

public sealed class JwtTokenService(IConfiguration cfg) : ITokenService
{
    public TokenResponse CreateToken(AppUser user)
    {
        var expires = DateTime.UtcNow.AddMinutes(cfg.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 15);
        var signingKey = cfg["Jwt:Key"] ?? throw new InvalidOperationException("JWT key missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var jwt = new JwtSecurityToken(
            cfg["Jwt:Issuer"],
            cfg["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new TokenResponse(accessToken, refreshToken, expires, user.Role, user.UserName);
    }
}
