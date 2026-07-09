using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContextFactory
    : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment
            .GetEnvironmentVariable("ConnectionStrings__CatalogDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'CatalogDatabase' must be provided by ConnectionStrings__CatalogDatabase when running EF Core design-time commands.");

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CatalogDbContext(options);
    }
}
