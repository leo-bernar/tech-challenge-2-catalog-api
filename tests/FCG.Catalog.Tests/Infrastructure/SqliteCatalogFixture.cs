using FCG.Catalog.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Tests.Infrastructure;

public sealed class SqliteCatalogFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteCatalogFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public CatalogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
