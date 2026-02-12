using Microsoft.EntityFrameworkCore;
using Pos.Backend.Api.Infrastructure.Data;

namespace Pos.Backend.Api.Tests;

public static class TestDbContextFactory
{
    public static PosDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        return new PosDbContext(options);
    }
}
