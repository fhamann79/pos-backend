using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Core.Entities;

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
        const string adminRoleCode = "ADMIN";

        var roleDefinitions = new[]
        {
            new { Code = "ADMIN", Name = "Administrador" },
            new { Code = "SUPERVISOR", Name = "Supervisor" },
            new { Code = "CASHIER", Name = "Cajero" }
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
            .FirstAsync(r => r.Code == adminRoleCode);

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

        var adminUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == adminUsername);

        var hasher = new PasswordHasher<User>();

        if (adminUser is null)
        {
            adminUser = new User
            {
                Username = adminUsername,
                Email = adminEmail,
                CompanyId = company.Id,
                RoleId = adminRole.Id,
                EstablishmentId = establishment.Id,
                EmissionPointId = emissionPoint.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            return;
        }

        var needsUpdate = false;

        if (adminUser.CompanyId <= 0)
        {
            adminUser.CompanyId = company.Id;
            needsUpdate = true;
        }

        if (adminUser.RoleId <= 0)
        {
            adminUser.RoleId = adminRole.Id;
            needsUpdate = true;
        }

        if (adminUser.EstablishmentId is null)
        {
            adminUser.EstablishmentId = establishment.Id;
            needsUpdate = true;
        }

        var adminEstablishmentId = adminUser.EstablishmentId ?? establishment.Id;

        if (adminUser.EmissionPointId <= 0)
        {
            var adminEmissionPoint = await context.EmissionPoints
                .FirstOrDefaultAsync(e => e.EstablishmentId == adminEstablishmentId && e.Code == emissionPointCode);

            if (adminEmissionPoint is null)
            {
                adminEmissionPoint = new EmissionPoint
                {
                    EstablishmentId = adminEstablishmentId,
                    Code = emissionPointCode,
                    Name = "Caja Principal",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.EmissionPoints.Add(adminEmissionPoint);
                await context.SaveChangesAsync();
            }

            adminUser.EmissionPointId = adminEmissionPoint.Id;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            await context.SaveChangesAsync();
        }
    }
}
