using FCG.Catalog.Domain.Abstractions;
using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Persistence.Repositories;

public sealed class UserGameRepository(CatalogDbContext context)
    : IUserGameRepository
{
    public async Task AddAsync(
        UserGame userGame,
        CancellationToken cancellationToken = default)
    {
        await context.UserGames.AddAsync(userGame, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken = default) =>
        context.UserGames.AnyAsync(
            userGame => userGame.UserId == userId && userGame.GameId == gameId,
            cancellationToken);

    public async Task<IReadOnlyList<UserGame>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await context.UserGames
            .AsNoTracking()
            .Include(userGame => userGame.Game)
            .Where(userGame => userGame.UserId == userId)
            .OrderBy(userGame => userGame.AcquiredAtUtc)
            .ToListAsync(cancellationToken);
}
