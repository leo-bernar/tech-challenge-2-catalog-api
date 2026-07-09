using FCG.Catalog.Domain.Abstractions;
using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Persistence.Repositories;

public sealed class GameRepository(CatalogDbContext context) : IGameRepository
{
    public async Task AddAsync(
        Game game,
        CancellationToken cancellationToken = default)
    {
        await context.Games.AddAsync(game, cancellationToken);
    }

    public Task<Game?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        context.Games.FirstOrDefaultAsync(game => game.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Game>> ListAsync(
        CancellationToken cancellationToken = default) =>
        await context.Games
            .AsNoTracking()
            .Where(game => game.IsActive)
            .OrderBy(game => game.Title)
            .ToListAsync(cancellationToken);
}
