using FCG.Catalog.Api.Contracts.Library;

namespace FCG.Catalog.Api.Services;

public interface IUserLibraryService
{
    Task AddApprovedGameAsync(
        Guid orderId,
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LibraryGameResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
