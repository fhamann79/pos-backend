using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Core.Entities;
using Pos.Backend.Api.Core.Security;

namespace Pos.Backend.Api.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedDevelopmentAsync(PosDbContext context)
    {
        const string companyRuc = "9999999999001";
        const string companyName = "Demo Company";
        const string establishmentCode = "001";
        const string emissionPointCode = "001";
        const string adminUsername = "admin";
        const string adminEmail = "admin@demo.local";
        const string adminPassword = "admin123";
        const string superUsername = "super";
        const string superEmail = "super@demo.local";
        const string superPassword = "super123";
        const string cashierUsername = "cashier";
        const string cashierEmail = "cashier@demo.local";
        const string cashierPassword = "cashier123";

        var roleDefinitions = new[]
        {
            new { Code = AppRoles.Admin, Name = "Administrador" },
            new { Code = AppRoles.Supervisor, Name = "Supervisor" },
            new { Code = AppRoles.Cashier, Name = "Cajero" }
        };

        foreach (var roleDefinition in roleDefinitions)
        {
            var exists = await context.Roles
                .AnyAsync(r => r.Code == roleDefinition.Code);

            if (!exists)
            {
                context.Roles.Add(new Role
                {
                    Code = roleDefinition.Code,
                    Name = roleDefinition.Name,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();

        var adminRole = await context.Roles
            .FirstAsync(r => r.Code == AppRoles.Admin);

        var supervisorRole = await context.Roles
            .FirstAsync(r => r.Code == AppRoles.Supervisor);

        var cashierRole = await context.Roles
            .FirstAsync(r => r.Code == AppRoles.Cashier);

        var company = await context.Companies
            .FirstOrDefaultAsync(c => c.Ruc == companyRuc);

        if (company is null)
        {
            company = new Company
            {
                Name = companyName,
                Ruc = companyRuc,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Companies.Add(company);
            await context.SaveChangesAsync();
        }

        var establishment = await context.Establishments
            .FirstOrDefaultAsync(e => e.CompanyId == company.Id && e.Code == establishmentCode);

        if (establishment is null)
        {
            establishment = new Establishment
            {
                CompanyId = company.Id,
                Code = establishmentCode,
                Name = "Matriz",
                Address = "Direccion principal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Establishments.Add(establishment);
            await context.SaveChangesAsync();
        }

        var emissionPoint = await context.EmissionPoints
            .FirstOrDefaultAsync(e => e.EstablishmentId == establishment.Id && e.Code == emissionPointCode);

        if (emissionPoint is null)
        {
            emissionPoint = new EmissionPoint
            {
                EstablishmentId = establishment.Id,
                Code = emissionPointCode,
                Name = "Caja Principal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.EmissionPoints.Add(emissionPoint);
            await context.SaveChangesAsync();
        }

        var hasher = new PasswordHasher<User>();

        async Task EnsureDemoUserAsync(string username, string email, string password, Role role)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
            {
                user = new User
                {
                    Username = username,
                    Email = email,
                    CompanyId = company.Id,
                    RoleId = role.Id,
                    EstablishmentId = establishment.Id,
                    EmissionPointId = emissionPoint.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.PasswordHash = hasher.HashPassword(user, password);
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return;
            }

            var needsUpdate = false;

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                user.Email = email;
                needsUpdate = true;
            }

            if (user.CompanyId <= 0)
            {
                user.CompanyId = company.Id;
                needsUpdate = true;
            }

            if (user.RoleId != role.Id)
            {
                user.RoleId = role.Id;
                needsUpdate = true;
            }

            if (user.EstablishmentId is null)
            {
                user.EstablishmentId = establishment.Id;
                needsUpdate = true;
            }

            if (user.EmissionPointId <= 0)
            {
                user.EmissionPointId = emissionPoint.Id;
                needsUpdate = true;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await context.SaveChangesAsync();
            }
        }

        await EnsureDemoUserAsync(adminUsername, adminEmail, adminPassword, adminRole);
        await EnsureDemoUserAsync(superUsername, superEmail, superPassword, supervisorRole);
        await EnsureDemoUserAsync(cashierUsername, cashierEmail, cashierPassword, cashierRole);
    }
}
