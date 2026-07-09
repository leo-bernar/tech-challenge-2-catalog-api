using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Domain.Abstractions;

public interface IGameRepository
{
    Task AddAsync(Game game, CancellationToken cancellationToken = default);
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> ListAsync(CancellationToken cancellationToken = default);
}
