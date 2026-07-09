using FCG.Catalog.Api.Consumers;
using FCG.Catalog.Api.Services;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Persistence.Repositories;
using FCG.Catalog.Tests.Infrastructure;
using FCG.IntegrationEvents.V1;
using Microsoft.Extensions.Logging.Abstractions;

namespace FCG.Catalog.Tests.Consumers;

public sealed class PaymentProcessedConsumerTests
{
    [Fact]
    public async Task ConsumeAsync_adds_game_when_payment_is_approved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var fixture = new SqliteCatalogFixture();
        await using var context = fixture.CreateContext();
        var game = Game.Create(
            59.90m,
            "Game",
            null,
            null,
            DateTime.UtcNow);
        await context.Games.AddAsync(game, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var consumer = CreateConsumer(context);
        var userId = Guid.NewGuid();

        await consumer.ConsumeAsync(new PaymentProcessedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            userId,
            "user@example.com",
            game.Id,
            game.Price,
            PaymentStatuses.Approved), cancellationToken);

        Assert.True(await new UserGameRepository(context)
            .ExistsAsync(userId, game.Id, cancellationToken));
    }

    [Fact]
    public async Task ConsumeAsync_does_not_add_game_when_payment_is_rejected()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var fixture = new SqliteCatalogFixture();
        await using var context = fixture.CreateContext();
        var game = Game.Create(
            150.00m,
            "Game",
            null,
            null,
            DateTime.UtcNow);
        await context.Games.AddAsync(game, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var consumer = CreateConsumer(context);
        var userId = Guid.NewGuid();

        await consumer.ConsumeAsync(new PaymentProcessedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            userId,
            "user@example.com",
            game.Id,
            game.Price,
            PaymentStatuses.Rejected), cancellationToken);

        Assert.False(await new UserGameRepository(context)
            .ExistsAsync(userId, game.Id, cancellationToken));
    }

    [Theory]
    [MemberData(nameof(InvalidMessages))]
    public async Task ConsumeAsync_rejects_malformed_messages(
        PaymentProcessedEvent message)
    {
        await using var fixture = new SqliteCatalogFixture();
        await using var context = fixture.CreateContext();
        var consumer = CreateConsumer(context);

        await Assert.ThrowsAsync<InvalidPaymentProcessedMessageException>(() =>
            consumer.ConsumeAsync(
                message,
                TestContext.Current.CancellationToken));
    }

    public static TheoryData<PaymentProcessedEvent> InvalidMessages()
    {
        var validOrderId = Guid.NewGuid();
        var validUserId = Guid.NewGuid();
        var validGameId = Guid.NewGuid();
        var processedAtUtc = DateTime.UtcNow;

        return
        [
            new PaymentProcessedEvent(
                Guid.Empty,
                processedAtUtc,
                validUserId,
                "user@example.com",
                validGameId,
                59.90m,
                PaymentStatuses.Approved),
            new PaymentProcessedEvent(
                validOrderId,
                default,
                validUserId,
                "user@example.com",
                validGameId,
                59.90m,
                PaymentStatuses.Approved),
            new PaymentProcessedEvent(
                validOrderId,
                processedAtUtc,
                Guid.Empty,
                "user@example.com",
                validGameId,
                59.90m,
                PaymentStatuses.Approved),
            new PaymentProcessedEvent(
                validOrderId,
                processedAtUtc,
                validUserId,
                "not-an-email",
                validGameId,
                59.90m,
                PaymentStatuses.Approved),
            new PaymentProcessedEvent(
                validOrderId,
                processedAtUtc,
                validUserId,
                "user@example.com",
                Guid.Empty,
                59.90m,
                PaymentStatuses.Approved),
            new PaymentProcessedEvent(
                validOrderId,
                processedAtUtc,
                validUserId,
                "user@example.com",
                validGameId,
                0,
                PaymentStatuses.Approved),
            new PaymentProcessedEvent(
                validOrderId,
                processedAtUtc,
                validUserId,
                "user@example.com",
                validGameId,
                59.90m,
                "Pending")
        ];
    }

    private static PaymentProcessedConsumer CreateConsumer(
        FCG.Catalog.Infrastructure.Persistence.CatalogDbContext context)
    {
        var library = new UserLibraryService(
            new GameRepository(context),
            new UserGameRepository(context),
            context,
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 8, 12, 0, 0, TimeSpan.Zero)));

        return new PaymentProcessedConsumer(
            library,
            NullLogger<PaymentProcessedConsumer>.Instance);
    }
}
