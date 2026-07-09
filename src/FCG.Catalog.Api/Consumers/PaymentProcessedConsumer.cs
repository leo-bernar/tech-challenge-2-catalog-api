using FCG.Catalog.Api.Services;
using FCG.IntegrationEvents.V1;
using MassTransit;

namespace FCG.Catalog.Api.Consumers;

public sealed class PaymentProcessedConsumer(
    IUserLibraryService library,
    ILogger<PaymentProcessedConsumer> logger)
    : IConsumer<PaymentProcessedEvent>
{
    public Task Consume(ConsumeContext<PaymentProcessedEvent> context) =>
        ConsumeAsync(context.Message, context.CancellationToken);

    public async Task ConsumeAsync(
        PaymentProcessedEvent message,
        CancellationToken cancellationToken)
    {
        if (string.Equals(
                message.Status,
                PaymentStatuses.Approved,
                StringComparison.OrdinalIgnoreCase))
        {
            await library.AddApprovedGameAsync(
                message.OrderId,
                message.UserId,
                message.GameId,
                cancellationToken);

            logger.LogInformation(
                "Game added to library after approved payment. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                message.OrderId,
                message.UserId,
                message.GameId);

            return;
        }

        if (string.Equals(
                message.Status,
                PaymentStatuses.Rejected,
                StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation(
                "Rejected payment did not change library. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
                message.OrderId,
                message.UserId,
                message.GameId);

            return;
        }

        logger.LogWarning(
            "Payment result has unknown status. OrderId: {OrderId}, Status: {Status}",
            message.OrderId,
            message.Status);
    }
}
