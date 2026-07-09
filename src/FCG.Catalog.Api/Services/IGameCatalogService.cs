using FCG.Catalog.Api.Contracts.Games;

namespace FCG.Catalog.Api.Services;

public interface IGameCatalogService
{
    Task<GameResponse> CreateAsync(
        CreateGameRequest request,
        CancellationToken cancellationToken = default);

    Task<GameResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameResponse>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<GameResponse> UpdateAsync(
        Guid id,
        UpdateGameRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
