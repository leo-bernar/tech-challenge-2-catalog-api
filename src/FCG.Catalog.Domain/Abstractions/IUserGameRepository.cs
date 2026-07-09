using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Abstractions;

public interface IUserGameRepository
{
    Task AddAsync(UserGame userGame, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserGame>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
