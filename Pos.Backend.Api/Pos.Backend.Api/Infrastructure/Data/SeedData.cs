using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Core.Entities;

namespace Pos.Backend.Api.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedDevelopmentAsync(PosDbContext context)
    {
        var company = await context.Companies.FirstOrDefaultAsync();
        if (company is null)
        {
            company = new Company
            {
                Name = "Demo Company",
                Ruc = "0999999999001",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Companies.Add(company);
            await context.SaveChangesAsync();
        }
        else if (!company.IsActive)
        {
            company.IsActive = true;
            await context.SaveChangesAsync();
        }

        var establishment = await context.Establishments
            .FirstOrDefaultAsync(e => e.CompanyId == company.Id);
        if (establishment is null)
        {
            establishment = new Establishment
            {
                CompanyId = company.Id,
                Code = "001",
                Name = "Establecimiento Matriz",
                Address = "Av. Principal 123",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Establishments.Add(establishment);
            await context.SaveChangesAsync();
        }
        else if (!establishment.IsActive)
        {
            establishment.IsActive = true;
            await context.SaveChangesAsync();
        }

        var emissionPoint = await context.EmissionPoints
            .FirstOrDefaultAsync(e => e.EstablishmentId == establishment.Id && e.Code == "001");
        if (emissionPoint is null)
        {
            emissionPoint = new EmissionPoint
            {
                EstablishmentId = establishment.Id,
                Code = "001",
                Name = "Caja Principal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.EmissionPoints.Add(emissionPoint);
            await context.SaveChangesAsync();
        }
        else if (!emissionPoint.IsActive)
        {
            emissionPoint.IsActive = true;
            await context.SaveChangesAsync();
        }

        var demoUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "demo");
        if (demoUser is null)
        {
            var hasher = new PasswordHasher<User>();
            demoUser = new User
            {
                Username = "demo",
                Email = "demo@pos.local",
                CompanyId = company.Id,
                EstablishmentId = establishment.Id,
                EmissionPointId = emissionPoint.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            demoUser.PasswordHash = hasher.HashPassword(demoUser, "Demo123!");
            context.Users.Add(demoUser);
            await context.SaveChangesAsync();
        }
        else
        {
            var updated = false;

            if (demoUser.CompanyId != company.Id)
            {
                demoUser.CompanyId = company.Id;
                updated = true;
            }

            if (demoUser.EstablishmentId is null)
            {
                demoUser.EstablishmentId = establishment.Id;
                updated = true;
            }

            if (demoUser.EmissionPointId <= 0)
            {
                demoUser.EmissionPointId = emissionPoint.Id;
                updated = true;
            }

            if (!demoUser.IsActive)
            {
                demoUser.IsActive = true;
                updated = true;
            }

            if (updated)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
