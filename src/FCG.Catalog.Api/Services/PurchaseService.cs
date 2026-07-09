using FCG.Catalog.Api.Contracts.Purchases;
using FCG.Catalog.Api.Messaging;
using FCG.Catalog.Domain.Abstractions;
using FCG.Catalog.Domain.Common;
using FCG.IntegrationEvents.V1;

namespace FCG.Catalog.Api.Services;

public sealed class PurchaseService(
    IGameRepository gameRepository,
    IUserGameRepository userGameRepository,
    IOrderPlacedPublisher publisher,
    TimeProvider timeProvider,
    ILogger<PurchaseService> logger)
    : IPurchaseService
{
    public async Task<PurchaseResponse> RequestPurchaseAsync(
        Guid userId,
        string userEmail,
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("UserId is required.");
        }

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new DomainValidationException("UserEmail is required.");
        }

        var game = await gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null || !game.IsActive)
        {
            throw new DomainValidationException("Game not found.");
        }

        if (await userGameRepository.ExistsAsync(userId, gameId, cancellationToken))
        {
            throw new DomainConflictException("Game is already in your library.");
        }

        var order = new OrderPlacedEvent(
            Guid.NewGuid(),
            timeProvider.GetUtcNow().UtcDateTime,
            userId,
            userEmail,
            game.Id,
            game.Price);

        await publisher.PublishAsync(order, cancellationToken);

        logger.LogInformation(
            "Order placed. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Price: {Price}",
            order.OrderId,
            order.UserId,
            order.GameId,
            order.Price);

        return new PurchaseResponse(
            order.OrderId,
            order.GameId,
            order.Price,
            "Processing");
    }
}
