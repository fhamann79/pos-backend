using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Core.DTOs;
using Pos.Backend.Api.Core.Entities;
using Pos.Backend.Api.Infrastructure.Data;

namespace Pos.Backend.Api.Core.Services;

public class AuthService
{
    private readonly PosDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthService(PosDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Ok, string Error)> RegisterAsync(RegisterDto dto)
    {
        var exists = await _context.Users.AnyAsync(u =>
            u.Username == dto.Username || u.Email == dto.Email);

        if (exists)
            return (false, "UsernameOrEmailAlreadyExists");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            CompanyId = dto.CompanyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "");
    }

    public async Task<User?> ValidateLoginAsync(LoginDto dto)
    {
        // 1) Traer user + Company + Establishment (para validar reglas)
        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.Establishment)
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        // 2) Validación: usuario existe y está activo
        if (user is null || !user.IsActive)
            return null;

        // 3) Validación: empresa existe y está activa
        if (user.Company is null || !user.Company.IsActive)
            return null;

        // 4) Validación: usuario debe tener Establishment asignado
        if (user.EstablishmentId is null)
            return null;

        // 5) Validación: establecimiento existe y está activo
        if (user.Establishment is null || !user.Establishment.IsActive)
            return null;

        // 6) Validación password hash
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        return result == PasswordVerificationResult.Success ? user : null;
    }
}