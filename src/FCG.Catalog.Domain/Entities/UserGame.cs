using FCG.Catalog.Domain.Common;

namespace FCG.Catalog.Domain.Entities;

public sealed class UserGame
{
    private UserGame()
    {
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public Guid OrderId { get; private set; }
    public DateTime AcquiredAtUtc { get; private set; }
    public Game Game { get; private set; } = null!;

    public static UserGame Create(
        Guid userId,
        Guid gameId,
        Guid orderId,
        DateTime acquiredAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("UserId is required.");
        }

        if (gameId == Guid.Empty)
        {
            throw new DomainValidationException("GameId is required.");
        }

        if (orderId == Guid.Empty)
        {
            throw new DomainValidationException("OrderId is required.");
        }

        if (acquiredAtUtc == default)
        {
            throw new DomainValidationException("Acquisition date is required.");
        }

        return new UserGame
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GameId = gameId,
            OrderId = orderId,
            AcquiredAtUtc = DateTime.SpecifyKind(acquiredAtUtc, DateTimeKind.Utc)
        };
    }
}
