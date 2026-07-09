using FCG.Catalog.Api.Messaging;
using FCG.Catalog.Api.Services;
using FCG.Catalog.Domain.Common;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Persistence.Repositories;
using FCG.Catalog.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace FCG.Catalog.Tests.Services;

public sealed class PurchaseServiceTests
{
    [Fact]
    public async Task RequestPurchaseAsync_publishes_order_with_catalog_price()
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

        var publisher = new RecordingOrderPlacedPublisher();
        var service = CreateService(context, publisher);
        var userId = Guid.NewGuid();

        var response = await service.RequestPurchaseAsync(
            userId,
            "user@example.com",
            game.Id,
            cancellationToken);

        var message = Assert.Single(publisher.Messages);
        Assert.Equal(response.OrderId, message.OrderId);
        Assert.Equal(userId, message.UserId);
        Assert.Equal("user@example.com", message.UserEmail);
        Assert.Equal(game.Id, message.GameId);
        Assert.Equal(59.90m, message.Price);
        Assert.Equal("Processing", response.Status);
    }

    [Fact]
    public async Task RequestPurchaseAsync_rejects_game_already_in_library()
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
        var userId = Guid.NewGuid();
        await context.Games.AddAsync(game, cancellationToken);
        await context.UserGames.AddAsync(UserGame.Create(
            userId,
            game.Id,
            Guid.NewGuid(),
            DateTime.UtcNow), cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var service = CreateService(context, new RecordingOrderPlacedPublisher());

        await Assert.ThrowsAsync<DomainConflictException>(() =>
            service.RequestPurchaseAsync(
                userId,
                "user@example.com",
                game.Id,
                cancellationToken));
    }

    private static PurchaseService CreateService(
        FCG.Catalog.Infrastructure.Persistence.CatalogDbContext context,
        IOrderPlacedPublisher publisher) =>
        new(
            new GameRepository(context),
            new UserGameRepository(context),
            publisher,
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 8, 12, 0, 0, TimeSpan.Zero)),
            NullLogger<PurchaseService>.Instance);
}
