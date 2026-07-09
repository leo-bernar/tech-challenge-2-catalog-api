using FCG.Catalog.Api.Contracts.Library;
using FCG.Catalog.Domain.Abstractions;
using FCG.Catalog.Domain.Common;
using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Api.Services;

public sealed class UserLibraryService(
    IGameRepository gameRepository,
    IUserGameRepository userGameRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IUserLibraryService
{
    public async Task AddApprovedGameAsync(
        Guid orderId,
        Guid userId,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        if (await userGameRepository.ExistsAsync(userId, gameId, cancellationToken))
        {
            return;
        }

        var game = await gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null || !game.IsActive)
        {
            throw new DomainValidationException("Game not found.");
        }

        var userGame = UserGame.Create(
            userId,
            gameId,
            orderId,
            timeProvider.GetUtcNow().UtcDateTime);

        await userGameRepository.AddAsync(userGame, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LibraryGameResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userGames = await userGameRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        return userGames
            .Where(userGame => userGame.Game.IsActive)
            .Select(GameMappings.ToLibraryResponse)
            .ToList();
    }
}
