using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pos.Backend.Api.Core.Entities;
using Pos.Backend.Api.Core.Security;
using Pos.Backend.Api.Infrastructure.Data;

namespace Pos.Backend.Api.Core.Services;

public class JwtService
{
    private readonly IConfiguration _config;
    private readonly PosDbContext _context;

    public JwtService(IConfiguration config, PosDbContext context)
    {
        _config = config;
        _context = context;
    }

    public string GenerateToken(User user)
    {
        var roleCode = user.Role?.Code;

        if (string.IsNullOrWhiteSpace(roleCode) && user.RoleId > 0)
        {
            roleCode = _context.Roles
                .AsNoTracking()
                .Where(r => r.Id == user.RoleId)
                .Select(r => r.Code)
                .FirstOrDefault();
        }

        var permissions = _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == user.RoleId && rp.Permission.IsActive)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim("companyId", user.CompanyId.ToString()),
            new Claim("establishmentId", user.EstablishmentId!.Value.ToString()),
            new Claim("emissionPointId", user.EmissionPointId.ToString()),
            new Claim(ClaimTypes.Role, roleCode ?? string.Empty)
        };

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(AppClaims.Permission, permission));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"])
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:ExpiresMinutes"])
            ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
