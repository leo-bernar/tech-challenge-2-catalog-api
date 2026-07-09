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
            ?? "Server=localhost,1433;Database=FcgCatalogDb;User Id=sa;Password=CatalogSql1!;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CatalogDbContext(options);
    }
}
