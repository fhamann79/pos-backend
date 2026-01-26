using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Core.Entities;

namespace Pos.Backend.Api.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedDevelopmentAsync(PosDbContext context)
    {
        if (await context.EmissionPoints.AnyAsync())
        {
            return;
        }

        var establishment = await context.Establishments.FirstOrDefaultAsync();
        if (establishment is null)
        {
            return;
        }

        var emissionPoint = new EmissionPoint
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
}
